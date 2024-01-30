#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Furion.DataEncryption;
using Furion.FriendlyException;

using Mapster;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

using System.Security.Claims;

using ThingsGateway.Admin.Core.Utils;

namespace ThingsGateway.Admin.Application.Services.Auth;

/// <inheritdoc cref="IAuthService"/>
public class AuthService : IAuthService
{
    private readonly ISimpleCacheService _simpleCacheService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IConfigService _configService;
    private readonly ISysUserService _userService;

    public AuthService(ISimpleCacheService simpleCacheService, IEventPublisher eventPublisher, IConfigService configService,
        ISysUserService userService)
    {
        _simpleCacheService = simpleCacheService;
        _eventPublisher = eventPublisher;
        _configService = configService;
        _userService = userService;
    }

    /// <inheritdoc/>
    public ValidCodeOutput GetCaptchaInfo()
    {
        //生成验证码
        var captchInfo = new Random().Next(1111, 9999).ToString();
        //生成请求号，并将验证码放入redis
        var reqNo = AddValidCodeToRedis(captchInfo);
        //返回验证码和请求号
        return new ValidCodeOutput
        {
            CodeValue = captchInfo,
            ValidCodeReqNo = reqNo
        };
    }

    /// <inheritdoc/>
    public async Task<LoginOutput> LoginAsync(LoginInput input)
    {
        //判断是否有验证码
        var sysBase = await _configService.GetByConfigKeyAsync(CateGoryConst.LOGIN_POLICY, ConfigConst.LOGIN_CAPTCHA_OPEN);
        if (sysBase != null)//如果有这个配置项
        {
            if (sysBase.ConfigValue.ToBoolean())//如果需要验证码
            {
                //如果没填验证码，提示验证码不能为空
                if (string.IsNullOrEmpty(input.ValidCode) || input.ValidCodeReqNo == 0) throw Oops.Bah("验证码不能为空").StatusCode(410);
                ValidValidCode(input.ValidCode, input.ValidCodeReqNo);//校验验证码
            }
        }
        return await LoginCoreAsync(input);
    }

    public async Task<LoginOutput> LoginCoreAsync(LoginInput input)
    {
        var password = CryptogramUtil.Sm2Decrypt(input.Password);//SM2解密
        var loginPolicy = await _configService.GetListByCategoryAsync(CateGoryConst.LOGIN_POLICY);
        BeforeLogin(loginPolicy, input.Account);//登录前校验
        var userInfo = await _userService.GetUserByAccountAsync(input.Account);//获取用户信息
        if (userInfo == null) throw Oops.Bah("用户不存在");//用户不存在
        if (userInfo.Password != password)
        {
            LoginError(loginPolicy, input.Account);//登录错误操作
        }
        var result = await ExecLogin(userInfo, input.Device);// 执行B端登录
        return result;
    }

    /// <inheritdoc/>
    public async Task LoginOutAsync(long verificatId)
    {
        //获取用户信息
        var userinfo = await _userService.GetUserByAccountAsync(UserManager.UserAccount);
        if (userinfo != null)
        {
            var loginEvent = new LoginEvent
            {
                Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
                SysUser = userinfo,
                VerificatId = verificatId
            };
            RemoveTokenFromRedis(loginEvent);//移除verificat
            //发布登出事件总线
            await _eventPublisher.PublishAsync(EventSubscriberConst.LoginOut, loginEvent);
        }
        try
        {
            await App.HttpContext?.SignOutAsync();
            App.HttpContext?.SignoutToSwagger();
        }
        catch
        {
        }
    }

    /// <inheritdoc/>
    public async Task<LoginUserOutput> GetLoginUserAsync()
    {
        var userInfo = await _userService.GetUserByAccountAsync(UserManager.UserAccount);//根据账号获取用户信息
        if (userInfo != null)
        {
            return userInfo.Adapt<LoginUserOutput>();
        }
        return null;
    }

    #region 方法

    /// <summary>
    /// 登录之前执行的方法
    /// </summary>
    /// <param name="loginPolicy"></param>
    /// <param name="userName"></param>
    public void BeforeLogin(List<SysConfig> loginPolicy, string userName)
    {
        var lockTime = loginPolicy.First(x => x.ConfigKey == ConfigConst.LOGIN_ERROR_LOCK).ConfigValue.ToInt();//获取锁定时间
        var errorCount = loginPolicy.First(x => x.ConfigKey == ConfigConst.LOGIN_ERROR_COUNT).ConfigValue.ToInt();//获取错误次数
        var key = SystemConst.Cache_LoginErrorCount + userName;//获取登录错误次数Key值
        var errorCountCache = _simpleCacheService.Get<int>(key);//获取登录错误次数
        if (errorCountCache >= errorCount)
        {
            _simpleCacheService.SetExpire(key, TimeSpan.FromMinutes(lockTime));//设置缓存
            throw Oops.Bah($"密码错误次数过多，请{lockTime}分钟后再试");
        }
    }

    /// <summary>
    /// 登录错误操作
    /// </summary>
    /// <param name="loginPolicy"></param>
    /// <param name="userName"></param>
    public void LoginError(List<SysConfig> loginPolicy, string userName)
    {
        var resetTime = loginPolicy.First(x => x.ConfigKey == ConfigConst.LOGIN_ERROR_RESET_TIME).ConfigValue.ToInt();//获取重置时间
        var lockTime = loginPolicy.First(x => x.ConfigKey == ConfigConst.LOGIN_ERROR_LOCK).ConfigValue.ToInt();//获取锁定时间
        var errorCount = loginPolicy.First(x => x.ConfigKey == ConfigConst.LOGIN_ERROR_COUNT).ConfigValue.ToInt();//获取错误次数
        var key = SystemConst.Cache_LoginErrorCount + userName;//获取登录错误次数Key值
        _simpleCacheService.Increment(key, 1);// 登录错误次数+1
        _simpleCacheService.SetExpire(key, TimeSpan.FromMinutes(resetTime));//设置过期时间
        var errorCountCache = _simpleCacheService.Get<int>(key);//获取登录错误次数
        throw Oops.Bah($"账号密码错误密码错误，超过{errorCount}次后将锁定{lockTime}分钟，错误次数{errorCountCache}");//账号密码错误
    }

    /// <summary>
    /// 校验验证码方法
    /// </summary>
    /// <param name="validCode">验证码</param>
    /// <param name="validCodeReqNo">请求号</param>
    /// <param name="isDelete">是否从Redis删除</param>
    public void ValidValidCode(string validCode, long validCodeReqNo, bool isDelete = true)
    {
        var key = SystemConst.Cache_Captcha + validCodeReqNo;//获取验证码Key值
        var code = _simpleCacheService.Get<string>(key);//从redis拿数据
        if (isDelete) RemoveValidCodeFromRedis(validCodeReqNo);//如果需要删除验证码
        if (code != null && validCode != null)//如果有
        {
            //验证码如果不匹配直接抛错误，这里忽略大小写
            if (validCode.ToLower() != code.ToLower()) throw Oops.Bah("验证码错误");
        }
        else
        {
            throw Oops.Bah("验证码不能为空");//抛出验证码不能为空
        }
    }

    /// <summary>
    /// 从Redis中删除验证码
    /// </summary>
    /// <param name="validCodeReqNo"></param>
    public void RemoveValidCodeFromRedis(long validCodeReqNo)
    {
        var key = SystemConst.Cache_Captcha + validCodeReqNo;//获取验证码Key值
        _simpleCacheService.Remove(key);//删除验证码
    }

    /// <summary>
    /// 添加验证码到redis
    /// </summary>
    /// <param name="code">验证码</param>
    /// <param name="expire">过期时间</param>
    /// <returns>验证码请求号</returns>
    public long AddValidCodeToRedis(string code, int expire = 5)
    {
        //生成请求号，并将验证码放入cache
        var reqNo = CommonUtils.GetSingleId();
        //插入redis
        _simpleCacheService.Set(SystemConst.Cache_Captcha + reqNo, code, TimeSpan.FromMinutes(expire));
        return reqNo;
    }

    /// <summary>
    /// 执行登录
    /// </summary>
    /// <param name="sysUser">用户信息</param>
    /// <param name="device">登录设备</param>
    /// <param name="loginClientType">登录类型</param>
    /// <returns></returns>
    public async Task<LoginOutput> ExecLogin(SysUser sysUser, AuthDeviceTypeEnum device)
    {
        if (sysUser.UserStatus == false) throw Oops.Bah("账号已停用");//账号已停用
        var verificatId = CommonUtils.GetSingleId();
        var sysBase = await _configService.GetByConfigKeyAsync(CateGoryConst.LOGIN_POLICY, ConfigConst.LOGIN_VERIFICAT_EXPIRES);
        var expire = sysBase.ConfigValue.ToInt(2880);
        if (device == AuthDeviceTypeEnum.Api)
        {
            #region Token

            //生成Token
            var accessToken = JWTEncryption.Encrypt(new Dictionary<string, object>
        {
            {
                ClaimConst.UserId, sysUser.Id
            },
            {
                ClaimConst.Account, sysUser.Account
            },
            {
                ClaimConst.SuperAdmin, sysUser.RoleCodeList.Contains(RoleConst.SuperAdmin)
            },
            {
                ClaimConst.VerificatId, verificatId
            }
        });
            // 生成刷新Token令牌
            var refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, expire * 2);
            // 设置Swagger自动登录
            App.HttpContext.SigninToSwagger(accessToken);
            // 设置响应报文头
            App.HttpContext.SetTokensOfResponseHeaders(accessToken, refreshToken);

            #endregion Token
        }
        else
        {
            #region cookie

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimConst.VerificatId, verificatId.ToString()));
            identity.AddClaim(new Claim(ClaimConst.UserId, sysUser.Id.ToString()));
            identity.AddClaim(new Claim(ClaimConst.Account, sysUser.Account));
            identity.AddClaim(new Claim(ClaimConst.SuperAdmin, sysUser.RoleCodeList.Contains(RoleConst.SuperAdmin).ToString()));

            var diffTime = DateTime.Now.AddMinutes(expire);
            await App.HttpContext.SignInAsync(new ClaimsPrincipal(identity), new AuthenticationProperties()
            {
                IsPersistent = true,
                ExpiresUtc = diffTime,
            });

            #endregion cookie
        }

        //登录事件参数
        var logingEvent = new LoginEvent
        {
            Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
            Device = device,
            Expire = expire,
            SysUser = sysUser,
            VerificatId = verificatId
        };
        await WriteTokenToRedis(logingEvent);//写入verificat到redis
        await _eventPublisher.PublishAsync(EventSubscriberConst.Login, logingEvent);//发布登录事件总线
        //返回结果
        return new LoginOutput
        {
            VerificatId = verificatId,
            Account = sysUser.Account,
            Id = sysUser.Id,
        };
    }

    /// <summary>
    /// 写入用户verificat到redis
    /// </summary>
    /// <param name="loginEvent">登录事件参数</param>
    /// <param name="loginClientType">登录类型</param>
    private async Task WriteTokenToRedis(LoginEvent loginEvent)
    {
        //获取verificat列表
        var verificatInfos = GetTokenInfos(loginEvent.SysUser.Id);
        var tokenTimeout = loginEvent.DateTime.AddMinutes(loginEvent.Expire);
        //生成verificat信息
        var verificatInfo = new VerificatInfo
        {
            Device = loginEvent.Device.ToString(),
            Expire = loginEvent.Expire,
            VerificatTimeout = tokenTimeout,
            Id = loginEvent.VerificatId,
        };
        //如果redis有数据
        if (verificatInfos != null)
        {
            var isSingle = false;//默认不开启单用户登录
            var singleConfig = await _configService.GetByConfigKeyAsync(CateGoryConst.LOGIN_POLICY, ConfigConst.LOGIN_SINGLE_OPEN);//获取系统单用户登录选项
            if (singleConfig != null) isSingle = singleConfig.ConfigValue.ToBoolean();//如果配置不为空则设置单用户登录选项为系统配置的值
            //判断是否单用户登录
            if (isSingle)
            {
                await SingleLogin(loginEvent.SysUser.Id, verificatInfos);//单用户登录方法
                verificatInfos = new();
                verificatInfos.Add(verificatInfo);//添加到列表
            }
            else
            {
                verificatInfos.Add(verificatInfo);
            }
        }
        else
        {
            verificatInfos = new List<VerificatInfo>
            {
                verificatInfo
            };//直接就一个
        }

        //添加到verificat列表
        UserTokenCacheUtil.HashAdd(loginEvent.SysUser.Id, verificatInfos);
    }

    /// <summary>
    /// redis删除用户verificat
    /// </summary>
    /// <param name="loginEvent">登录事件参数</param>
    /// <param name="loginClientType">登录类型</param>
    private void RemoveTokenFromRedis(LoginEvent loginEvent)
    {
        //获取verificat列表
        var verificatInfos = GetTokenInfos(loginEvent.SysUser.Id);
        if (verificatInfos != null)
        {
            //获取当前用户的verificat
            var verificat = verificatInfos.Where(it => it.Id == loginEvent.VerificatId).FirstOrDefault();
            if (verificat != null)
                verificatInfos.Remove(verificat);
            if (verificatInfos.Count > 0)
            {
                //更新verificat列表
                UserTokenCacheUtil.HashAdd(loginEvent.SysUser.Id, verificatInfos);
            }
            else
            {
                //从列表中删除
                UserTokenCacheUtil.HashDel(loginEvent.SysUser.Id);
            }
        }
    }

    /// <summary>
    /// 获取用户verificat列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>verificat列表</returns>
    private List<VerificatInfo> GetTokenInfos(long userId)
    {
        //redis获取用户verificat列表

        var verificatInfos = UserTokenCacheUtil.HashGetOne(userId);
        if (verificatInfos != null)
        {
            verificatInfos = verificatInfos.Where(it => it.VerificatTimeout > DateTime.Now).ToList();//去掉登录超时的
        }
        return verificatInfos;
    }

    /// <summary>
    /// 单用户登录通知用户下线
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="verificatInfos">Token列表</param>
    private async Task SingleLogin(long userId, List<VerificatInfo> verificatInfos)
    {
        await _eventPublisher.PublishAsync(EventSubscriberConst.UserLoginOut, new UserLoginOutEvent
        {
            Message = "您的账号已在别处登录!",
            VerificatInfos = verificatInfos,
            UserId = userId
        });//通知用户下线
    }

    #endregion 方法
}
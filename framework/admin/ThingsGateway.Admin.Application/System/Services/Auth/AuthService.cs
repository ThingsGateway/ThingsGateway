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
using Furion.EventBus;
using Furion.FriendlyException;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using System.Security.Claims;

using ThingsGateway.Foundation.Extension.String;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IAuthService"/>
public class AuthService : IAuthService
{
    private readonly IConfigService _configService;
    private readonly IEventPublisher _eventPublisher;
    private readonly INoticeService _noticeService;
    private readonly IServiceScope _serviceScope;
    private readonly ISysUserService _userService;
    private readonly IVerificatService _verificatService;
    /// <inheritdoc cref="IAuthService"/>
    public AuthService(
                       IEventPublisher eventPublisher,
                       ISysUserService userService,
                       IConfigService configService,
                       IVerificatService verificatService,
                        INoticeService noticeService, IServiceScopeFactory serviceScopeFactory
        )
    {
        _eventPublisher = eventPublisher;
        _userService = userService;
        _configService = configService;
        _verificatService = verificatService;
        _noticeService = noticeService;
        _serviceScope = serviceScopeFactory.CreateScope();
    }

    /// <inheritdoc/>
    public ValidCodeOutput GetCaptchaInfo()
    {
        //生成验证码
        var captchInfo = new Random().Next(1111, 9999).ToString();
        //生成请求号，并将验证码放入cache
        var reqNo = YitIdHelper.NextId();
        //插入cache
        _serviceScope.ServiceProvider.GetService<MemoryCache>().Set(CacheConst.LOGIN_CAPTCHA + reqNo, captchInfo, TimeSpan.FromMinutes(1), false);
        //返回验证码和请求号
        return new ValidCodeOutput { CodeValue = captchInfo, ValidCodeReqNo = reqNo };
    }

    /// <inheritdoc/>
    public async Task<SysUser> GetLoginUserAsync()
    {
        return await _userService.GetUserByIdAsync(UserManager.UserId);
    }

    /// <inheritdoc/>
    public async Task<LoginOutput> LoginAsync(LoginInput input)
    {
        //判断是否有验证码
        var sysBase = await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_CAPTCHA_OPEN);

        if (sysBase != null)//如果有这个配置项
        {
            if (sysBase.ConfigValue.ToBool(false))//如果需要验证码
            {
                //如果没填验证码，提示验证码不能为空
                if (input.ValidCode.IsNullOrEmpty() || input.ValidCodeReqNo == 0) throw Oops.Bah("验证码不能为空").StatusCode(410);
                ValidValidCode(input.ValidCode, input.ValidCodeReqNo);//校验验证码
            }
        }

        var password = DESCEncryption.Decrypt(input.Password, DESCKeyConst.DESCKey);  // 解密
        var userInfo = await _userService.GetUserByAccountAsync(input.Account) ?? throw Oops.Bah("用户不存在");//获取用户信息
        if (userInfo.Password != password) throw Oops.Bah("账号密码错误");//账号密码错误
        return await LoginAsync(userInfo, input.Device);
    }

    /// <inheritdoc/>
    public async Task LogoutAsync()
    {
        //获取用户信息
        var userinfo = await _userService.GetUserByAccountAsync(UserManager.UserAccount);
        if (userinfo != null)
        {
            LoginEvent loginEvent = new()
            {
                Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
                SysUser = userinfo,
                VerificatId = UserManager.VerificatId.ToLong(),
            };
            await RemoveVerificatAsync(loginEvent);//移除验证Id
        }
    }

    /// <summary>
    /// 校验验证码方法
    /// </summary>
    /// <param name="validCode">验证码</param>
    /// <param name="validCodeReqNo">请求号</param>
    /// <param name="isDelete">是否从Cache删除</param>
    private void ValidValidCode(string validCode, long validCodeReqNo, bool isDelete = true)
    {
        var code = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<string>(CacheConst.LOGIN_CAPTCHA + validCodeReqNo, false);//从cache拿数据
        if (isDelete) _serviceScope.ServiceProvider.GetService<MemoryCache>().Remove(CacheConst.LOGIN_CAPTCHA + validCodeReqNo);//删除验证码
        if (code != null)//如果有
        {
            //验证码如果不匹配直接抛错误，这里忽略大小写
            if (validCode.ToLower() != code.ToLower()) throw Oops.Bah("验证码错误");
        }
        else
        {
            throw Oops.Bah("验证码已过期");
        }
    }

    /// <summary>
    /// 执行B端登录
    /// </summary>
    /// <param name="sysUser">用户信息</param>
    /// <param name="device">登录设备</param>
    /// <returns></returns>
    private async Task<LoginOutput> LoginAsync(SysUser sysUser, AuthDeviceTypeEnum device)
    {
        if (sysUser.UserEnable == false) throw Oops.Bah("账号已停用");//账号已停用

        var sysBase = await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_VERIFICAT_EXPIRES);
        var sessionid = YitIdHelper.NextId();
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimConst.VerificatId, sessionid.ToString()));
        identity.AddClaim(new Claim(ClaimConst.UserId, sysUser.Id.ToString()));
        identity.AddClaim(new Claim(ClaimConst.Account, sysUser.Account));
        identity.AddClaim(new Claim(ClaimConst.IsSuperAdmin, sysUser.RoleCodeList.Contains(RoleConst.SuperAdmin).ToString()));
        identity.AddClaim(new Claim(ClaimConst.IsOpenApi, false.ToString()));

        var config = sysBase.ConfigValue.ToInt(2880);
        var diffTime = DateTimeExtensions.CurrentDateTime.AddMinutes(config);
        await App.HttpContext.SignInAsync(new ClaimsPrincipal(identity), new AuthenticationProperties()
        {
            IsPersistent = true,
            ExpiresUtc = diffTime,
        });

        //登录事件参数
        var loginEvent = new LoginEvent
        {
            Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
            Device = device,
            Expire = config,
            SysUser = sysUser,
            VerificatId = sessionid,
        };

        await SetVerificatAsync(loginEvent);//写入verificat

        await _eventPublisher.PublishAsync(EventSubscriberConst.Login, loginEvent); //发布登录事件总线
        return new LoginOutput { VerificatId = sessionid, Account = sysUser.Account };
    }


    private async Task RemoveVerificatAsync(LoginEvent loginEvent)
    {
        //获取verificat列表
        List<VerificatInfo> verificatInfos = await _verificatService.GetVerificatIdAsync(loginEvent.SysUser.Id);
        if (verificatInfos != null)
        {
            //获取当前用户的verificat
            var verificat = verificatInfos.Where(it => it.Id == loginEvent.VerificatId).FirstOrDefault();
            if (verificat != null)
                verificatInfos.Remove(verificat);
            //更新verificat列表
            await _verificatService.SetVerificatIdAsync(loginEvent.SysUser.Id, verificatInfos);
        }
        await App.HttpContext?.SignOutAsync();
        App.HttpContext?.SignoutToSwagger();
    }

    /// <summary>
    /// 写入验证信息到缓存
    /// </summary>
    /// <param name="loginEvent"></param>
    /// <returns></returns>
    private async Task SetVerificatAsync(LoginEvent loginEvent)
    {
        //获取verificat列表
        List<VerificatInfo> verificatInfos = await _verificatService.GetVerificatIdAsync(loginEvent.SysUser.Id);
        var verificatTimeout = loginEvent.DateTime.AddMinutes(loginEvent.Expire);
        //生成verificat信息
        var verificatInfo = new VerificatInfo
        {
            Device = loginEvent.Device.ToString(),
            Expire = loginEvent.Expire,
            VerificatTimeout = verificatTimeout,
            Id = loginEvent.VerificatId,
            UserId = loginEvent.SysUser.Id,
        };
        if (verificatInfos != null)
        {
            bool isSingle = false;//默认不开启单用户登录

            var singleConfig = await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_SINGLE_OPEN);//获取系统单用户登录选项
            if (singleConfig != null) isSingle = singleConfig.ConfigValue.ToBool(false);//如果配置不为空则设置单用户登录选项为系统配置的值
            if (isSingle)//判断是否单用户登录
            {
                await _noticeService.LogoutAsync(loginEvent.SysUser.Id, verificatInfos.Where(it => it.Device == loginEvent.Device.ToString()).ToList(), "该账号已在别处登录!");//通知其他用户下线
                verificatInfos = verificatInfos.Where(it => it.Device != loginEvent.Device.ToString()).ToList();//去掉当前登录类型
                verificatInfos.Add(verificatInfo);//添加到列表
            }
            else
            {
                verificatInfos.Add(verificatInfo);
            }
        }
        else
        {
            verificatInfos = new List<VerificatInfo> { verificatInfo };//直接就一个
        }

        //添加到verificat列表
        await _verificatService.SetVerificatIdAsync(loginEvent.SysUser.Id, verificatInfos);
    }
}
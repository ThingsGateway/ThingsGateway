//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

using SqlSugar;

using System.Security.Claims;

using ThingsGateway.DataEncryption;
using ThingsGateway.FriendlyException;

namespace ThingsGateway.Admin.Application;

public class AuthService : IAuthService
{
    private readonly ISysDictService _configService;
    private readonly ISysResourceService _sysResourceService;
    private readonly IUserCenterService _userCenterService;
    private readonly ISysUserService _sysUserService;
    private readonly ISysOrgService _sysOrgService;
    private IStringLocalizer<AuthService> _localizer;
    private IVerificatInfoService _verificatInfoService;
    private IAppService _appService;

    public AuthService(ISysDictService configService, ISysResourceService sysResourceService,
        ISysUserService userService, IUserCenterService userCenterService,
         ISysOrgService sysOrgService,
        IVerificatInfoService verificatInfoService, IAppService appService,
        IStringLocalizer<AuthService> localizer)
    {
        _sysOrgService = sysOrgService;
        _configService = configService;
        _appService = appService;
        _sysUserService = userService;
        _sysResourceService = sysResourceService;
        _userCenterService = userCenterService;
        _localizer = localizer;
        _verificatInfoService = verificatInfoService;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="input">登录参数</param>
    /// <param name="isCookie">cookie方式登录</param>
    /// <returns>登录输出</returns>
    public async Task<LoginOutput> LoginAsync(LoginInput input, bool isCookie = true)
    {
        var appConfig = await _configService.GetAppConfigAsync().ConfigureAwait(false);

        //判断是否开启web访问
        if (!appConfig.WebsitePolicy.WebStatus
            && input.Account != RoleConst.SuperAdmin)//如果禁用了网站并且不是超级管理员
        {
            throw Oops.Bah(appConfig.WebsitePolicy.CloseTip);
        }
        string? password = input.Password;
        if (isCookie) //openApi登录不再需要解密
        {
            try
            {
                password = DESEncryption.Decrypt(input.Password);//解密
            }
            catch (Exception)
            {
                throw Oops.Bah(_localizer["MustDesc"]);
            }
        }

        await BeforeLoginAsync(appConfig, input).ConfigureAwait(false);//登录前校验

        var userInfo = await _sysUserService.GetUserByAccountAsync(input.Account, input.TenantId).ConfigureAwait(false);//获取用户信息
        if (userInfo == null)
            throw Oops.Bah(_localizer["UserNull", input.Account]);//用户不存在

        if (userInfo.Password != password)
        {
            LoginError(appConfig.LoginPolicy, input.Account);//登录错误操作
        }
        var result = await ExecLogin(appConfig.LoginPolicy, input, userInfo, isCookie).ConfigureAwait(false);// 执行登录
        return result;
    }

    /// <summary>
    /// 注销当前用户
    /// </summary>
    public async Task LoginOutAsync()
    {
        if (UserManager.UserId == 0)
            return;
        var verificatId = UserManager.UserId;
        //获取用户信息
        var userinfo = await _sysUserService.GetUserByAccountAsync(UserManager.UserAccount, UserManager.TenantId).ConfigureAwait(false);
        if (userinfo != null)
        {
            var loginEvent = new LoginEvent
            {
                Ip = _appService.RemoteIpAddress?.MapToIPv4()?.ToString(),
                SysUser = userinfo,
                VerificatId = verificatId
            };
            RemoveTokenFromCache(loginEvent);//移除verificat
        }
        await _appService.LoginOutAsync().ConfigureAwait(false);
    }

    #region 方法

    /// <summary>
    /// 登录之前执行的方法
    /// </summary>
    /// <param name="appConfig">配置</param>
    /// <param name="input">input</param>
    private async Task BeforeLoginAsync(AppConfig appConfig, LoginInput input)
    {
        var tenantEnable = App.GetOptions<TenantOptions>()?.Enable ?? false;

        if (tenantEnable)
        {
            //如果租户ID为空表示用域名登录
            if (input.TenantId == null)
            {
                //获取域名
                var origin = App.HttpContext.Request.Headers["Origin"].ToString();
                // 如果Origin头不存在，可以尝试使用Referer头作为备选
                if (string.IsNullOrEmpty(origin))
                    origin = App.HttpContext.Request.Headers["Referer"].ToString();
                //根据域名获取二级域名
                var domain = origin.Split("//")[1].Split(".")[0];
                //根据二级域名获取租户
                var tenantList = await _sysOrgService.GetTenantListAsync().ConfigureAwait(false);
                var tenant = tenantList.FirstOrDefault(x => x.Code.Equals(domain, StringComparison.OrdinalIgnoreCase));//获取租户默认是机构编码
                if (tenant != null)
                    input.TenantId = tenant.Id;
                else
                    input.TenantId = RoleConst.DefaultTenantId;
            }
        }
        else
        {
            input.TenantId = RoleConst.DefaultTenantId;
        }

        var key = CacheConst.Cache_LoginErrorCount + input.Account + input.TenantId;//获取登录错误次数Key值
        var errorCountCache = App.CacheService.Get<int>(key);//获取登录错误次数

        if (errorCountCache >= appConfig.LoginPolicy.ErrorCount)
        {
            App.CacheService.SetExpire(key, TimeSpan.FromMinutes(appConfig.LoginPolicy.ErrorLockTime));//设置缓存
            throw Oops.Bah(_localizer["PasswordError", appConfig.LoginPolicy.ErrorLockTime]);
        }
    }

    /// <summary>
    /// 执行登录
    /// </summary>
    /// <param name="loginPolicy">登录策略</param>
    /// <param name="input">用户登录参数</param>
    /// <param name="sysUser">用户信息</param>
    /// <param name="isCookie">cookie方式登录</param>
    /// <returns>登录输出结果</returns>
    private async Task<LoginOutput> ExecLogin(LoginPolicy loginPolicy, LoginInput input, SysUser sysUser, bool isCookie = true)
    {
        if (sysUser.Status == false)
            throw Oops.Bah(_localizer["UserDisable", sysUser.Account]);//账号已停用

        var verificatId = CommonUtils.GetSingleId();
        var expire = loginPolicy.VerificatExpireTime;
        string accessToken = string.Empty;
        string refreshToken = string.Empty;
        if (!isCookie)
        {
            #region Token

            //生成Token
            accessToken = JWTEncryption.Encrypt(new Dictionary<string, object>
        {
            {
                ClaimConst.UserId, sysUser.Id
            },
            {
                ClaimConst.Account, sysUser.Account
            },
            {
                ClaimConst.SuperAdmin, sysUser.RoleIdList.Contains(RoleConst.SuperAdminRoleId)
            },
                                {
                ClaimConst.VerificatId, verificatId
            },
            {
                ClaimConst.OrgId, sysUser.OrgId
            },
             {
                ClaimConst.TenantId, input.TenantId
            }
        });
            // 生成刷新Token令牌
            refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, expire * 2);
            // 设置Swagger自动登录
            App.HttpContext?.SigninToSwagger(accessToken);
            // 设置响应报文头
            App.HttpContext?.SetTokensOfResponseHeaders(accessToken, refreshToken);

            #endregion Token
        }
        else
        {
            if (sysUser.ModuleList.Count == 0)
                throw Oops.Bah(_localizer["UserNoModule"]);//未分配模块
            var org = await _sysOrgService.GetSysOrgByIdAsync(sysUser.OrgId).ConfigureAwait(false);//获取机构
            if (!org.Status) throw Oops.Bah(_localizer["OrgDisable"]);//机构冻结
            #region cookie

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimConst.VerificatId, verificatId.ToString()));
            identity.AddClaim(new Claim(ClaimConst.UserId, sysUser.Id.ToString()));
            identity.AddClaim(new Claim(ClaimConst.Account, sysUser.Account));
            identity.AddClaim(new Claim(ClaimConst.SuperAdmin, sysUser.RoleIdList.Contains(RoleConst.SuperAdminRoleId).ToString()));
            identity.AddClaim(new Claim(ClaimConst.OrgId, sysUser.OrgId.ToString()));
            identity.AddClaim(new Claim(ClaimConst.TenantId, input.TenantId?.ToString() ?? "0"));

            await _appService.LoginAsync(identity).ConfigureAwait(false);

            #endregion cookie
        }
        //登录事件参数
        var logingEvent = new LoginEvent
        {
            Ip = _appService.RemoteIpAddress?.MapToIPv4()?.ToString(),
            Device = input.Device?.ToString(),
            Expire = expire,
            SysUser = sysUser,
            VerificatId = verificatId
        };
        await WriteTokenToCache(loginPolicy, logingEvent).ConfigureAwait(false);//写入verificat到cache
        await UpdateUser(logingEvent).ConfigureAwait(false);
        if (sysUser.Account == RoleConst.SuperAdmin)
        {
            var modules = (await _sysResourceService.GetAllAsync().ConfigureAwait(false)).Where(a => a.Category == ResourceCategoryEnum.Module);//获取模块列表
            sysUser.ModuleList = modules.ToList();//模块列表赋值给用户
        }
        //返回结果
        return new LoginOutput
        {
            VerificatId = verificatId,
            Account = sysUser.Account,
            Id = sysUser.Id,
            ModuleList = sysUser.ModuleList,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    /// <summary>
    /// 登录错误反馈
    /// </summary>
    /// <param name="loginPolicy">登录策略</param>
    /// <param name="userName">用户名称</param>
    private void LoginError(LoginPolicy loginPolicy, string userName)
    {
        var key = CacheConst.Cache_LoginErrorCount + userName;//获取登录错误次数Key值
        App.CacheService.Increment(key, 1);// 登录错误次数+1
        App.CacheService.SetExpire(key, TimeSpan.FromMinutes(loginPolicy.ErrorResetTime));//设置过期时间
        var errorCountCache = App.CacheService.Get<int>(key);//获取登录错误次数
        throw Oops.Bah(_localizer["AuthErrorMax", loginPolicy.ErrorCount, loginPolicy.ErrorLockTime, errorCountCache]);//账号密码错误
    }

    /// <summary>
    /// 从cache删除用户verificat
    /// </summary>
    /// <param name="loginEvent">登录事件参数</param>
    private void RemoveTokenFromCache(LoginEvent loginEvent)
    {
        //更新verificat列表
        _verificatInfoService.Delete(loginEvent.VerificatId);
    }

    /// <summary>
    /// 单用户登录通知用户下线
    /// </summary>
    /// <param name="userId">用户Id</param>
    private async Task SingleLogin(long userId)
    {
        var clientIds = _verificatInfoService.GetClientIdListByUserId(userId);
        await NoticeUtil.UserLoginOut(new UserLoginOutEvent
        {
            Message = _localizer["SingleLoginWarn"],
            ClientIds = clientIds,
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// 登录事件
    /// </summary>
    /// <param name="loginEvent"></param>
    /// <returns></returns>
    private async Task UpdateUser(LoginEvent loginEvent)
    {
        var sysUser = loginEvent.SysUser;

        #region 登录/密码策略

        var key = CacheConst.Cache_LoginErrorCount + sysUser.Account;//获取登录错误次数Key值
        App.CacheService.Remove(key);//移除登录错误次数

        //获取用户verificat列表
        var userToken = _verificatInfoService.GetOne(loginEvent.VerificatId);

        #endregion 登录/密码策略

        #region 重新赋值属性,设置本次登录信息为最新的信息

        sysUser.LastLoginIp = sysUser.LatestLoginIp;
        sysUser.LastLoginTime = sysUser.LatestLoginTime;
        sysUser.LatestLoginIp = loginEvent.Ip;
        sysUser.LatestLoginTime = loginEvent.DateTime;

        #endregion 重新赋值属性,设置本次登录信息为最新的信息

        using var db = DbContext.Db.GetConnectionScopeWithAttr<SysUser>().CopyNew();
        //更新用户登录信息
        if (await db.Updateable(sysUser).UpdateColumns(it => new
        {
            it.LastLoginIp,
            it.LastLoginTime,
            it.LatestLoginIp,
            it.LatestLoginTime,
        }).ExecuteCommandAsync().ConfigureAwait(false) > 0)
            App.CacheService.HashAdd(CacheConst.Cache_SysUser, sysUser.Id.ToString(), sysUser);//更新Cache信息
    }

    /// <summary>
    /// 写入用户verificat到cache
    /// </summary>
    /// <param name="loginPolicy">登录策略</param>
    /// <param name="loginEvent">登录事件参数</param>
    private async Task WriteTokenToCache(LoginPolicy loginPolicy, LoginEvent loginEvent)
    {
        //获取verificat列表
        var tokenTimeout = loginEvent.DateTime.AddMinutes(loginEvent.Expire);
        //生成verificat信息
        var verificatInfo = new VerificatInfo
        {
            Device = loginEvent.Device,
            Expire = loginEvent.Expire,
            VerificatTimeout = tokenTimeout,
            Id = loginEvent.VerificatId,
            UserId = loginEvent.SysUser.Id,
            LoginIp = loginEvent.Ip,
            LoginTime = loginEvent.DateTime
        };
        //判断是否单用户登录
        if (loginPolicy.SingleOpen)
        {
            await SingleLogin(loginEvent.SysUser.Id).ConfigureAwait(false);//单用户登录方法
        }

        //添加到verificat列表
        _verificatInfoService.Add(verificatInfo);
    }

    #endregion 方法
}

/// <summary>
/// 登录事件参数
/// </summary>
public class LoginEvent
{
    /// <summary>
    /// 时间
    /// </summary>
    public DateTime DateTime = DateTime.Now;

    /// <summary>
    /// 过期时间
    /// </summary>
    public int Expire { get; set; }

    /// <summary>
    /// Ip地址
    /// </summary>
    public string? Ip { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public SysUser SysUser { get; set; }

    /// <summary>
    /// VerificatId
    /// </summary>
    public long VerificatId { get; set; }

    /// <summary>
    /// 登录设备
    /// </summary>
    public string Device { get; set; }
}

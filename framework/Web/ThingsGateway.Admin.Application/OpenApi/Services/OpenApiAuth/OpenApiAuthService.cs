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

using Furion;
using Furion.DataEncryption;
using Furion.DependencyInjection;
using Furion.EventBus;
using Furion.FriendlyException;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

using ThingsGateway.Foundation.Extension.String;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IOpenApiAuthService"/>
public class OpenApiAuthService : IOpenApiAuthService, ITransient
{
    private readonly IConfigService _configService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IOpenApiUserService _openApiUserService;
    private readonly IVerificatService _verificatService;

    /// <inheritdoc cref="IOpenApiAuthService"/>
    public OpenApiAuthService(
                       IEventPublisher eventPublisher,
                       IOpenApiUserService openApiUserService,
                       IVerificatService verificatService,
                       IConfigService configService)
    {
        _verificatService = verificatService;
        _eventPublisher = eventPublisher;
        _openApiUserService = openApiUserService;
        _configService = configService;
    }

    /// <inheritdoc/>
    public async Task<LoginOpenApiOutput> LoginOpenApiAsync(LoginOpenApiInput input)
    {
        var password = input.Password;
        var userInfo = await _openApiUserService.GetUserByAccountAsync(input.Account);//获取用户信息
        if (userInfo == null) throw Oops.Bah("用户不存在");//用户不存在
        if (userInfo.Password != password) throw Oops.Bah("账号密码错误");//账号密码错误
        return await PrivateLoginOpenApiAsync(userInfo);
    }

    /// <inheritdoc/>
    public async Task LogoutAsync()
    {
        //获取用户信息
        var userinfo = await _openApiUserService.GetUserByAccountAsync(UserManager.UserAccount);
        if (userinfo != null)
        {
            LoginOpenApiEvent loginEvent = new()
            {
                Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
                OpenApiUser = userinfo,
                VerificatId = UserManager.VerificatId.ToLong(),
            };
            await RemoveVerificatFromCacheAsync(loginEvent);
        }
    }

    private async Task<List<VerificatInfo>> GetVerificatInfos(long userId)
    {
        List<VerificatInfo> verificatInfos = await _verificatService.GetOpenApiVerificatIdAsync(userId);
        return verificatInfos;
    }

    private async Task<LoginOpenApiOutput> PrivateLoginOpenApiAsync(OpenApiUser openApiUser)
    {
        if (openApiUser.UserEnable == false) throw Oops.Bah("账号已停用");//账号冻结
        var sessionid = YitIdHelper.NextId();
        var expire = (await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_VERIFICAT_EXPIRES)).ConfigValue.ToInt();
        //生成Token
        var accessToken = JWTEncryption.Encrypt(new Dictionary<string, object>
    {
        {ClaimConst.UserId, openApiUser.Id},
        {ClaimConst.Account, openApiUser.Account},
        { ClaimConst.VerificatId, sessionid.ToString()},
        { ClaimConst.IsOpenApi, true},
      }, expire);
        // 生成刷新Token令牌
        var refreshToken = JWTEncryption.GenerateRefreshToken(accessToken, expire * 2);
        // 设置Swagger自动登录
        App.HttpContext.SigninToSwagger(accessToken);
        // 设置响应报文头
        App.HttpContext.SetTokensOfResponseHeaders(accessToken, refreshToken);
        //登录事件参数
        var logingEvent = new LoginOpenApiEvent
        {
            Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
            Device = AuthDeviceTypeEnum.Api,
            Expire = expire,
            OpenApiUser = openApiUser,
            VerificatId = sessionid
        };

        await WriteVerificatToCacheAsync(logingEvent);//写入verificat到cache
        await _eventPublisher.PublishAsync(EventSubscriberConst.LoginOpenApi, logingEvent); //发布登录事件总线
                                                                                            //返回结果
        return new LoginOpenApiOutput { VerificatId = sessionid, Token = accessToken, Account = openApiUser.Account };
    }

    private async Task RemoveVerificatFromCacheAsync(LoginOpenApiEvent loginEvent)
    {
        //获取verificat列表
        var verificatInfos = await GetVerificatInfos(loginEvent.OpenApiUser.Id);
        if (verificatInfos != null)
        {
            //获取当前用户的verificat
            var verificat = verificatInfos.Where(it => it.Id == loginEvent.VerificatId).FirstOrDefault();
            if (verificat != null)
                verificatInfos.Remove(verificat);
            //更新verificat列表
            await _verificatService.SetOpenApiVerificatIdAsync(loginEvent.OpenApiUser.Id, verificatInfos);
        }
        await App.HttpContext?.SignOutAsync();
        App.HttpContext?.SignoutToSwagger();
    }

    private async Task WriteVerificatToCacheAsync(LoginOpenApiEvent loginEvent)
    {
        //获取verificat列表
        List<VerificatInfo> verificatInfos = await GetVerificatInfos(loginEvent.OpenApiUser.Id);
        var verificatTimeout = loginEvent.DateTime.AddMinutes(loginEvent.Expire);
        //生成verificat信息
        var verificatInfo = new VerificatInfo
        {
            Device = loginEvent.Device.ToString(),
            Expire = loginEvent.Expire,
            VerificatTimeout = verificatTimeout,
            Id = loginEvent.VerificatId,
            UserId = loginEvent.OpenApiUser.Id,
        };
        if (verificatInfos != null)
        {
            bool isSingle = false;//默认不开启单用户登录
            var singleConfig = await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_SINGLE_OPEN);//获取系统单用户登录选项
            if (singleConfig != null) isSingle = singleConfig.ConfigValue.ToBoolean();//如果配置不为空则设置单用户登录选项为系统配置的值

            //判断是否单用户登录
            if (isSingle)
            {
                verificatInfos = verificatInfos.ToList();//去掉当前登录类型的verificat
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
        await _verificatService.SetOpenApiVerificatIdAsync(loginEvent.OpenApiUser.Id, verificatInfos);
    }
}
using Furion.DataEncryption;

using Microsoft.AspNetCore.Authentication;

namespace ThingsGateway.Application.Services.Auth
{
    public class OpenApiAuthService : IOpenApiAuthService, ITransient
    {
        private readonly IConfigService _configService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IOpenApiUserService _openApiUserService;
        private readonly SysCacheService _sysCacheService;

        public OpenApiAuthService(
                           IEventPublisher eventPublisher,
                           IOpenApiUserService openApiUserService,
                           SysCacheService sysCacheService,
                           IConfigService configService)
        {
            _sysCacheService = sysCacheService;
            _eventPublisher = eventPublisher;
            _openApiUserService = openApiUserService;
            _configService = configService;
        }

        /// <inheritdoc/>
        public async Task<LoginOpenApiOutPut> LoginOpenApi(LoginOpenApiInput input)
        {
            var password = input.Password;
            var userInfo = await _openApiUserService.GetUserByAccount(input.Account);//获取用户信息
            if (userInfo == null) throw Oops.Bah("用户不存在");//用户不存在
            if (userInfo.Password != password) throw Oops.Bah("账号密码错误");//账号密码错误
            return await PrivateLoginOpenApi(userInfo);
        }

        /// <inheritdoc/>
        public async Task LoginOut()
        {
            //获取用户信息
            var userinfo = await _openApiUserService.GetUserByAccount(UserManager.UserAccount);
            if (userinfo != null)
            {
                LoginOpenApiEvent loginEvent = new LoginOpenApiEvent
                {
                    Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
                    OpenApiUser = userinfo,
                    VerificatId = UserManager.VerificatId.ToLong(),
                };
                await RemoveVerificatFromCache(loginEvent);
            }
        }

        private List<VerificatInfo> GetVerificatInfos(long userId)
        {
            List<VerificatInfo> verificatInfos = _sysCacheService.GetOpenApiVerificatId(userId);
            return verificatInfos;
        }

        private async Task<LoginOpenApiOutPut> PrivateLoginOpenApi(OpenApiUser openApiUser)
        {
            if (openApiUser.UserStatus == false) throw Oops.Bah("账号已停用");//账号冻结
            var sessionid = YitIdHelper.NextId();
            var expire = (await _configService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_VERIFICAT_EXPIRES)).ConfigValue.ToInt();
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

            await WriteVerificatToCache(logingEvent);//写入verificat到cache
            await _eventPublisher.PublishAsync(EventSubscriberConst.LoginOpenApi, logingEvent); //发布登录事件总线
                                                                                                //返回结果
            return new LoginOpenApiOutPut { VerificatId = sessionid, Token = accessToken, Account = openApiUser.Account };
        }

        private async Task RemoveVerificatFromCache(LoginOpenApiEvent loginEvent)
        {
            //获取verificat列表
            var verificatInfos = GetVerificatInfos(loginEvent.OpenApiUser.Id);
            if (verificatInfos != null)
            {
                //获取当前用户的verificat
                var verificat = verificatInfos.Where(it => it.Id == loginEvent.VerificatId).FirstOrDefault();
                if (verificat != null)
                    verificatInfos.Remove(verificat);
                //更新verificat列表
                _sysCacheService.SetOpenApiVerificatId(loginEvent.OpenApiUser.Id, verificatInfos);
            }
            await App.HttpContext?.SignOutAsync();
            App.HttpContext?.SignoutToSwagger();
            await Task.CompletedTask;
        }

        private async Task WriteVerificatToCache(LoginOpenApiEvent loginEvent)
        {
            //获取verificat列表
            List<VerificatInfo> verificatInfos = GetVerificatInfos(loginEvent.OpenApiUser.Id);
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
                var singleConfig = await _configService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_SINGLE_OPEN);//获取系统单用户登录选项
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
            _sysCacheService.SetOpenApiVerificatId(loginEvent.OpenApiUser.Id, verificatInfos);
        }
    }
}
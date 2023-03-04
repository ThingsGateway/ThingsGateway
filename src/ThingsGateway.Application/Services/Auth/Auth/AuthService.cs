using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using System.Security.Claims;

namespace ThingsGateway.Application.Services.Auth
{
    /// <inheritdoc cref="IAuthService"/>
    public class AuthService : IAuthService
    {
        private readonly IConfigService _configService;
        private readonly IEventPublisher _eventPublisher;
        private readonly INoticeService _noticeService;
        private readonly SysCacheService _sysCacheService;
        private readonly ISysUserService _userService;

        public AuthService(
                           IEventPublisher eventPublisher,
                           ISysUserService userService,
                           SysCacheService sysCacheService,
                           IConfigService configService,
                            INoticeService noticeService)
        {
            _sysCacheService = sysCacheService;
            _eventPublisher = eventPublisher;
            _userService = userService;
            _configService = configService;
            _noticeService = noticeService;
        }

        public ValidCodeOutPut GetCaptchaInfo()
        {
            //生成验证码
            var captchInfo = new Random().Next(1111, 9999).ToString();
            //生成请求号，并将验证码放入cache
            var reqNo = AddValidCodeToCache(captchInfo);
            //返回验证码和请求号
            return new ValidCodeOutPut { CodeValue = captchInfo, ValidCodeReqNo = reqNo };
        }

        /// <inheritdoc/>
        public async Task<SysUser> GetLoginUser()
        {
            var userInfo = await _userService.GetUserByAccount(UserManager.UserAccount);//根据账号获取用户信息
            return userInfo;
        }

        /// <inheritdoc/>
        public async Task<LoginOutPut> Login(LoginInput input)
        {
            //判断是否有验证码
            var sysBase = await _configService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_CAPTCHA_OPEN);
            if (sysBase != null)//如果有这个配置项
            {
                if (sysBase.ConfigValue.ToBoolean())//如果需要验证码
                {
                    //如果没填验证码，提示验证码不能为空
                    if (string.IsNullOrEmpty(input.ValidCode) || string.IsNullOrEmpty(input.ValidCodeReqNo)) throw Oops.Bah("验证码不能为空").StatusCode(410);
                    ValidValidCode(input.ValidCode, input.ValidCodeReqNo);//校验验证码
                }
            }
            var password = CryptogramUtil.Sm4Decrypt(input.Password);//SM4解密

            var userInfo = await _userService.GetUserByAccount(input.Account);//获取用户信息
            if (userInfo == null) throw Oops.Bah("用户不存在");//用户不存在
            if (userInfo.Password != password) throw Oops.Bah("账号密码错误");//账号密码错误
            return await Login(userInfo, input.Device);
        }

        /// <inheritdoc/>
        public async Task LoginOut()
        {
            //获取用户信息
            var userinfo = await _userService.GetUserByAccount(UserManager.UserAccount);
            if (userinfo != null)
            {
                LoginEvent loginEvent = new LoginEvent
                {
                    Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
                    SysUser = userinfo,
                    VerificatId = UserManager.VerificatId.ToLong(),
                };
                await RemoveVerificatFromCache(loginEvent);//移除verificat
            }
        }

        /// <summary>
        /// 添加验证码到cache
        /// </summary>
        /// <param name="code">验证码</param>
        /// <param name="expire">过期时间</param>
        /// <returns>验证码请求号</returns>
        private string AddValidCodeToCache(string code, int expire = 5)
        {
            //生成请求号
            var reqNo = YitIdHelper.NextId().ToString();
            //插入cache
            _sysCacheService.Set(CacheConst.Cache_Captcha, reqNo, code, TimeSpan.FromMinutes(expire));
            return reqNo;
        }

        private List<VerificatInfo> GetVerificatInfos(long userId)
        {
            //cache获取用户verificat列表
            List<VerificatInfo> verificatInfos = _sysCacheService.GetVerificatId(userId);
            return verificatInfos;
        }

        /// <summary>
        /// 执行B端登录
        /// </summary>
        /// <param name="sysUser">用户信息</param>
        /// <param name="device">登录设备</param>
        /// <returns></returns>
        private async Task<LoginOutPut> Login(SysUser sysUser, AuthDeviceTypeEnum device)
        {
            if (sysUser.UserStatus == false) throw Oops.Bah("账号已停用");//账号冻结

            var sysBase = await _configService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_VERIFICAT_EXPIRES);
            var sessionid = YitIdHelper.NextId();
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimConst.VerificatId, sessionid.ToString()));
            identity.AddClaim(new Claim(ClaimConst.UserId, sysUser.Id.ToString()));
            identity.AddClaim(new Claim(ClaimConst.Account, sysUser.Account));
            identity.AddClaim(new Claim(ClaimConst.IsSuperAdmin, sysUser.RoleCodeList.Contains(RoleConst.SuperAdmin).ToString()));
            identity.AddClaim(new Claim(ClaimConst.IsOpenApi, false.ToString()));

            var config = sysBase.ConfigValue.ToInt(2880);
            var diffTime = DateTimeOffset.Now.AddMinutes(config);
            await App.HttpContext.SignInAsync(new ClaimsPrincipal(identity), new AuthenticationProperties()
            {
                IsPersistent = true,
                ExpiresUtc = diffTime,
            });

            //登录事件参数
            var logingEvent = new LoginEvent
            {
                Ip = App.HttpContext.GetRemoteIpAddressToIPv4(),
                Device = device,
                Expire = config,
                SysUser = sysUser,
                VerificatId = sessionid,
            };

            await WriteVerificatToCache(logingEvent);//写入verificat到cache

            await _eventPublisher.PublishAsync(EventSubscriberConst.Login, logingEvent); //发布登录事件总线
            return new LoginOutPut { VerificatId = sessionid, Account = sysUser.Account };
        }

        /// <summary>
        /// 从Cache中删除验证码
        /// </summary>
        /// <param name="validCodeReqNo"></param>
        private void RemoveValidCodeFromCache(string validCodeReqNo)
        {
            _sysCacheService.Remove(CacheConst.Cache_Captcha, validCodeReqNo);//删除验证码
        }

        private async Task RemoveVerificatFromCache(LoginEvent loginEvent)
        {
            //获取verificat列表
            var verificatInfos = GetVerificatInfos(loginEvent.SysUser.Id);
            if (verificatInfos != null)
            {
                //获取当前用户的verificat
                var verificat = verificatInfos.Where(it => it.Id == loginEvent.VerificatId).FirstOrDefault();
                if (verificat != null)
                    verificatInfos.Remove(verificat);
                //更新verificat列表
                _sysCacheService.SetVerificatId(loginEvent.SysUser.Id, verificatInfos);
            }
            await App.HttpContext?.SignOutAsync();
            App.HttpContext?.SignoutToSwagger();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 单用户登录
        /// </summary>
        private async Task SingleLogin(string userId, List<VerificatInfo> verificatInfos)
        {
            var message = "该账号已在别处登录!";

            await _noticeService.LoginOut(userId, verificatInfos, message);//通知其他用户下线
        }

        /// <summary>
        /// 校验验证码方法
        /// </summary>
        /// <param name="validCode">验证码</param>
        /// <param name="validCodeReqNo">请求号</param>
        /// <param name="isDelete">是否从Cache删除</param>
        private void ValidValidCode(string validCode, string validCodeReqNo, bool isDelete = true)
        {
            var code = _sysCacheService.Get<string>(CacheConst.Cache_Captcha, validCodeReqNo);//从cache拿数据
            if (isDelete) RemoveValidCodeFromCache(validCodeReqNo);//如果需要删除验证码
            if (code != null)//如果有
            {
                //验证码如果不匹配直接抛错误，这里忽略大小写
                if (validCode.ToLower() != code.ToLower()) throw Oops.Bah("验证码错误");
            }
            else
            {
                throw Oops.Bah("验证码不能为空");//抛出验证码不能为空
            }
        }

        private async Task WriteVerificatToCache(LoginEvent loginEvent)
        {
            //获取verificat列表
            List<VerificatInfo> verificatInfos = GetVerificatInfos(loginEvent.SysUser.Id);
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
                var singleConfig = await _configService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_SINGLE_OPEN);//获取系统单用户登录选项
                if (singleConfig != null) isSingle = singleConfig.ConfigValue.ToBoolean();//如果配置不为空则设置单用户登录选项为系统配置的值
                                                                                          //判断是否单用户登录
                if (isSingle)
                {
                    await SingleLogin(loginEvent.SysUser.Id.ToString(), verificatInfos.Where(it => it.Device == loginEvent.Device.ToString()).ToList());//单用户登录方法
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
            _sysCacheService.SetVerificatId(loginEvent.SysUser.Id, verificatInfos);
        }
    }
}
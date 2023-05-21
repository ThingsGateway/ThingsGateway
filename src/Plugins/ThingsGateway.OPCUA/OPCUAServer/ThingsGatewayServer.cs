using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Opc.Ua;
using Opc.Ua.Server;

using System.Security.Cryptography.X509Certificates;

using ThingsGateway.Application;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.OPCUA;

/// <summary>
/// UAServer核心实现
/// </summary>
public partial class ThingsGatewayServer : StandardServer
{
    /// <summary>
    /// 自定义节点
    /// </summary>
    public ThingsGatewayNodeManager NodeManager;
    private ILogger _logger;
    private IServiceScope _serviceScope;
    private ICertificateValidator m_userCertificateValidator;
    private UploadDevice _device;
    /// <inheritdoc cref="ThingsGatewayServer"/>
    public ThingsGatewayServer(UploadDevice device, ILogger logger, IServiceScope serviceScope)
    {
        _device = device;
        _logger = logger;
        _serviceScope = serviceScope;
    }
    /// <inheritdoc/>
    public override UserTokenPolicyCollection GetUserTokenPolicies(ApplicationConfiguration configuration, EndpointDescription description)
    {
        var policies = base.GetUserTokenPolicies(configuration, description);

        // 样品如何修改默认用户令牌的政策
        if (description.SecurityPolicyUri == SecurityPolicies.Aes256_Sha256_RsaPss &&
            description.SecurityMode == MessageSecurityMode.SignAndEncrypt)
        {
            policies = new UserTokenPolicyCollection(policies.Where(u => u.TokenType != UserTokenType.Certificate));
        }
        else if (description.SecurityPolicyUri == SecurityPolicies.Aes128_Sha256_RsaOaep &&
            description.SecurityMode == MessageSecurityMode.Sign)
        {
            policies = new UserTokenPolicyCollection(policies.Where(u => u.TokenType != UserTokenType.Anonymous));
        }
        else if (description.SecurityPolicyUri == SecurityPolicies.Aes128_Sha256_RsaOaep &&
            description.SecurityMode == MessageSecurityMode.SignAndEncrypt)
        {
            policies = new UserTokenPolicyCollection(policies.Where(u => u.TokenType != UserTokenType.UserName));
        }
        return policies;
    }

    /// <inheritdoc/>
    protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
    {
        IList<INodeManager> nodeManagers = new List<INodeManager>();
        // 创建自定义节点管理器.
        NodeManager = new ThingsGatewayNodeManager(_serviceScope, _device, server, configuration);
        nodeManagers.Add(NodeManager);
        // 创建主节点管理器.
        var masterNodeManager = new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
        return masterNodeManager;
    }

    /// <inheritdoc/>
    protected override ResourceManager CreateResourceManager(IServerInternal server, ApplicationConfiguration configuration)
    {
        ResourceManager resourceManager = new ResourceManager(server, configuration);

        System.Reflection.FieldInfo[] fields = typeof(StatusCodes).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        foreach (System.Reflection.FieldInfo field in fields)
        {
            uint? id = field.GetValue(typeof(StatusCodes)) as uint?;

            if (id != null)
            {
                resourceManager.Add(id.Value, "en-US", field.Name);
            }
        }

        resourceManager.Add("InvalidPassword", "zh-cn", "密码验证失败，'{0}'.");
        resourceManager.Add("UnexpectedUserTokenError", "zh-cn", "错误的用户令牌。");
        resourceManager.Add("BadUserAccessDenied", "zh-cn", "当前用户名不存在。");

        return resourceManager;
    }

    /// <inheritdoc/>
    protected override ServerProperties LoadServerProperties()
    {
        ServerProperties properties = new ServerProperties
        {
            ManufacturerName = "Diego",
            ProductName = "ThingsGateway OPCUAServer",
            ProductUri = "https://diego2098.gitee.io/thingsgateway",
            SoftwareVersion = Utils.GetAssemblySoftwareVersion(),
            BuildNumber = Utils.GetAssemblyBuildNumber(),
            BuildDate = Utils.GetAssemblyTimestamp()
        };

        return properties;
    }

    /// <inheritdoc/>
    protected override void OnServerStarted(IServerInternal server)
    {
        // 当用户身份改变时请求。
        server.SessionManager.ImpersonateUser += SessionManager_ImpersonateUser;
        base.OnServerStarted(server);
        _logger.LogInformation("OPCUAServer启动成功");
    }

    /// <inheritdoc/>
    protected override void OnServerStarting(ApplicationConfiguration configuration)
    {
        _logger.LogInformation("OPCUAServer启动中......");
        base.OnServerStarting(configuration);

        // 由应用程序决定如何验证用户身份令牌。
        // 此函数为 X509 身份令牌创建验证器。
        CreateUserIdentityValidators(configuration);
    }

    /// <inheritdoc/>
    protected override void OnServerStopping()
    {
        _logger.LogInformation("OPCUAServer停止中......");
        base.OnServerStopping();
    }

    private void CreateUserIdentityValidators(ApplicationConfiguration configuration)
    {
        for (int ii = 0; ii < configuration.ServerConfiguration.UserTokenPolicies.Count; ii++)
        {
            UserTokenPolicy policy = configuration.ServerConfiguration.UserTokenPolicies[ii];

            // 为证书令牌策略创建验证器。
            if (policy.TokenType == UserTokenType.Certificate)
            {
                // check if user certificate trust lists are specified in configuration.
                if (configuration.SecurityConfiguration.TrustedUserCertificates != null &&
                    configuration.SecurityConfiguration.UserIssuerCertificates != null)
                {
                    CertificateValidator certificateValidator = new CertificateValidator();
                    certificateValidator.Update(configuration.SecurityConfiguration).Wait();
                    certificateValidator.Update(configuration.SecurityConfiguration.UserIssuerCertificates,
                        configuration.SecurityConfiguration.TrustedUserCertificates,
                        configuration.SecurityConfiguration.RejectedCertificateStore);

                    // set custom validator for user certificates.
                    m_userCertificateValidator = certificateValidator.GetChannelValidator();
                }
            }
        }
    }

    private void SessionManager_ImpersonateUser(Session session, ImpersonateEventArgs args)
    {
        // check for a user name token.
        UserNameIdentityToken userNameToken = args.NewIdentity as UserNameIdentityToken;

        if (userNameToken != null)
        {
            args.Identity = VerifyPassword(userNameToken);


            // set AuthenticatedUser role for accepted user/password authentication
            args.Identity.GrantedRoleIds.Add(ObjectIds.WellKnownRole_AuthenticatedUser);

            if (args.Identity is SystemConfigurationIdentity)
            {
                // set ConfigureAdmin role for user with permission to configure server
                args.Identity.GrantedRoleIds.Add(ObjectIds.WellKnownRole_ConfigureAdmin);
                args.Identity.GrantedRoleIds.Add(ObjectIds.WellKnownRole_SecurityAdmin);
            }

            return;
        }

        // check for x509 user token.
        X509IdentityToken x509Token = args.NewIdentity as X509IdentityToken;

        if (x509Token != null)
        {
            VerifyUserTokenCertificate(x509Token.Certificate);
            args.Identity = new UserIdentity(x509Token);
            Utils.LogInfo(Utils.TraceMasks.Security, "X509 Token Accepted: {0}", args.Identity?.DisplayName);

            // set AuthenticatedUser role for accepted certificate authentication
            args.Identity.GrantedRoleIds.Add(ObjectIds.WellKnownRole_AuthenticatedUser);

            return;
        }

        // check for anonymous token.
        if (args.NewIdentity is AnonymousIdentityToken || args.NewIdentity == null)
        {
            // allow anonymous authentication and set Anonymous role for this authentication
            args.Identity = new UserIdentity();
            args.Identity.GrantedRoleIds.Add(ObjectIds.WellKnownRole_Anonymous);

            return;
        }

        // unsuported identity token type.
        throw ServiceResultException.Create(StatusCodes.BadIdentityTokenInvalid,
               "Not supported user token type: {0}.", args.NewIdentity);
    }

    /// <summary>
    /// 从第三方用户中校验
    /// </summary>
    /// <param name="userNameToken"></param>
    /// <returns></returns>
    private IUserIdentity VerifyPassword(UserNameIdentityToken userNameToken)
    {
        var userName = userNameToken.UserName;
        var password = userNameToken.DecryptedPassword;
        if (string.IsNullOrEmpty(userName))
        {
            // an empty username is not accepted.
            throw ServiceResultException.Create(StatusCodes.BadIdentityTokenInvalid,
                "Security token is not a valid username token. An empty username is not accepted.");
        }

        if (string.IsNullOrEmpty(password))
        {
            // an empty password is not accepted.
            throw ServiceResultException.Create(StatusCodes.BadIdentityTokenRejected,
                "Security token is not a valid username token. An empty password is not accepted.");
        }
        var _openApiUserService = _serviceScope.ServiceProvider.GetService<IOpenApiUserService>();
        var userInfo = _openApiUserService.GetUserByAccount(userName).GetAwaiter().GetResult();//获取用户信息
        if (userInfo == null)
        {
            // construct translation object with default text.
            TranslationInfo info = new TranslationInfo(
                "InvalidPassword",
                "en-US",
                "Invalid username or password.",
                userName);

            // create an exception with a vendor defined sub-code.
            throw new ServiceResultException(new ServiceResult(
                StatusCodes.BadUserAccessDenied,
                "InvalidPassword",
                LoadServerProperties().ProductUri,
                new LocalizedText(info)));
        }
        // 有权配置服务器的用户
        if (userName == userInfo.Account && password == userInfo.Password)
        {
            return new SystemConfigurationIdentity(new UserIdentity(userNameToken));
        }
        else
        {
            return new UserIdentity(userNameToken);
        }


    }
    private void VerifyUserTokenCertificate(X509Certificate2 certificate)
    {
        try
        {
            if (m_userCertificateValidator != null)
            {
                m_userCertificateValidator.Validate(certificate);
            }
            else
            {
                CertificateValidator.Validate(certificate);
            }
        }
        catch (Exception e)
        {
            TranslationInfo info;
            StatusCode result = StatusCodes.BadIdentityTokenRejected;
            ServiceResultException se = e as ServiceResultException;
            if (se != null && se.StatusCode == StatusCodes.BadCertificateUseNotAllowed)
            {
                info = new TranslationInfo(
                    "InvalidCertificate",
                    "en-US",
                    "'{0}' is an invalid user certificate.",
                    certificate.Subject);

                result = StatusCodes.BadIdentityTokenInvalid;
            }
            else
            {
                // construct translation object with default text.
                info = new TranslationInfo(
                    "UntrustedCertificate",
                    "en-US",
                    "'{0}' is not a trusted user certificate.",
                    certificate.Subject);
            }

            // create an exception with a vendor defined sub-code.
            throw new ServiceResultException(new ServiceResult(
                result,
                info.Key,
                LoadServerProperties().ProductUri,
                new LocalizedText(info)));
        }
    }

}

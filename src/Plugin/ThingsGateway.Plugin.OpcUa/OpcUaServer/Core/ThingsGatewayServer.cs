//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Opc.Ua;
using Opc.Ua.Server;

using System.Security.Cryptography.X509Certificates;

using ThingsGateway.Admin.Application;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.OpcUa;

/// <summary>
/// UAServer核心实现
/// </summary>
public partial class ThingsGatewayServer : StandardServer
{
    /// <summary>
    /// 自定义节点
    /// </summary>
    public ThingsGatewayNodeManager NodeManager;

    private readonly BusinessBase _businessBase;

    private ICertificateValidator m_userCertificateValidator;

    /// <inheritdoc cref="ThingsGatewayServer"/>
    public ThingsGatewayServer(BusinessBase businessBase)
    {
        _businessBase = businessBase;
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
        List<INodeManager> nodeManagers = new List<INodeManager>();
        // 创建自定义节点管理器.
        NodeManager = new ThingsGatewayNodeManager(_businessBase, server, configuration);
        nodeManagers.Add(NodeManager);
        // 创建主节点管理器.
        var masterNodeManager = new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
        return masterNodeManager;
    }

    /// <inheritdoc/>
    protected override ResourceManager CreateResourceManager(IServerInternal server, ApplicationConfiguration configuration)
    {
        ResourceManager resourceManager = new(server, configuration);

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
    protected override void Dispose(bool disposing)
    {
        NodeManager?.SafeDispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override ServerProperties LoadServerProperties()
    {
        ServerProperties properties = new()
        {
            ManufacturerName = "Diego",
            ProductName = "ThingsGateway OPCUAServer",
            ProductUri = "https://gitee.com/diego2098/ThingsGateway",
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
        _businessBase.LogMessage.LogInformation("OPCUAServer Started");
    }

    /// <inheritdoc/>
    protected override void OnServerStarting(ApplicationConfiguration configuration)
    {
        _businessBase.LogMessage.LogInformation("OPCUAServer Starting");
        base.OnServerStarting(configuration);

        // 由应用程序决定如何验证用户身份令牌。
        // 此函数为 X509 身份令牌创建验证器。
        CreateUserIdentityValidators(configuration);
    }

    /// <inheritdoc/>
    protected override void OnServerStopping()
    {
        _businessBase.LogMessage.LogInformation("OPCUAServer Stoping");
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
                    CertificateValidator certificateValidator = new();
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
        // check for a user name cancellationToken.

        if (args.NewIdentity is UserNameIdentityToken userNameToken)
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

        // check for x509 user cancellationToken.

        if (args.NewIdentity is X509IdentityToken x509Token)
        {
            VerifyUserTokenCertificate(x509Token.Certificate);
            args.Identity = new UserIdentity(x509Token);
            Utils.LogInfo(Utils.TraceMasks.Security, "X509 Token Accepted: {0}", args.Identity?.DisplayName);

            // set AuthenticatedUser role for accepted certificate authentication
            args.Identity.GrantedRoleIds.Add(ObjectIds.WellKnownRole_AuthenticatedUser);

            return;
        }

        // check for anonymous cancellationToken.
        if (args.NewIdentity is AnonymousIdentityToken || args.NewIdentity == null)
        {
            // allow anonymous authentication and set Anonymous role for this authentication
            args.Identity = new UserIdentity();
            args.Identity.GrantedRoleIds.Add(ObjectIds.WellKnownRole_Anonymous);

            return;
        }

        // unsuported identity cancellationToken type.
        throw ServiceResultException.Create(StatusCodes.BadIdentityTokenInvalid,
               "Not supported user cancellationToken type: {0}.", args.NewIdentity);
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
                "Security cancellationToken is not a valid username cancellationToken. An empty username is not accepted.");
        }

        if (string.IsNullOrEmpty(password))
        {
            // an empty password is not accepted.
            throw ServiceResultException.Create(StatusCodes.BadIdentityTokenRejected,
                "Security cancellationToken is not a valid username cancellationToken. An empty password is not accepted.");
        }
        var sysUserService = App.RootServices.GetService<ISysUserService>();
        var userInfo = sysUserService.GetUserByAccountAsync(userName, null).ConfigureAwait(true).GetAwaiter().GetResult();//获取用户信息
        if (userInfo == null)
        {
            // construct translation object with default text.
            TranslationInfo info = new(
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
            if (e is ServiceResultException se && se.StatusCode == StatusCodes.BadCertificateUseNotAllowed)
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

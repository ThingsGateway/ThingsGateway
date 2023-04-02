using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Opc.Ua;
using Opc.Ua.Configuration;

using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.OPCUA;

/// <summary>
/// OPCUA服务端
/// </summary>
public partial class OPCUAServer : UpLoadBase
{
    private ApplicationInstance m_application;
    private ApplicationConfiguration m_configuration;
    private ThingsGatewayServer m_server;
    /// <inheritdoc cref="OPCUAServer"/>
    public OPCUAServer(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
    {
    }
    /// <summary>
    /// 服务地址
    /// </summary>
    [DeviceProperty("服务地址", "")]
    public string OpcUaStringUrl { get; set; } = "opc.tcp://127.0.0.1:49321";
    /// <summary>
    /// 安全策略
    /// </summary>
    [DeviceProperty("安全策略", "")]
    public bool SecurityPolicy { get; set; }

    /// <inheritdoc/>
    public override async Task BeforStart()
    {
        // 启动服务器。
        await m_application.CheckApplicationInstanceCertificate(true, 0);
        await m_application.Start(m_server);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        m_server.Stop();
        m_server.Dispose();
        m_application.Stop();
    }

    /// <inheritdoc/>
    public override OperResult IsConnected()
    {
        var result = m_server.GetStatus();

        return OperResult.CreateSuccessResult(result);
    }

    /// <inheritdoc/>
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            ////变化推送
            var varList = CollectVariableRunTimes.ToListWithDequeue();
            if (varList?.Count != 0)
            {
                foreach (var item in varList)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            m_server.NodeManager.UpVariable(item);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, ToString());
                    }
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ToString());
        }
        await Task.CompletedTask;
    }
    private ConcurrentQueue<VariableData> CollectVariableRunTimes { get; set; } = new();

    /// <inheritdoc/>
    protected override void Init(UploadDevice device)
    {
        m_application = new ApplicationInstance();
        m_configuration = GetDefaultConfiguration();
        m_configuration.Validate(ApplicationType.Server).GetAwaiter().GetResult();
        m_application.ApplicationConfiguration = m_configuration;
        if (m_configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
        {
            m_configuration.CertificateValidator.CertificateValidation += (s, e) =>
            {
                e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted);
            };
        }
        m_server = new(_logger, _scopeFactory.CreateScope());

        using var serviceScope = _scopeFactory.CreateScope();
        var _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();


        _globalCollectDeviceData.CollectVariables.ForEach(a =>
        {
            VariableValueChange(a);
            a.VariableValueChange += VariableValueChange;
        });
    }

    private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
    {
        CollectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }


    private ApplicationConfiguration GetDefaultConfiguration()
    {
        ApplicationConfiguration config = new ApplicationConfiguration();
        string url = OpcUaStringUrl;
        // 签名及加密验证
        ServerSecurityPolicyCollection policies = new ServerSecurityPolicyCollection();
        if (SecurityPolicy)
        {
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.Sign,
                SecurityPolicyUri = SecurityPolicies.Basic128Rsa15
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = SecurityPolicies.Basic128Rsa15
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.Sign,
                SecurityPolicyUri = SecurityPolicies.Basic256
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = SecurityPolicies.Basic256
            });
        }
        else
        {
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.None,
                SecurityPolicyUri = SecurityPolicies.None
            });
        }

        config.ApplicationName = "ThingsGateway OPCUAServer";
        config.ApplicationType = ApplicationType.Server;
        config.ApplicationUri = Utils.Format(@"urn:{0}:thingsgatewayopcuaserver", System.Net.Dns.GetHostName());

        var userTokens = new UserTokenPolicyCollection();
        userTokens.Add(new UserTokenPolicy(UserTokenType.UserName));

        config.ServerConfiguration = new ServerConfiguration()
        {
            // 配置登录的地址
            BaseAddresses = new string[] { url },
            SecurityPolicies = policies,
            UserTokenPolicies = userTokens,
            ShutdownDelay = 1,

            DiagnosticsEnabled = false,           // 是否启用诊断
            MaxSessionCount = 1000,               // 最大打开会话数
            MinSessionTimeout = 10000,            // 允许该会话在与客户端断开时（单位毫秒）仍然保持连接的最小时间
            MaxSessionTimeout = 60000,            // 允许该会话在与客户端断开时（单位毫秒）仍然保持连接的最大时间
            MaxBrowseContinuationPoints = 1000,   // 用于Browse / BrowseNext操作的连续点的最大数量。
            MaxQueryContinuationPoints = 1000,    // 用于Query / QueryNext操作的连续点的最大数量
            MaxHistoryContinuationPoints = 500,   // 用于HistoryRead操作的最大连续点数。
            MaxRequestAge = 1000000,              // 传入请求的最大年龄（旧请求被拒绝）。
            MinPublishingInterval = 100,          // 服务器支持的最小发布间隔（以毫秒为单位）
            MaxPublishingInterval = 3600000,      // 服务器支持的最大发布间隔（以毫秒为单位）1小时
            PublishingResolution = 50,            // 支持的发布间隔（以毫秒为单位）的最小差异
            MaxSubscriptionLifetime = 3600000,    // 订阅将在没有客户端发布的情况下保持打开多长时间 1小时
            MaxMessageQueueSize = 100,            // 每个订阅队列中保存的最大消息数
            MaxNotificationQueueSize = 100,       // 为每个被监视项目保存在队列中的最大证书数
            MaxNotificationsPerPublish = 1000,    // 每次发布的最大通知数
            MinMetadataSamplingInterval = 1000,   // 元数据的最小采样间隔
            MaxRegistrationInterval = 30000,   // 两次注册尝试之间的最大时间（以毫秒为单位）

        };
        config.SecurityConfiguration = new SecurityConfiguration()
        {
            AddAppCertToTrustedStore = true,
            AutoAcceptUntrustedCertificates = true,
            RejectSHA1SignedCertificates = false,
            MinimumCertificateKeySize = 1024,
            SuppressNonceValidationErrors = true,
            ApplicationCertificate = new CertificateIdentifier()
            {
                StoreType = CertificateStoreType.X509Store,
                StorePath = "CurrentUser\\UAServer_ThingsGateway",
                SubjectName = "CN=ThingsGateway OPCUAServer, C=CN, S=GUANGZHOU, O=ThingsGateway, DC=" + System.Net.Dns.GetHostName(),
            },

            TrustedPeerCertificates = new CertificateTrustList()
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = "%CommonApplicationData%\\ThingsGateway\\pki\\issuer",
            },

            TrustedIssuerCertificates = new CertificateTrustList()
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = "%CommonApplicationData%\\ThingsGateway\\pki\\issuer",
            },

            RejectedCertificateStore = new CertificateStoreIdentifier()
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = "%CommonApplicationData%\\ThingsGateway\\pki\\rejected",
            },
            UserIssuerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = "%CommonApplicationData%\\ThingsGateway\\pki\\issuerUser",

            },
            TrustedUserCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = "%CommonApplicationData%\\ThingsGateway\\pki\\trustedUser",
            }
        };

        config.TransportConfigurations = new TransportConfigurationCollection();
        config.TransportQuotas = new TransportQuotas { OperationTimeout = 15000 };
        config.ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 };
        config.TraceConfiguration = new TraceConfiguration();


        config.CertificateValidator = new CertificateValidator();
        config.CertificateValidator.Update(config);
        config.Extensions = new XmlElementCollection();

        return config;
    }

}

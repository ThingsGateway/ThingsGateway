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

using Newtonsoft.Json.Linq;

using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Generic;


//修改自https://github.com/dathlin/OpcUaHelper 与OPC基金会net库

namespace ThingsGateway.Foundation.Adapter.OPCUA;
public delegate void DataChangedEventHandler(List<(NodeId id, DataValue dataValue, JToken jToken)> values);
public class OPCUAClient : DisposableObject
{

    #region 属性，变量等
    /// <summary>
    /// 当前配置
    /// </summary>
    public OPCNode OPCNode;

    /// <summary>
    /// 当前保存的变量名称列表,需重新连接,订阅生效
    /// </summary>
    public List<string> Variables = new();
    /// <summary>
    /// 当前的变量名称/OPC变量节点
    /// </summary>
    private Dictionary<string, VariableNode> _variableDicts = new();
    private EasyLock checkLock = new();
    /// <summary>
    /// 当前的订阅组，组名称/组
    /// </summary>
    private Dictionary<string, Subscription> dic_subscriptions;

    private ApplicationInstance m_application;

    private ApplicationConfiguration m_configuration;

    private EventHandler m_ConnectComplete;

    private ConcurrentQueue<(NodeId, DataValue, JToken)> m_data = new();

    private bool m_IsConnected;

    private EventHandler m_KeepAliveComplete;

    private EventHandler<OPCUAStatusEventArgs> m_OpcStatusChange;

    private EventHandler m_ReconnectComplete;

    private SessionReconnectHandler m_reConnectHandler;

    private EventHandler m_ReconnectStarting;

    private ISession m_session;

    /// <summary>
    /// 默认的构造函数，实例化一个新的OPC UA类
    /// </summary>
    public OPCUAClient()
    {
        dic_subscriptions = new();

        var certificateValidator = new CertificateValidator();
        certificateValidator.CertificateValidation += CertificateValidation;
        SecurityConfiguration securityConfigurationcv = new SecurityConfiguration
        {
            UseValidatedCertificates = true,
            AutoAcceptUntrustedCertificates = true,
            RejectSHA1SignedCertificates = false,
            MinimumCertificateKeySize = 1024,
        };
        certificateValidator.Update(securityConfigurationcv);

        // 构建应用程序配置
        var configuration = new ApplicationConfiguration
        {
            ApplicationName = OPCUAName,
            ApplicationType = ApplicationType.Client,
            CertificateValidator = certificateValidator,
            ApplicationUri = Utils.Format(@"urn:{0}:thingsgatewayopcuaclient", System.Net.Dns.GetHostName()),
            ProductUri = "https://diego2098.gitee.io/thingsgateway-docs/",

            ServerConfiguration = new ServerConfiguration
            {
                MaxSubscriptionCount = 100000,
                MaxMessageQueueSize = 1000000,
                MaxNotificationQueueSize = 1000000,
                MaxPublishRequestCount = 10000000,

            },

            SecurityConfiguration = new SecurityConfiguration
            {
                AutoAcceptUntrustedCertificates = true,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 1024,
                SuppressNonceValidationErrors = true,

                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = CertificateStoreType.X509Store,
                    StorePath = "CurrentUser\\UA_ThingsGateway",
                    SubjectName = "CN=ThingsGateway OPCUAClient, C=CN, S=GUANGZHOU, O=ThingsGateway, DC=" + System.Net.Dns.GetHostName(),
                },

                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = AppContext.BaseDirectory + @"OPCUAClientCertificate\pki\trustedIssuer",
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = AppContext.BaseDirectory + @"OPCUAClientCertificate\pki\trustedPeer",
                },
                RejectedCertificateStore = new CertificateStoreIdentifier
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = AppContext.BaseDirectory + @"OPCUAClientCertificate\pki\rejected",
                },
                UserIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = AppContext.BaseDirectory + @"OPCUAClientCertificate\pki\issuerUser",
                },
                TrustedUserCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.Directory,
                    StorePath = AppContext.BaseDirectory + @"OPCUAClientCertificate\pki\trustedUser",
                }


            },

            TransportQuotas = new TransportQuotas
            {
                OperationTimeout = 6000000,
                MaxStringLength = int.MaxValue,
                MaxByteStringLength = int.MaxValue,
                MaxArrayLength = 65535,
                MaxMessageSize = 419430400,
                MaxBufferSize = 65535,
                ChannelLifetime = -1,
                SecurityTokenLifetime = -1
            },
            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = -1,
                MinSubscriptionLifetime = -1,
            },
            DisableHiResClock = true
        };

        configuration.Validate(ApplicationType.Client);
        m_configuration = configuration;
        m_application = new ApplicationInstance();
        m_application.ApplicationConfiguration = m_configuration;

        Task.Run(dataChangedHandlerInvoke);

    }

    /// <summary>
    /// 成功连接后或disconnecing从服务器。
    /// </summary>
    public event EventHandler ConnectComplete
    {
        add { m_ConnectComplete += value; }
        remove { m_ConnectComplete -= value; }
    }

    public event DataChangedEventHandler DataChangedHandler;
    /// <summary>
    /// Raised when a good keep alive from the server arrives.
    /// </summary>
    public event EventHandler KeepAliveComplete
    {
        add { m_KeepAliveComplete += value; }
        remove { m_KeepAliveComplete -= value; }
    }

    /// <summary>
    /// 状态更改
    /// </summary>
    public event EventHandler<OPCUAStatusEventArgs> OpcStatusChange
    {
        add { m_OpcStatusChange += value; }
        remove { m_OpcStatusChange -= value; }
    }

    /// <summary>
    /// 连接操作完成
    /// </summary>
    public event EventHandler ReconnectComplete
    {
        add { m_ReconnectComplete += value; }
        remove { m_ReconnectComplete -= value; }
    }

    /// <summary>
    /// 连接操作开始
    /// </summary>
    public event EventHandler ReconnectStarting
    {
        add { m_ReconnectStarting += value; }
        remove { m_ReconnectStarting -= value; }
    }

    /// <summary>
    /// 配置信息
    /// </summary>
    public ApplicationConfiguration AppConfig => m_configuration;

    /// <summary>
    /// 连接状态
    /// </summary>
    public bool Connected
    {
        get { return m_IsConnected; }
    }

    /// <summary>
    /// OPCUAClient
    /// </summary>
    public string OPCUAName { get; set; } = "OPCUAClient";

    /// <summary>
    /// 当前活动会话。
    /// </summary>
    public ISession Session => m_session;

    /// <summary>
    /// UserIdentity
    /// </summary>
    public IUserIdentity UserIdentity { get; set; }

    #endregion

    #region 订阅
    /// <summary>
    /// 新增订阅，需要指定订阅的关键字，订阅的tag名数组，以及回调方法
    /// </summary>
    public void AddSubscription(string subscriptionName, string[] tags)
    {
        Subscription m_subscription = new Subscription(m_session.DefaultSubscription);

        m_subscription.PublishingEnabled = true;
        m_subscription.PublishingInterval = 0;
        m_subscription.KeepAliveCount = uint.MaxValue;
        m_subscription.LifetimeCount = uint.MaxValue;
        m_subscription.MaxNotificationsPerPublish = uint.MaxValue;
        m_subscription.Priority = 100;
        m_subscription.DisplayName = subscriptionName;
        List<MonitoredItem> monitoredItems = new List<MonitoredItem>();
        for (int i = 0; i < tags.Length; i++)
        {
            var item = new MonitoredItem
            {
                StartNodeId = new NodeId(tags[i]),
                AttributeId = Attributes.Value,
                DisplayName = tags[i],
                Filter = new DataChangeFilter() { DeadbandValue = OPCNode.DeadBand, DeadbandType = (int)DeadbandType.Absolute, Trigger = DataChangeTrigger.StatusValue },
                SamplingInterval = OPCNode?.UpdateRate ?? 1000,
            };
            item.Notification += callback;
            monitoredItems.Add(item);
        }
        m_subscription.AddItems(monitoredItems);

        m_session.AddSubscription(m_subscription);
        m_subscription.Create();
        foreach (var item in m_subscription.MonitoredItems.Where(a => a.Status.Error?.StatusCode == Opc.Ua.StatusCodes.BadFilterNotAllowed))
        {
            item.Filter = new DataChangeFilter() { DeadbandValue = OPCNode.DeadBand, DeadbandType = (int)DeadbandType.None, Trigger = DataChangeTrigger.StatusValue };
        }
        m_subscription.ApplyChanges();

        lock (dic_subscriptions)
        {
            if (dic_subscriptions.ContainsKey(subscriptionName))
            {
                // remove
                dic_subscriptions[subscriptionName].Delete(true);
                m_session.RemoveSubscription(dic_subscriptions[subscriptionName]);
                dic_subscriptions[subscriptionName].SafeDispose();
                dic_subscriptions[subscriptionName] = m_subscription;
            }
            else
            {
                dic_subscriptions.Add(subscriptionName, m_subscription);
            }
        }
    }

    /// <summary>
    /// 移除所有的订阅消息
    /// </summary>
    public void RemoveAllSubscription()
    {
        lock (dic_subscriptions)
        {
            foreach (var item in dic_subscriptions)
            {
                item.Value.Delete(true);
                m_session.RemoveSubscription(item.Value);
                item.Value.SafeDispose();
            }
            dic_subscriptions.Clear();
        }
    }

    /// <summary>
    /// 移除订阅消息
    /// </summary>
    /// <param name="key">组名称</param>
    public void RemoveSubscription(string key)
    {
        lock (dic_subscriptions)
        {
            if (dic_subscriptions.ContainsKey(key))
            {
                // remove
                dic_subscriptions[key].Delete(true);
                m_session.RemoveSubscription(dic_subscriptions[key]);
                dic_subscriptions[key].SafeDispose();
                dic_subscriptions.RemoveWhere(a => a.Key == key);
            }
        }

    }

    #endregion


    #region 其他方法
    /// <summary>
    /// 浏览一个节点的引用
    /// </summary>
    /// <param name="tag">节点值</param>
    /// <returns>引用节点描述</returns>
    public async Task<ReferenceDescription[]> BrowseNodeReferenceAsync(string tag)
    {
        NodeId sourceId = new NodeId(tag);

        // 该节点可以读取到方法
        BrowseDescription nodeToBrowse1 = new BrowseDescription();

        nodeToBrowse1.NodeId = sourceId;
        nodeToBrowse1.BrowseDirection = BrowseDirection.Forward;
        nodeToBrowse1.ReferenceTypeId = ReferenceTypeIds.Aggregates;
        nodeToBrowse1.IncludeSubtypes = true;
        nodeToBrowse1.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method);
        nodeToBrowse1.ResultMask = (uint)BrowseResultMask.All;

        // find all nodes organized by the node.
        BrowseDescription nodeToBrowse2 = new BrowseDescription();

        nodeToBrowse2.NodeId = sourceId;
        nodeToBrowse2.BrowseDirection = BrowseDirection.Forward;
        nodeToBrowse2.ReferenceTypeId = ReferenceTypeIds.Organizes;
        nodeToBrowse2.IncludeSubtypes = true;
        nodeToBrowse2.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable);
        nodeToBrowse2.ResultMask = (uint)BrowseResultMask.All;

        BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
        nodesToBrowse.Add(nodeToBrowse1);
        nodesToBrowse.Add(nodeToBrowse2);

        // fetch references from the server.
        ReferenceDescriptionCollection references = await FormUtils.BrowseAsync(m_session, nodesToBrowse, false);

        return references.ToArray();
    }




    /// <summary>
    /// 调用服务器的方法
    /// </summary>
    /// <param name="tagParent">方法的父节点tag</param>
    /// <param name="tag">方法的节点tag</param>
    /// <param name="args">传递的参数</param>
    /// <returns>输出的结果值</returns>
    public object[] CallMethodByNodeId(string tagParent, string tag, params object[] args)
    {
        if (m_session == null)
        {
            return null;
        }

        IList<object> outputArguments = m_session.Call(
            new NodeId(tagParent),
            new NodeId(tag),
            args);

        return outputArguments.ToArray();
    }



    /// <summary>
    /// 读取历史数据
    /// </summary>
    /// <param name="tag">节点的索引</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="count">读取的个数</param>
    /// <param name="containBound">是否包含边界</param>
    /// <returns>读取的数据列表</returns>
    public async Task<List<DataValue>> ReadHistoryRawDataValues(string tag, DateTime start, DateTime end, uint count = 1, bool containBound = false, CancellationToken cancellationToken = default)
    {
        HistoryReadValueId m_nodeToContinue = new HistoryReadValueId()
        {
            NodeId = new NodeId(tag),
        };

        ReadRawModifiedDetails m_details = new ReadRawModifiedDetails
        {
            StartTime = start,
            EndTime = end,
            NumValuesPerNode = count,
            IsReadModified = false,
            ReturnBounds = containBound
        };

        HistoryReadValueIdCollection nodesToRead = new HistoryReadValueIdCollection();
        nodesToRead.Add(m_nodeToContinue);

        var result = await m_session.HistoryReadAsync(
             null,
             new ExtensionObject(m_details),
             TimestampsToReturn.Both,
             false,
             nodesToRead,
             cancellationToken);
        var results = result.Results;
        var diagnosticInfos = result.DiagnosticInfos;
        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        if (StatusCode.IsBad(results[0].StatusCode))
        {
            throw new ServiceResultException(results[0].StatusCode);
        }

        HistoryData values = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;
        return values.DataValues;
    }

    #endregion


    #region 连接
    /// <summary>
    /// 连接到服务器
    /// </summary>
    public async Task ConnectAsync()
    {
        m_session = await ConnectAsync(OPCNode.OPCURL);
    }

    /// <summary>
    /// Disconnects from the server.
    /// </summary>
    public void Disconnect()
    {
        UpdateStatus(false, DateTime.UtcNow, "断开连接");

        // stop any reconnect operation.
        if (m_reConnectHandler != null)
        {
            m_reConnectHandler.SafeDispose();
            m_reConnectHandler = null;
        }

        // disconnect any existing session.
        if (m_session != null)
        {
            m_session.Close(10000);
            m_session = null;
        }

        // update the client status
        m_IsConnected = false;

    }
    #endregion

    #region 读取

    /// <summary>
    /// 从服务器或缓存读取节点
    /// </summary>
    private Node ReadNode(string nodeIdStr, bool isOnlyServer = true)
    {
        if (!isOnlyServer)
        {
            if (_variableDicts.TryGetValue(nodeIdStr, out var value))
            {
                return value;
            }
        }

        NodeId nodeToRead = new NodeId(nodeIdStr);
        var node = m_session.ReadNode(nodeToRead);
        _variableDicts.AddOrUpdate(nodeIdStr, (VariableNode)node);
        return node;
    }

    /// <summary>
    /// 从服务器读取值
    /// </summary>
    private async Task<List<DataValue>> ReadValueAsync(NodeId[] nodeIds, CancellationToken cancellationToken = default)
    {
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
        for (int i = 0; i < nodeIds.Length; i++)
        {
            nodesToRead.Add(new ReadValueId()
            {
                NodeId = nodeIds[i],
                AttributeId = Attributes.Value
            });
        }

        // 读取当前的值
        var result = await m_session.ReadAsync(
             null,
             0,
             TimestampsToReturn.Neither,
             nodesToRead,
             cancellationToken);
        var results = result.Results;
        var diagnosticInfos = result.DiagnosticInfos;
        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        return results.ToList();
    }

    /// <summary>
    /// 从服务器读取值
    /// </summary>
    private async Task<List<(string, StatusCode, JToken)>> ReadJTokenValueAsync(NodeId[] nodeIds, CancellationToken cancellationToken = default)
    {
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
        for (int i = 0; i < nodeIds.Length; i++)
        {
            nodesToRead.Add(new ReadValueId()
            {
                NodeId = nodeIds[i],
                AttributeId = Attributes.Value
            });
        }

        // 读取当前的值
        var result = await m_session.ReadAsync(
             null,
             0,
             TimestampsToReturn.Neither,
             nodesToRead,
             cancellationToken);
        var results = result.Results;
        var diagnosticInfos = result.DiagnosticInfos;
        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);
        List<(string, StatusCode, JToken)> jTokens = new();
        for (int i = 0; i < results.Count; i++)
        {
            var node = (VariableNode)ReadNode(nodeIds[i].ToString(), false);
            var typeManager = new DataTypeManager(m_session);
            var opcvalue = typeManager.GetJToken(node, results[i]);
            jTokens.Add((node.NodeId.ToString(), results[i].StatusCode, opcvalue));
        }
        return jTokens.ToList();
    }

    /// <summary>
    /// 从服务器读取值节
    /// </summary>
    public async Task<List<(string, StatusCode, JToken)>> ReadJTokenValueAsync(string[] tags, CancellationToken cancellationToken = default)
    {
        var result = await ReadJTokenValueAsync(tags.Select(a => new NodeId(a)).ToArray(), cancellationToken);
        return result;
    }

    /// <summary>
    /// 异步写opc标签
    /// </summary>
    public async Task<OperResult> WriteNodeAsync(string tag, JToken value, CancellationToken cancellationToken = default)
    {
        try
        {

            WriteValue valueToWrite = new WriteValue()
            {
                NodeId = new NodeId(tag),
                AttributeId = Attributes.Value,
            };
            var node = (VariableNode)ReadNode(tag.ToString(), false);
            var typeManager = new DataTypeManager(m_session);
            valueToWrite.Value = typeManager.GetDataValueFromVariableState(value, node);
            WriteValueCollection valuesToWrite = new WriteValueCollection
            {
                valueToWrite
            };

            var result = await m_session.WriteAsync(
     requestHeader: null,
     nodesToWrite: valuesToWrite, cancellationToken);

            ClientBase.ValidateResponse(result.Results, valuesToWrite);
            ClientBase.ValidateDiagnosticInfos(result.DiagnosticInfos, valuesToWrite);
            if (!StatusCode.IsGood(result.Results[0]))
            {
                return new OperResult(result.Results[0].ToString());
            }
            return OperResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }

    }
    private void callback(MonitoredItem monitoreditem, MonitoredItemNotificationEventArgs monitoredItemNotificationEventArgs)
    {
        VariableNode variableNode = (VariableNode)ReadNode(monitoreditem.StartNodeId.ToString(), false);
        foreach (var value in monitoreditem.DequeueValues())
        {
            var variant = new Variant(value.Value);
            BuiltInType type = TypeInfo.GetBuiltInType(variableNode.DataType, m_session.SystemContext.TypeTable);
            var typeManager = new DataTypeManager(monitoreditem.Subscription.Session);
            var opcvalue = typeManager.GetJToken(variableNode, value);
            m_data.Enqueue((monitoreditem.StartNodeId, value, opcvalue));
        }
    }

    /// <summary>
    /// 订阅通知线程
    /// </summary>
    /// <returns></returns>
    private async Task dataChangedHandlerInvoke()
    {
        while (!DisposedValue)
        {
            if (m_data.Count > 0)
                DataChangedHandler?.Invoke(m_data.ToListWithDequeue());
            if (OPCNode == null)
                await Task.Delay(1000);
            else
                await Task.Delay(OPCNode.UpdateRate == 0 ? 1000 : OPCNode.UpdateRate);
        }
    }

    #endregion


    #region 特性

    /// <summary>
    /// 读取一个节点的所有属性
    /// </summary>
    /// <param name="tag">节点信息</param>
    /// <returns>节点的特性值</returns>
    public OPCNodeAttribute[] ReadNoteAttributes(string tag)
    {
        NodeId sourceId = new NodeId(tag);
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();

        // attempt to read all possible attributes.
        // 尝试着去读取所有可能的特性
        for (uint ii = Attributes.NodeClass; ii <= Attributes.UserExecutable; ii++)
        {
            ReadValueId nodeToRead = new ReadValueId();
            nodeToRead.NodeId = sourceId;
            nodeToRead.AttributeId = ii;
            nodesToRead.Add(nodeToRead);
        }

        int startOfProperties = nodesToRead.Count;

        // find all of the pror of the node.
        BrowseDescription nodeToBrowse1 = new BrowseDescription();

        nodeToBrowse1.NodeId = sourceId;
        nodeToBrowse1.BrowseDirection = BrowseDirection.Forward;
        nodeToBrowse1.ReferenceTypeId = ReferenceTypeIds.HasProperty;
        nodeToBrowse1.IncludeSubtypes = true;
        nodeToBrowse1.NodeClassMask = 0;
        nodeToBrowse1.ResultMask = (uint)BrowseResultMask.All;

        BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
        nodesToBrowse.Add(nodeToBrowse1);

        // fetch property references from the server.
        ReferenceDescriptionCollection references = FormUtils.Browse(m_session, nodesToBrowse, false);

        if (references == null)
        {
            return new OPCNodeAttribute[0];
        }

        for (int ii = 0; ii < references.Count; ii++)
        {
            // ignore external references.
            if (references[ii].NodeId.IsAbsolute)
            {
                continue;
            }

            ReadValueId nodeToRead = new ReadValueId();
            nodeToRead.NodeId = (NodeId)references[ii].NodeId;
            nodeToRead.AttributeId = Attributes.Value;
            nodesToRead.Add(nodeToRead);
        }

        // read all values.
        DataValueCollection results = null;
        DiagnosticInfoCollection diagnosticInfos = null;

        m_session.Read(
            null,
            0,
            TimestampsToReturn.Neither,
            nodesToRead,
            out results,
            out diagnosticInfos);

        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        // process results.

        List<OPCNodeAttribute> nodeAttribute = new List<OPCNodeAttribute>();
        for (int ii = 0; ii < results.Count; ii++)
        {
            OPCNodeAttribute item = new OPCNodeAttribute();

            // process attribute value.
            if (ii < startOfProperties)
            {
                // ignore attributes which are invalid for the node.
                if (results[ii].StatusCode == StatusCodes.BadAttributeIdInvalid)
                {
                    continue;
                }

                // get the name of the attribute.
                item.Name = Attributes.GetBrowseName(nodesToRead[ii].AttributeId);

                // display any unexpected error.
                if (StatusCode.IsBad(results[ii].StatusCode))
                {
                    item.Type = Utils.Format("{0}", Attributes.GetDataTypeId(nodesToRead[ii].AttributeId));
                    item.Value = Utils.Format("{0}", results[ii].StatusCode);
                }

                // display the value.
                else
                {
                    TypeInfo typeInfo = TypeInfo.Construct(results[ii].Value);

                    item.Type = typeInfo.BuiltInType.ToString();

                    if (typeInfo.ValueRank >= ValueRanks.OneOrMoreDimensions)
                    {
                        item.Type += "[]";
                    }

                    item.Value = results[ii].Value;//Utils.Format("{0}", results[ii].Value);
                }
            }

            // process property value.
            else
            {
                // ignore properties which are invalid for the node.
                if (results[ii].StatusCode == StatusCodes.BadNodeIdUnknown)
                {
                    continue;
                }

                // get the name of the property.
                item.Name = Utils.Format("{0}", references[ii - startOfProperties]);

                // display any unexpected error.
                if (StatusCode.IsBad(results[ii].StatusCode))
                {
                    item.Type = String.Empty;
                    item.Value = Utils.Format("{0}", results[ii].StatusCode);
                }

                // display the value.
                else
                {
                    TypeInfo typeInfo = TypeInfo.Construct(results[ii].Value);

                    item.Type = typeInfo.BuiltInType.ToString();

                    if (typeInfo.ValueRank >= ValueRanks.OneOrMoreDimensions)
                    {
                        item.Type += "[]";
                    }

                    item.Value = results[ii].Value; //Utils.Format("{0}", results[ii].Value);
                }
            }

            nodeAttribute.Add(item);
        }

        return nodeAttribute.ToArray();
    }

    /// <summary>
    /// 读取一个节点的所有属性
    /// </summary>
    public async Task<OperResult<List<OPCNodeAttribute>>> ReadNoteAttributeAsync(string tag, uint attributesId, CancellationToken cancellationToken = default)
    {
        BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
        NodeId sourceId = new NodeId(tag);

        ReadValueId nodeToRead = new ReadValueId
        {
            NodeId = sourceId,
            AttributeId = attributesId
        };
        nodesToRead.Add(nodeToRead);
        BrowseDescription nodeToBrowse = new BrowseDescription
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.HasProperty,
            IncludeSubtypes = true,
            NodeClassMask = 0,
            ResultMask = (uint)BrowseResultMask.All
        };
        nodesToBrowse.Add(nodeToBrowse);

        var result1 = await ReadNoteAttributeAsync(nodesToBrowse, nodesToRead, cancellationToken);
        var result2 = result1.Copy<List<OPCNodeAttribute>>();
        result2.Content = result1.Content?.Values?.FirstOrDefault()?.ToList();
        return result2;
    }

    /// <summary>
    /// 读取节点的所有属性
    /// </summary>
    public async Task<OperResult<Dictionary<string, List<OPCNodeAttribute>>>> ReadNoteAttributeAsync(List<string> tags, CancellationToken cancellationToken)
    {
        BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
        foreach (var tag in tags)
        {
            NodeId sourceId = new NodeId(tag);

            for (uint ii = Attributes.NodeClass; ii <= Attributes.UserExecutable; ii++)
            {
                ReadValueId nodeToRead = new ReadValueId();
                nodeToRead.NodeId = sourceId;
                nodeToRead.AttributeId = ii;
                nodesToRead.Add(nodeToRead);
            }
            BrowseDescription nodeToBrowse = new BrowseDescription
            {
                NodeId = sourceId,
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HasProperty,
                IncludeSubtypes = true,
                NodeClassMask = 0,
                ResultMask = (uint)BrowseResultMask.All
            };
            nodesToBrowse.Add(nodeToBrowse);

        }

        return await ReadNoteAttributeAsync(nodesToBrowse, nodesToRead, cancellationToken);
    }
    #endregion





    protected override void Dispose(bool disposing)
    {
        Disconnect();
        base.Dispose(disposing);
    }

    #region 私有方法

    private void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs eventArgs)
    {
        if (ServiceResult.IsGood(eventArgs.Error))
            eventArgs.Accept = true;
        else if (eventArgs.Error.StatusCode.Code == StatusCodes.BadCertificateUntrusted)
            eventArgs.Accept = true;
        else
            throw new Exception(string.Format("验证证书失败，错误代码:{0}: {1}", eventArgs.Error.Code, eventArgs.Error.AdditionalInfo));
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <returns>The new session object.</returns>
    private async Task<ISession> ConnectAsync(string serverUrl)
    {
        // disconnect from existing session.
        Disconnect();

        if (m_configuration == null)
        {
            throw new ArgumentNullException("未初始化配置");
        }
        var useSecurity = OPCNode?.IsUseSecurity ?? true;
        // select the best endpoint.
        EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(serverUrl, useSecurity);
        EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(m_configuration);

        ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);
        await m_application.CheckApplicationInstanceCertificate(true, 0, 1200);
        //var x509 = await m_configuration.SecurityConfiguration.ApplicationCertificate.Find(true);
        m_session = await Opc.Ua.Client.Session.Create(
     m_configuration,
    endpoint,
    true,
    false,
    (string.IsNullOrEmpty(OPCUAName)) ? m_configuration.ApplicationName : OPCUAName,
    5000,
    UserIdentity,
    new string[] { });


        // set up keep alive callback.
        m_session.KeepAlive += new KeepAliveEventHandler(Session_KeepAlive);

        // update the client status
        m_IsConnected = true;

        // raise an event.
        DoConnectComplete(null);
        //TODO:添加订阅
        AddSubscription(Guid.NewGuid().ToString(), Variables.ToArray());
        // return the new session.
        return m_session;
    }


    private void DoConnectComplete(object state)
    {
        m_ConnectComplete?.Invoke(this, null);
    }

    private async Task<OperResult<Dictionary<string, List<OPCNodeAttribute>>>> ReadNoteAttributeAsync(BrowseDescriptionCollection nodesToBrowse, ReadValueIdCollection nodesToRead, CancellationToken cancellationToken)
    {
        int startOfProperties = nodesToRead.Count;


        ReferenceDescriptionCollection references = await FormUtils.BrowseAsync(m_session, nodesToBrowse, false);

        if (references == null)
        {
            return new OperResult<Dictionary<string, List<OPCNodeAttribute>>>("浏览失败");
        }

        for (int ii = 0; ii < references.Count; ii++)
        {
            if (references[ii].NodeId.IsAbsolute)
            {
                continue;
            }

            ReadValueId nodeToRead = new ReadValueId();
            nodeToRead.NodeId = (NodeId)references[ii].NodeId;
            nodeToRead.AttributeId = Attributes.Value;
            nodesToRead.Add(nodeToRead);
        }

        var result = await m_session.ReadAsync(
            null,
            0,
            TimestampsToReturn.Neither,
            nodesToRead, cancellationToken);

        ClientBase.ValidateResponse(result.Results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(result.DiagnosticInfos, nodesToRead);

        Dictionary<string, List<OPCNodeAttribute>> nodeAttributes = new Dictionary<string, List<OPCNodeAttribute>>();
        for (int ii = 0; ii < result.Results.Count; ii++)
        {
            DataValue nodeValue = result.Results[ii];
            var nodeToRead = nodesToRead[ii];
            OPCNodeAttribute item = new OPCNodeAttribute();
            if (ii < startOfProperties)
            {
                if (nodeValue.StatusCode == StatusCodes.BadAttributeIdInvalid)
                {
                    continue;
                }

                item.Name = Attributes.GetBrowseName(nodesToRead[ii].AttributeId);
                if (StatusCode.IsBad(nodeValue.StatusCode))
                {
                    item.Type = Utils.Format("{0}", Attributes.GetDataTypeId(nodesToRead[ii].AttributeId));
                    item.Value = Utils.Format("{0}", nodeValue.StatusCode);
                }
                else
                {
                    TypeInfo typeInfo = TypeInfo.Construct(nodeValue.Value);
                    item.Type = typeInfo.BuiltInType.ToString();

                    if (typeInfo.ValueRank >= ValueRanks.OneOrMoreDimensions)
                    {
                        item.Type += "[]";
                    }
                    if (item.Name == nameof(Attributes.NodeClass))
                    {
                        item.Value = ((NodeClass)nodeValue.Value).ToString();
                    }
                    else if (item.Name == nameof(Attributes.EventNotifier))
                    {
                        item.Value = ((EventNotifierType)nodeValue.Value).ToString();
                    }
                    else
                        item.Value = nodeValue.Value;
                }
            }


            if (nodeAttributes.ContainsKey(nodeToRead.NodeId.ToString()))
            {
                nodeAttributes[nodeToRead.NodeId.ToString()].Add(item);
            }
            else
            {
                nodeAttributes.Add(nodeToRead.NodeId.ToString(), new() { item });
            }
        }

        return OperResult.CreateSuccessResult(nodeAttributes);
    }

    /// <summary>
    /// 连接处理器连接事件处理完成。
    /// </summary>
    private void Server_ReconnectComplete(object sender, EventArgs e)
    {

        // ignore callbacks from discarded objects.
        if (!Object.ReferenceEquals(sender, m_reConnectHandler))
        {
            return;
        }

        m_session = m_reConnectHandler.Session;
        m_reConnectHandler.SafeDispose();
        m_reConnectHandler = null;

        // raise any additional notifications.
        m_ReconnectComplete?.Invoke(this, e);

    }

    private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
    {
        if (checkLock.IsWaitting) { return; }
        checkLock.Lock();
        try
        {

            // check for events from discarded sessions.
            if (!Object.ReferenceEquals(session, m_session))
            {
                return;
            }

            // start reconnect sequence on communication error.
            if (ServiceResult.IsBad(e.Status))
            {
                if (OPCNode?.ReconnectPeriod <= 0)
                {
                    UpdateStatus(true, e.CurrentTime, "连接失败 ({0})", e.Status);
                    return;
                }

                UpdateStatus(true, e.CurrentTime, "重新连接中 in {0}s", OPCNode?.ReconnectPeriod);

                if (m_reConnectHandler == null)
                {
                    m_ReconnectStarting?.Invoke(this, e);

                    m_reConnectHandler = new SessionReconnectHandler();
                    m_reConnectHandler.BeginReconnect(m_session, (OPCNode?.ReconnectPeriod ?? 5000), Server_ReconnectComplete);
                }

                return;
            }

            // update status.
            UpdateStatus(false, e.CurrentTime, "连接正常 [{0}]", session.Endpoint.EndpointUrl);

            // raise any additional notifications.
            m_KeepAliveComplete?.Invoke(this, e);
        }
        finally
        {
            checkLock.UnLock();
        }
    }

    private void UpdateStatus(bool error, DateTime time, string status, params object[] args)
    {
        m_OpcStatusChange?.Invoke(this, new OPCUAStatusEventArgs()
        {
            Error = error,
            Time = time.ToLocalTime(),
            Text = String.Format(status, args),
        });
    }


    #endregion
}

using Opc.Ua;
using Opc.Ua.Client;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ThingsGateway.Foundation.Extension;

//https://github.com/dathlin/OpcUaHelper 修改自此代码库

namespace ThingsGateway.Foundation.Adapter.OPCUA;
public class OPCUAClient : DisposableObject
{

    public Action<List<(MonitoredItem monitoredItem, MonitoredItemNotification monitoredItemNotification)>> DataChangedHandler;
    public OPCNode OPCNode;
    private Dictionary<string, Subscription> dic_subscriptions;

    private ApplicationConfiguration m_configuration;

    private EventHandler m_ConnectComplete;

    private IntelligentConcurrentQueue<(MonitoredItem, MonitoredItemNotification)> m_data = new(100000);
    private bool m_IsConnected;

    private EventHandler m_KeepAliveComplete;

    private EventHandler<OPCUAStatusEventArgs> m_OpcStatusChange;

    private EventHandler m_ReconnectComplete;

    private SessionReconnectHandler m_reConnectHandler;

    private EventHandler m_ReconnectStarting;

    private ISession m_session;

    // 重连状态
    private bool m_useSecurity;

    /// <summary>
    /// 默认的构造函数，实例化一个新的OPC UA类
    /// </summary>
    public OPCUAClient()
    {
        dic_subscriptions = new Dictionary<string, Subscription>();

        var certificateValidator = new CertificateValidator();
        certificateValidator.CertificateValidation += (sender, eventArgs) =>
        {
            if (ServiceResult.IsGood(eventArgs.Error))
                eventArgs.Accept = true;
            else if (eventArgs.Error.StatusCode.Code == StatusCodes.BadCertificateUntrusted)
                eventArgs.Accept = true;
            else
                throw new Exception(string.Format("验证证书失败，错误代码:{0}: {1}", eventArgs.Error.Code, eventArgs.Error.AdditionalInfo));
        };

        SecurityConfiguration securityConfigurationcv = new SecurityConfiguration
        {
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
            ApplicationUri = nameof(OPCUAClient),
            ProductUri = nameof(OPCUAClient),

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
                    StorePath = "CurrentUser\\My",
                    SubjectName = OPCUAName,
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.X509Store,
                    StorePath = "CurrentUser\\Root",
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = CertificateStoreType.X509Store,
                    StorePath = "CurrentUser\\Root",
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

        EasyTask.Run(dataChangedHandlerInvoke);

    }

    /// <summary>
    /// 成功连接后或disconnecing从服务器。
    /// </summary>
    public event EventHandler ConnectComplete
    {
        add { m_ConnectComplete += value; }
        remove { m_ConnectComplete -= value; }
    }

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
    /// a name of application name show on server
    /// </summary>
    public string OPCUAName { get; set; } =typeof( OPCUAClient).FullName;



    /// <summary>
    /// 当前活动会话。
    /// </summary>
    public ISession Session
    {
        get { return m_session; }
    }
    /// <summary>
    /// The user identity to use when creating the session.
    /// </summary>
    public IUserIdentity UserIdentity { get; set; }

    /// <summary>
    /// Whether to use security when connecting.
    /// </summary>
    public bool UseSecurity
    {
        get { return m_useSecurity; }
        set { m_useSecurity = value; }
    }

    private Dictionary<string, List<string>> _tagDicts;
    public Dictionary<string, List<string>> SetTags(List<string> tags)
    {
        int i = 0;
        _tagDicts = tags.ChunkTrivialBetter(OPCNode.GroupSize).ToDictionary(a => "default" + (i++));
        return _tagDicts;
    }
    /// <summary>
    /// 新增一个订阅，需要指定订阅的关键字，订阅的tag名，以及回调方法
    /// </summary>
    /// <param name="key">关键字</param>
    /// <param name="tag">tag</param>
    /// <param name="callback">回调方法</param>
    public void AddSubscription(string key, string tag, Action<string, MonitoredItem, MonitoredItemNotificationEventArgs> callback)
    {
        AddSubscription(key, new string[] { tag }, callback);
    }

    /// <summary>
    /// 新增一批订阅，需要指定订阅的关键字，订阅的tag名数组，以及回调方法
    /// </summary>
    /// <param name="key">关键字</param>
    /// <param name="tags">节点名称数组</param>
    /// <param name="callback">回调方法</param>
    public void AddSubscription(string key, string[] tags, Action<string, MonitoredItem, MonitoredItemNotificationEventArgs> callback)
    {
        Subscription m_subscription = new Subscription(m_session.DefaultSubscription);

        m_subscription.PublishingEnabled = true;
        m_subscription.PublishingInterval = 0;
        m_subscription.KeepAliveCount = uint.MaxValue;
        m_subscription.LifetimeCount = uint.MaxValue;
        m_subscription.MaxNotificationsPerPublish = uint.MaxValue;
        m_subscription.Priority = 100;
        m_subscription.DisplayName = key;
        for (int i = 0; i < tags.Length; i++)
        {
            var item = new MonitoredItem
            {
                StartNodeId = new NodeId(tags[i]),
                AttributeId = Attributes.Value,
                DisplayName = tags[i],
                Filter = new DataChangeFilter() { DeadbandValue = OPCNode.DeadBand, DeadbandType = 1,Trigger=DataChangeTrigger.StatusValue },
                SamplingInterval = 100,
            };
            item.Notification += (MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args) =>
            {
                callback?.Invoke(key, monitoredItem, args);
            };
            m_subscription.AddItem(item);
        }

        m_session.AddSubscription(m_subscription);
        m_subscription.Create();

        lock (dic_subscriptions)
        {
            if (dic_subscriptions.ContainsKey(key))
            {
                // remove
                dic_subscriptions[key].Delete(true);
                m_session.RemoveSubscription(dic_subscriptions[key]);
                dic_subscriptions[key].Dispose();
                dic_subscriptions[key] = m_subscription;
            }
            else
            {
                dic_subscriptions.Add(key, m_subscription);
            }
        }
    }

    /// <summary>
    /// 浏览一个节点的引用
    /// </summary>
    /// <param name="tag">节点值</param>
    /// <returns>引用节点描述</returns>
    public ReferenceDescription[] BrowseNodeReference(string tag)
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

        // 该节点无论怎么样都读取不到方法
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
        ReferenceDescriptionCollection references = FormUtils.Browse(m_session, nodesToBrowse, false);

        return references.ToArray();
    }

    /// <summary>
    /// call a server method
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
    /// connect to server
    /// </summary>
    public async Task ConnectServer()
    {
        m_session = await Connect(OPCNode.OPCUrl);
    }

    /// <summary>
    /// 删除一个节点的操作，除非服务器配置允许，否则引发异常，成功返回<c>True</c>，否则返回<c>False</c>
    /// </summary>
    /// <param name="tag">节点文本描述</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteExsistNode(string tag)
    {
        DeleteNodesItemCollection waitDelete = new DeleteNodesItemCollection();

        DeleteNodesItem nodesItem = new DeleteNodesItem()
        {
            NodeId = new NodeId(tag),
        };

        m_session.DeleteNodes(
            null,
            waitDelete,
            out StatusCodeCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, waitDelete);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, waitDelete);

        return !StatusCode.IsBad(results[0]);
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
            m_reConnectHandler.Dispose();
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

        // raise an event.
        DoConnectComplete(null);
    }

    /// <summary>
    /// read History data
    /// </summary>
    /// <param name="tag">节点的索引</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="count">读取的个数</param>
    /// <param name="containBound">是否包含边界</param>
    /// <returns>读取的数据列表</returns>
    public IEnumerable<DataValue> ReadHistoryRawDataValues(string tag, DateTime start, DateTime end, uint count = 1, bool containBound = false)
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

        m_session.HistoryRead(
            null,
            new ExtensionObject(m_details),
            TimestampsToReturn.Both,
            false,
            nodesToRead,
            out HistoryReadResultCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        if (StatusCode.IsBad(results[0].StatusCode))
        {
            throw new ServiceResultException(results[0].StatusCode);
        }

        HistoryData values = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;
        foreach (var value in values.DataValues)
        {
            yield return value;
        }
    }

    /// <summary>
    /// 读取一连串的历史数据，并将其转化成指定的类型
    /// </summary>
    /// <param name="tag">节点的索引</param>
    /// <param name="start">开始时间</param>
    /// <param name="end">结束时间</param>
    /// <param name="count">读取的个数</param>
    /// <param name="containBound">是否包含边界</param>
    /// <returns>读取的数据列表</returns>
    public IEnumerable<T> ReadHistoryRawDataValues<T>(string tag, DateTime start, DateTime end, uint count = 1, bool containBound = false)
    {
        HistoryReadValueId m_nodeToContinue = new HistoryReadValueId()
        {
            NodeId = new NodeId(tag),
        };

        ReadRawModifiedDetails m_details = new ReadRawModifiedDetails
        {
            StartTime = start.ToUniversalTime(),
            EndTime = end.ToUniversalTime(),
            NumValuesPerNode = count,
            IsReadModified = false,
            ReturnBounds = containBound
        };

        HistoryReadValueIdCollection nodesToRead = new HistoryReadValueIdCollection();
        nodesToRead.Add(m_nodeToContinue);

        m_session.HistoryRead(
            null,
            new ExtensionObject(m_details),
            TimestampsToReturn.Both,
            false,
            nodesToRead,
            out HistoryReadResultCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        if (StatusCode.IsBad(results[0].StatusCode))
        {
            throw new ServiceResultException(results[0].StatusCode);
        }

        HistoryData values = ExtensionObject.ToEncodeable(results[0].HistoryData) as HistoryData;
        foreach (var value in values.DataValues)
        {
            yield return (T)value.Value;
        }
    }

    /// <summary>
    /// Read a value node from server
    /// </summary>
    /// <typeparam name="T">type of value</typeparam>
    /// <param name="tag">node id</param>
    /// <returns>实际值</returns>
    public T ReadNode<T>(string tag)
    {
        var dataValue = ReadNode(new NodeId(tag));
        return (T)dataValue.FirstOrDefault().Value;
    }

    /// <summary>
    /// read several value nodes from server
    /// </summary>
    /// <param name="nodeIds">all NodeIds</param>
    /// <returns>all values</returns>
    public List<DataValue> ReadNode(params NodeId[] nodeIds)
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
        m_session.Read(
            null,
            0,
            TimestampsToReturn.Neither,
            nodesToRead,
            out DataValueCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        return results.ToList();
    }

    /// <summary>
    /// read several value nodes from server
    /// </summary>
    /// <param name="tags">所以的节点数组信息</param>
    /// <returns>all values</returns>
    public List<T> ReadNode<T>(params string[] tags)
    {
        List<T> result = new List<T>();
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
        for (int i = 0; i < tags.Length; i++)
        {
            nodesToRead.Add(new ReadValueId()
            {
                NodeId = new NodeId(tags[i]),
                AttributeId = Attributes.Value
            });
        }

        // 读取当前的值
        m_session.Read(
            null,
            0,
            TimestampsToReturn.Neither,
            nodesToRead,
            out DataValueCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        foreach (var item in results)
        {
            result.Add((T)item.Value);
        }
        return result;
    }

    /// <summary>
    /// Read a tag asynchronously
    /// </summary>
    /// <typeparam name="T">The type of tag to read</typeparam>
    /// <param name="tag">tag值</param>
    /// <returns>The value retrieved from the OPC</returns>
    public Task<T> ReadNodeAsync<T>(string tag)
    {
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection
            {
                new ReadValueId()
                {
                    NodeId = new NodeId(tag),
                    AttributeId = Attributes.Value
                }
            };

        // Wrap the ReadAsync logic in a TaskCompletionSource, so we can use C# async/await syntax to call it:
        var taskCompletionSource = new TaskCompletionSource<T>();
        m_session.BeginRead(
            requestHeader: null,
            maxAge: 0,
            timestampsToReturn: TimestampsToReturn.Neither,
            nodesToRead: nodesToRead,
            callback: ar =>
            {
                DataValueCollection results;
                DiagnosticInfoCollection diag;
                var response = m_session.EndRead(
                  result: ar,
                  results: out results,
                  diagnosticInfos: out diag);

                try
                {
                    CheckReturnValue(response.ServiceResult);
                    CheckReturnValue(results[0].StatusCode);
                    var val = results[0];
                    taskCompletionSource.TrySetResult((T)val.Value);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            },
            asyncState: null);

        return taskCompletionSource.Task;
    }

    /// <summary>
    /// read several value nodes from server
    /// </summary>
    /// <param name="nodeIds">all NodeIds</param>
    /// <returns>all values</returns>
    public Task<List<DataValue>> ReadNodeAsync(params NodeId[] nodeIds)
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

        var taskCompletionSource = new TaskCompletionSource<List<DataValue>>();
        // 读取当前的值
        m_session.BeginRead(
            null,
            0,
            TimestampsToReturn.Neither,
            nodesToRead,
            callback: ar =>
            {
                DataValueCollection results;
                DiagnosticInfoCollection diag;
                var response = m_session.EndRead(
                  result: ar,
                  results: out results,
                  diagnosticInfos: out diag);

                try
                {
                    CheckReturnValue(response.ServiceResult);
                    taskCompletionSource.TrySetResult(results.ToList());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            },
            asyncState: null);

        return taskCompletionSource.Task;
    }
    /// <summary>
    /// read several value nodes from server
    /// </summary>
    /// <param name="nodeIds">all NodeIds</param>
    /// <returns>all values</returns>
    public Task<List<DataValue>> ReadNodeAsync(params string[] nodeIds)
    {
        return ReadNodeAsync(nodeIds.Select(a => new NodeId(a)).ToArray());
    }

    /// <summary>
    /// read several value nodes from server
    /// </summary>
    /// <param name="tags">all NodeIds</param>
    /// <returns>all values</returns>
    public Task<List<T>> ReadNodeAsync<T>(params string[] tags)
    {
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
        for (int i = 0; i < tags.Length; i++)
        {
            nodesToRead.Add(new ReadValueId()
            {
                NodeId = new NodeId(tags[i]),
                AttributeId = Attributes.Value
            });
        }

        var taskCompletionSource = new TaskCompletionSource<List<T>>();
        // 读取当前的值
        m_session.BeginRead(
            null,
            0,
            TimestampsToReturn.Neither,
            nodesToRead,
            callback: ar =>
            {
                DataValueCollection results;
                DiagnosticInfoCollection diag;
                var response = m_session.EndRead(
                  result: ar,
                  results: out results,
                  diagnosticInfos: out diag);

                try
                {
                    CheckReturnValue(response.ServiceResult);
                    List<T> result = new List<T>();
                    foreach (var item in results)
                    {
                        result.Add((T)item.Value);
                    }
                    taskCompletionSource.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            },
            asyncState: null);

        return taskCompletionSource.Task;
    }

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
    /// <param name="tag">节点值</param>
    /// <returns>所有的数据</returns>
    public DataValue[] ReadNoteDataValueAttributes(string tag)
    {
        NodeId sourceId = new NodeId(tag);
        ReadValueIdCollection nodesToRead = new ReadValueIdCollection();

        // attempt to read all possible attributes.
        // 尝试着去读取所有可能的特性
        for (uint ii = Attributes.NodeId; ii <= Attributes.UserExecutable; ii++)
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
            return new DataValue[0];
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

        return results.ToArray();
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
                item.Value.Dispose();
            }
            dic_subscriptions.Clear();
        }
    }

    /// <summary>
    /// 移除订阅消息，如果该订阅消息是批量的，也直接移除
    /// </summary>
    /// <param name="key">订阅关键值</param>
    public void RemoveSubscription(string key)
    {
        lock (dic_subscriptions)
        {
            if (dic_subscriptions.ContainsKey(key))
            {
                // remove
                dic_subscriptions[key].Delete(true);
                m_session.RemoveSubscription(dic_subscriptions[key]);
                dic_subscriptions[key].Dispose();
                dic_subscriptions.Remove(key);
            }
        }
    }

    /// <summary>
    /// 设置OPC客户端的日志输出
    /// </summary>
    /// <param name="filePath">完整的文件路径</param>
    /// <param name="deleteExisting">是否删除原文件</param>
    public void SetLogPathName(string filePath, bool deleteExisting)
    {
        Utils.SetTraceLog(filePath, deleteExisting);
        Utils.SetTraceMask(515);
    }

    /// <summary>
    /// write a note to server(you should use try catch)
    /// </summary>
    /// <typeparam name="T">The type of tag to write on</typeparam>
    /// <param name="tag">节点名称</param>
    /// <param name="value">值</param>
    /// <returns>if success True,otherwise False</returns>
    public bool WriteNode<T>(string tag, T value)
    {
        WriteValue valueToWrite = new WriteValue()
        {
            NodeId = new NodeId(tag),
            AttributeId = Attributes.Value
        };
        valueToWrite.Value.Value = value;
        valueToWrite.Value.StatusCode = StatusCodes.Good;
        valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
        valueToWrite.Value.SourceTimestamp = DateTime.MinValue;

        WriteValueCollection valuesToWrite = new WriteValueCollection
            {
                valueToWrite
            };

        // 写入当前的值

        m_session.Write(
            null,
            valuesToWrite,
            out StatusCodeCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, valuesToWrite);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, valuesToWrite);

        if (StatusCode.IsBad(results[0]))
        {
            throw new ServiceResultException(results[0]);
        }

        return !StatusCode.IsBad(results[0]);
    }

    /// <summary>
    /// Write a value on the specified opc tag asynchronously
    /// </summary>
    /// <typeparam name="T">The type of tag to write on</typeparam>
    /// <param name="tag">The fully-qualified identifier of the tag. You can specify a subfolder by using a comma delimited name. E.g: the tag `foo.bar` writes on the tag `bar` on the folder `foo`</param>
    /// <param name="value">The value for the item to write</param>
    public Task<bool> WriteNodeAsync<T>(string tag, T value)
    {
        WriteValue valueToWrite = new WriteValue()
        {
            NodeId = new NodeId(tag),
            AttributeId = Attributes.Value,
        };
        valueToWrite.Value.Value = value;
        valueToWrite.Value.StatusCode = StatusCodes.Good;
        valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
        valueToWrite.Value.SourceTimestamp = DateTime.MinValue;
        WriteValueCollection valuesToWrite = new WriteValueCollection
            {
                valueToWrite
            };

        // Wrap the WriteAsync logic in a TaskCompletionSource, so we can use C# async/await syntax to call it:
        var taskCompletionSource = new TaskCompletionSource<bool>();
        m_session.BeginWrite(
            requestHeader: null,
            nodesToWrite: valuesToWrite,
            callback: ar =>
            {
                var response = m_session.EndWrite(
                  result: ar,
                  results: out StatusCodeCollection results,
                  diagnosticInfos: out DiagnosticInfoCollection diag);

                try
                {
                    ClientBase.ValidateResponse(results, valuesToWrite);
                    ClientBase.ValidateDiagnosticInfos(diag, valuesToWrite);
                    taskCompletionSource.SetResult(StatusCode.IsGood(results[0]));
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            },
            asyncState: null);
        return taskCompletionSource.Task;
    }

    /// <summary>
    /// 所有的节点都写入成功，返回<c>True</c>，否则返回<c>False</c>
    /// </summary>
    /// <param name="tags">节点名称数组</param>
    /// <param name="values">节点的值数据</param>
    /// <returns>所有的是否都写入成功</returns>
    public bool WriteNodes(string[] tags, object[] values)
    {
        WriteValueCollection valuesToWrite = new WriteValueCollection();

        for (int i = 0; i < tags.Length; i++)
        {
            if (i < values.Length)
            {
                WriteValue valueToWrite = new WriteValue()
                {
                    NodeId = new NodeId(tags[i]),
                    AttributeId = Attributes.Value
                };
                valueToWrite.Value.Value = values[i];
                valueToWrite.Value.StatusCode = StatusCodes.Good;
                valueToWrite.Value.ServerTimestamp = DateTime.MinValue;
                valueToWrite.Value.SourceTimestamp = DateTime.MinValue;
                valuesToWrite.Add(valueToWrite);
            }
        }

        // 写入当前的值

        m_session.Write(
            null,
            valuesToWrite,
            out StatusCodeCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, valuesToWrite);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, valuesToWrite);

        bool result = true;
        foreach (var r in results)
        {
            if (StatusCode.IsBad(r))
            {
                result = false;
                break;
            }
        }

        return result;
    }

    protected override void Dispose(bool disposing)
    {
        Disconnect();
        base.Dispose(disposing);
    }

    private void callback(string arg1, MonitoredItem arg2, MonitoredItemNotificationEventArgs arg3)
    {
        var data = arg3.NotificationValue as MonitoredItemNotification;
        m_data.Enqueue((arg2,data));
    }
    private void CheckReturnValue(StatusCode status)
    {
        if (!StatusCode.IsGood(status))
            throw new Exception(string.Format("无效值(状态: {0})", status));
    }

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <returns>The new session object.</returns>
    private async Task<ISession> Connect(string serverUrl)
    {
        // disconnect from existing session.
        Disconnect();

        if (m_configuration == null)
        {
            throw new ArgumentNullException("未初始化配置");
        }

        // select the best endpoint.
        EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(serverUrl, UseSecurity);
        EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(m_configuration);

        ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

        m_session = await Opc.Ua.Client.Session.Create(
            m_configuration,
            endpoint,
            false,
            false,
            (string.IsNullOrEmpty(OPCUAName)) ? m_configuration.ApplicationName : OPCUAName,
            60000,
            UserIdentity,
            new string[] { });

        // set up keep alive callback.
        m_session.KeepAlive += new KeepAliveEventHandler(Session_KeepAlive);

        // update the client status
        m_IsConnected = true;

        // raise an event.
        DoConnectComplete(null);
        //添加订阅
        foreach (var item in _tagDicts)
        {
            AddSubscription(item.Key, item.Value.ToArray(), callback);
        }
        // return the new session.
        return m_session;
    }

    private async void dataChangedHandlerInvoke()
    {
        while (!DisposedValue)
        {
            if (m_data.Count > 0)
                DataChangedHandler?.Invoke(m_data.ToListWithDequeue(m_data.Count));
            if (OPCNode == null)
                await Task.Delay(1000);
            else
                await Task.Delay(OPCNode.UpdateRate == 0 ? 1000 : OPCNode.UpdateRate);
        }
    }
    /// <summary>
    /// Raises the connect complete event on the main GUI thread.
    /// </summary>
    private void DoConnectComplete(object state)
    {
        m_ConnectComplete?.Invoke(this, null);
    }

    /// <summary>
    /// Handles a reconnect event complete from the reconnect handler.
    /// </summary>
    private void Server_ReconnectComplete(object sender, EventArgs e)
    {

        // ignore callbacks from discarded objects.
        if (!Object.ReferenceEquals(sender, m_reConnectHandler))
        {
            return;
        }

        m_session = m_reConnectHandler.Session;
        m_reConnectHandler.Dispose();
        m_reConnectHandler = null;

        // raise any additional notifications.
        m_ReconnectComplete?.Invoke(this, e);

    }

    private EasyLock _keepAliveLock = new();
    /// <summary>
    /// Handles a keep alive event from a session.
    /// </summary>
    private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
    {
        if (_keepAliveLock.IsWaitting) { return; }
        _keepAliveLock.Lock();
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
                    m_reConnectHandler.BeginReconnect(m_session, (OPCNode?.ReconnectPeriod ?? 5000) , Server_ReconnectComplete);
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
            _keepAliveLock.UnLock();
        }
    }

    /// <summary>
    /// Report the client status
    /// </summary>
    /// <param name="error">Whether the status represents an error.</param>
    /// <param name="time">The time associated with the status.</param>
    /// <param name="status">The status message.</param>
    /// <param name="args">Arguments used to format the status message.</param>
    private void UpdateStatus(bool error, DateTime time, string status, params object[] args)
    {
        m_OpcStatusChange?.Invoke(this, new OPCUAStatusEventArgs()
        {
            Error = error,
            Time = time.ToLocalTime(),
            Text = String.Format(status, args),
        });
    }

}

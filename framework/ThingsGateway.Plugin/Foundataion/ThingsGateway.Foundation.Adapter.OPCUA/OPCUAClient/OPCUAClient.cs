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
using Opc.Ua.Client.ComplexTypes;
using Opc.Ua.Configuration;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Foundation.Extension;


//修改自https://github.com/dathlin/OpcUaHelper 与OPC基金会net库

namespace ThingsGateway.Foundation.Adapter.OPCUA;
/// <summary>
/// 订阅委托
/// </summary>
/// <param name="value"></param>
public delegate void DataChangedEventHandler((VariableNode variableNode, DataValue dataValue, JToken jToken) value);
/// <summary>
/// OPCUAClient
/// </summary>
public class OPCUAClient : DisposableObject
{

    #region 属性，变量等
    /// <summary>
    /// 当前配置
    /// </summary>
    public OPCNode OPCNode;

    /// <summary>
    /// ProductUri
    /// </summary>
    public string ProductUri = "https://diego2098.gitee.io/thingsgateway-docs/";

    /// <summary>
    /// 当前保存的变量名称列表
    /// </summary>
    public List<string> Variables = new();

    /// <summary>
    /// 当前的变量名称/OPC变量节点
    /// </summary>
    private readonly Dictionary<string, VariableNode> _variableDicts = new();
    private readonly EasyLock checkLock = new();
    /// <summary>
    /// 当前的订阅组，组名称/组
    /// </summary>
    private readonly Dictionary<string, Subscription> dic_subscriptions = new();

    private readonly ILog Log;
    private readonly ApplicationInstance m_application = new();

    private readonly ApplicationConfiguration m_configuration;

    private SessionReconnectHandler m_reConnectHandler;
    private ISession m_session;
    /// <summary>
    /// 默认的构造函数，实例化一个新的OPC UA类
    /// </summary>
    public OPCUAClient(ILog log)
    {
        Log = log;
        var certificateValidator = new CertificateValidator();
        certificateValidator.CertificateValidation += CertificateValidation;
        SecurityConfiguration securityConfigurationcv = new()
        {
            UseValidatedCertificates = true,
            AutoAcceptUntrustedCertificates = true,//自动接受证书
            RejectSHA1SignedCertificates = false,
            MinimumCertificateKeySize = 1024,
        };
        certificateValidator.Update(securityConfigurationcv);

        // 构建应用程序配置
        m_configuration = new ApplicationConfiguration
        {
            ApplicationName = OPCUAName,
            ApplicationType = ApplicationType.Client,
            CertificateValidator = certificateValidator,
            ApplicationUri = Utils.Format(@"urn:{0}:{1}", System.Net.Dns.GetHostName(), OPCUAName),
            ProductUri = ProductUri,

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
                    StorePath = "CurrentUser\\" + OPCUAName,
                    SubjectName = $"CN={OPCUAName}, C=CN, S=GUANGZHOU, O=ThingsGateway, DC=" + System.Net.Dns.GetHostName(),
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

        m_configuration.Validate(ApplicationType.Client);
        m_application.ApplicationConfiguration = m_configuration;


    }

    /// <summary>
    /// 订阅
    /// </summary>
    public event DataChangedEventHandler DataChangedHandler;

    /// <summary>
    /// 配置信息
    /// </summary>
    public ApplicationConfiguration AppConfig => m_configuration;

    /// <summary>
    /// 连接状态
    /// </summary>
    public bool Connected => m_session?.Connected == true;

    /// <summary>
    /// OPCUAClient
    /// </summary>
    public string OPCUAName { get; set; } = "ThingsGateway";

    /// <summary>
    /// 当前活动会话。
    /// </summary>
    public ISession Session => m_session;

    #endregion

    #region 订阅

    /// <summary>
    /// 新增订阅，需要指定订阅组名称，订阅的tag名数组
    /// </summary>
    public async Task AddSubscriptionAsync(string subscriptionName, string[] items)
    {
        Subscription m_subscription = new(m_session.DefaultSubscription)
        {
            PublishingEnabled = true,
            PublishingInterval = 0,
            KeepAliveCount = uint.MaxValue,
            LifetimeCount = uint.MaxValue,
            MaxNotificationsPerPublish = uint.MaxValue,
            Priority = 100,
            DisplayName = subscriptionName
        };
        List<MonitoredItem> monitoredItems = new();
        for (int i = 0; i < items.Length; i++)
        {
            try
            {
                var variableNode = (VariableNode)ReadNode(items[i], false);
                var item = new MonitoredItem
                {
                    StartNodeId = variableNode.NodeId,
                    AttributeId = Attributes.Value,
                    DisplayName = items[i],
                    Filter = OPCNode.DeadBand == 0 ? null : new DataChangeFilter() { DeadbandValue = OPCNode.DeadBand, DeadbandType = (int)DeadbandType.Absolute, Trigger = DataChangeTrigger.StatusValue },
                    SamplingInterval = OPCNode?.UpdateRate ?? 1000,
                };
                await typeSystem.LoadType(variableNode.DataType, true, true);

                item.Notification += Callback;
                monitoredItems.Add(item);
            }
            catch (Exception ex)
            {
                Log.Error($"初始化{items[i]}变量订阅失败" + ex.Message);
            }
        }
        m_subscription.AddItems(monitoredItems);

        m_session.AddSubscription(m_subscription);
        m_subscription.Create();
        foreach (var item in m_subscription.MonitoredItems.Where(a => a.Status.Error != null && StatusCode.IsBad(a.Status.Error.StatusCode)))
        {
            item.Filter = OPCNode.DeadBand == 0 ? null : new DataChangeFilter() { DeadbandValue = OPCNode.DeadBand, DeadbandType = (int)DeadbandType.None, Trigger = DataChangeTrigger.StatusValue };
        }
        m_subscription.ApplyChanges();

        var iserror = m_subscription.MonitoredItems.Any(a => a.Status.Error != null && StatusCode.IsBad(a.Status.Error.StatusCode));
        if (iserror)
        {
            Log.Error("创建以下变量订阅失败" + m_subscription.MonitoredItems.Where(a => a.Status.Error != null && StatusCode.IsBad(a.Status.Error.StatusCode)).Select(a => a.StartNodeId.ToString() + "：" + a.Status.Error.ToString()).ToJson());
        }

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
    /// <param name="subscriptionName">组名称</param>
    public void RemoveSubscription(string subscriptionName)
    {
        lock (dic_subscriptions)
        {
            if (dic_subscriptions.ContainsKey(subscriptionName))
            {
                // remove
                dic_subscriptions[subscriptionName].Delete(true);
                m_session.RemoveSubscription(dic_subscriptions[subscriptionName]);
                dic_subscriptions[subscriptionName].SafeDispose();
                dic_subscriptions.RemoveWhere(a => a.Key == subscriptionName);
            }
        }

    }

    private void Callback(MonitoredItem monitoreditem, MonitoredItemNotificationEventArgs monitoredItemNotificationEventArgs)
    {
        try
        {
            VariableNode variableNode = (VariableNode)ReadNode(monitoreditem.StartNodeId.ToString(), false);
            foreach (var value in monitoreditem.DequeueValues())
            {
                if (value.Value != null)
                {
                    var data = JsonUtils.Encode(m_session.MessageContext, TypeInfo.GetBuiltInType(variableNode.DataType, m_session.SystemContext.TypeTable), value.Value);
                    if (data == null && value.Value != null)
                    {
                        Log.Warning($"{monitoreditem.StartNodeId}转换出错，原始值String为{value.Value}");
                    }
                    DataChangedHandler?.Invoke((variableNode, value, data));
                }
                else
                {
                    var data = JValue.CreateNull();
                    DataChangedHandler?.Invoke((variableNode, value, data));
                }

            }
        }
        catch (Exception ex)
        {
            Log.Warning($"{monitoreditem.StartNodeId}订阅事件出错：{ex.Message}");
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
        NodeId sourceId = new(tag);

        // 该节点可以读取到方法
        BrowseDescription nodeToBrowse1 = new()
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.Aggregates,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method),
            ResultMask = (uint)BrowseResultMask.All
        };

        // find all nodes organized by the node.
        BrowseDescription nodeToBrowse2 = new()
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.Organizes,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable),
            ResultMask = (uint)BrowseResultMask.All
        };

        BrowseDescriptionCollection nodesToBrowse = new()
        {
            nodeToBrowse1,
            nodeToBrowse2
        };

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
    /// <param name="token">token</param>
    /// <returns>读取的数据列表</returns>
    public async Task<List<DataValue>> ReadHistoryRawDataValues(string tag, DateTime start, DateTime end, uint count = 1, bool containBound = false, CancellationToken token = default)
    {
        HistoryReadValueId m_nodeToContinue = new()
        {
            NodeId = new NodeId(tag),
        };

        ReadRawModifiedDetails m_details = new()
        {
            StartTime = start,
            EndTime = end,
            NumValuesPerNode = count,
            IsReadModified = false,
            ReturnBounds = containBound
        };

        HistoryReadValueIdCollection nodesToRead = new()
        {
            m_nodeToContinue
        };

        var result = await m_session.HistoryReadAsync(
             null,
             new ExtensionObject(m_details),
             TimestampsToReturn.Both,
             false,
             nodesToRead,
             token);
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
        await ConnectAsync(OPCNode.OPCUrl);
    }

    /// <summary>
    /// 断开连接。
    /// </summary>
    public void Disconnect()
    {
        PrivateDisconnect();
        // disconnect any existing session.
        if (m_session != null)
        {
            Log.Debug("断开连接");
            m_session = null;
        }
    }

    private void PrivateDisconnect()
    {
        // stop any reconnect operation.
        if (m_reConnectHandler != null)
        {
            m_reConnectHandler.SafeDispose();
            m_reConnectHandler = null;
        }
        m_session?.Close(10000);


    }
    #endregion

    #region 读取/写入



    /// <summary>
    /// 从服务器读取值
    /// </summary>
    public async Task<List<(string, DataValue, JToken)>> ReadJTokenValueAsync(string[] tags, CancellationToken token = default)
    {
        var result = await ReadJTokenValueAsync(tags.Select(a => new NodeId(a)).ToArray(), token);
        return result;
    }

    /// <summary>
    /// 异步写opc标签
    /// </summary>
    public async Task<Dictionary<string, OperResult>> WriteNodeAsync(Dictionary<string, JToken> writeInfoLists, CancellationToken token = default)
    {
        Dictionary<string, OperResult> results = new();
        try
        {
            WriteValueCollection valuesToWrite = new();
            foreach (var item in writeInfoLists)
            {
                WriteValue valueToWrite = new()
                {
                    NodeId = new NodeId(item.Key),
                    AttributeId = Attributes.Value,
                };
                var variableNode = (VariableNode)ReadNode(item.Key, false);
                await typeSystem.LoadType(variableNode.DataType, true, true);
                var dataValue = JsonUtils.Decode(
                    m_session.MessageContext,
                    variableNode.DataType,
                    TypeInfo.GetBuiltInType(variableNode.DataType, m_session.SystemContext.TypeTable),
                    item.Value.CalculateActualValueRank(),
                    item.Value
                    );
                valueToWrite.Value = dataValue;

                valuesToWrite.Add(valueToWrite);
            }


            var result = await m_session.WriteAsync(
     requestHeader: null,
     nodesToWrite: valuesToWrite, token);

            ClientBase.ValidateResponse(result.Results, valuesToWrite);
            ClientBase.ValidateDiagnosticInfos(result.DiagnosticInfos, valuesToWrite);


            var keys = writeInfoLists.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                if (!StatusCode.IsGood(result.Results[i]))
                    results.Add(keys[i], new(result.Results[i].ToString()));
                else
                {
                    results.Add(keys[i], OperResult.CreateSuccessResult());
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            var keys = writeInfoLists.Keys.ToList();
            foreach (var item in keys)
            {
                results.Add(item, new(ex));
            }
            return results;
        }

    }

    /// <summary>
    /// 从服务器读取值
    /// </summary>
    private async Task<List<(string, DataValue, JToken)>> ReadJTokenValueAsync(NodeId[] nodeIds, CancellationToken token = default)
    {
        if (m_session == null)
        {
            throw new("服务器未初始化连接");
        }
        ReadValueIdCollection nodesToRead = new();
        for (int i = 0; i < nodeIds.Length; i++)
        {
            var variableNode = (VariableNode)ReadNode(nodeIds[i].ToString(), false);
            await typeSystem.LoadType(variableNode.DataType, true, true);
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
             token);
        var results = result.Results;
        var diagnosticInfos = result.DiagnosticInfos;
        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);
        List<(string, DataValue, JToken)> jTokens = new();
        for (int i = 0; i < results.Count; i++)
        {
            var variableNode = (VariableNode)ReadNode(nodeIds[i].ToString(), false);
            var type = TypeInfo.GetBuiltInType(variableNode.DataType, m_session.SystemContext.TypeTable);
            var jToken = JsonUtils.Encode(m_session.MessageContext, type, results[i].Value);
            jTokens.Add((variableNode.NodeId.ToString(), results[i], jToken));
        }
        return jTokens.ToList();
    }

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

        NodeId nodeToRead = new(nodeIdStr);
        var node = m_session.ReadNode(nodeToRead);
        _variableDicts.AddOrUpdate(nodeIdStr, (VariableNode)node);
        return node;
    }
    #endregion


    #region 特性

    /// <summary>
    /// 读取一个节点的所有属性
    /// </summary>
    public async Task<OperResult<List<OPCNodeAttribute>>> ReadNoteAttributeAsync(string tag, uint attributesId, CancellationToken token = default)
    {
        BrowseDescriptionCollection nodesToBrowse = new();
        ReadValueIdCollection nodesToRead = new();
        NodeId sourceId = new(tag);

        ReadValueId nodeToRead = new()
        {
            NodeId = sourceId,
            AttributeId = attributesId
        };
        nodesToRead.Add(nodeToRead);
        BrowseDescription nodeToBrowse = new()
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.HasProperty,
            IncludeSubtypes = true,
            NodeClassMask = 0,
            ResultMask = (uint)BrowseResultMask.All
        };
        nodesToBrowse.Add(nodeToBrowse);

        var result1 = await ReadNoteAttributeAsync(nodesToBrowse, nodesToRead, token);
        var result2 = result1.Copy<List<OPCNodeAttribute>>();
        result2.Content = result1.Content?.Values?.FirstOrDefault()?.ToList();
        return result2;
    }

    /// <summary>
    /// 读取节点的所有属性
    /// </summary>
    public async Task<OperResult<Dictionary<string, List<OPCNodeAttribute>>>> ReadNoteAttributeAsync(List<string> tags, CancellationToken token)
    {
        BrowseDescriptionCollection nodesToBrowse = new();
        ReadValueIdCollection nodesToRead = new();
        foreach (var tag in tags)
        {
            NodeId sourceId = new(tag);

            for (uint ii = Attributes.NodeClass; ii <= Attributes.UserExecutable; ii++)
            {
                ReadValueId nodeToRead = new()
                {
                    NodeId = sourceId,
                    AttributeId = ii
                };
                nodesToRead.Add(nodeToRead);
            }
            BrowseDescription nodeToBrowse = new()
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

        return await ReadNoteAttributeAsync(nodesToBrowse, nodesToRead, token);
    }

    /// <summary>
    /// 读取一个节点的所有属性
    /// </summary>
    /// <param name="tag">节点信息</param>
    /// <returns>节点的特性值</returns>
    public OPCNodeAttribute[] ReadNoteAttributes(string tag)
    {
        NodeId sourceId = new(tag);
        ReadValueIdCollection nodesToRead = new();

        for (uint ii = Attributes.NodeClass; ii <= Attributes.UserExecutable; ii++)
        {
            ReadValueId nodeToRead = new()
            {
                NodeId = sourceId,
                AttributeId = ii
            };
            nodesToRead.Add(nodeToRead);
        }

        int startOfProperties = nodesToRead.Count;

        // find all of the pror of the node.
        BrowseDescription nodeToBrowse1 = new()
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.HasProperty,
            IncludeSubtypes = true,
            NodeClassMask = 0,
            ResultMask = (uint)BrowseResultMask.All
        };

        BrowseDescriptionCollection nodesToBrowse = new()
        {
            nodeToBrowse1
        };

        // fetch property references from the server.
        ReferenceDescriptionCollection references = FormUtils.Browse(m_session, nodesToBrowse, false);

        if (references == null)
        {
            return Array.Empty<OPCNodeAttribute>();
        }

        for (int ii = 0; ii < references.Count; ii++)
        {
            // ignore external references.
            if (references[ii].NodeId.IsAbsolute)
            {
                continue;
            }

            ReadValueId nodeToRead = new()
            {
                NodeId = (NodeId)references[ii].NodeId,
                AttributeId = Attributes.Value
            };
            nodesToRead.Add(nodeToRead);
        }

        // read all values.

        m_session.Read(
            null,
            0,
            TimestampsToReturn.Neither,
            nodesToRead,
            out DataValueCollection results,
            out DiagnosticInfoCollection diagnosticInfos);

        ClientBase.ValidateResponse(results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

        // process results.

        List<OPCNodeAttribute> nodeAttribute = new();
        for (int ii = 0; ii < results.Count; ii++)
        {
            OPCNodeAttribute item = new();

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

                    item.Value = results[ii].Value;
                }
            }

            nodeAttribute.Add(item);
        }

        return nodeAttribute.ToArray();
    }
    #endregion




    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        Disconnect();
        checkLock.SafeDispose();
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
    private ComplexTypeSystem typeSystem;
    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <returns>The new session object.</returns>
    private async Task<ISession> ConnectAsync(string serverUrl)
    {
        PrivateDisconnect();

        if (m_configuration == null)
        {
            throw new ArgumentNullException("未初始化配置");
        }
        var useSecurity = OPCNode?.IsUseSecurity ?? true;
        EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(serverUrl, useSecurity);
        EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(m_configuration);
        UserIdentity userIdentity;
        ConfiguredEndpoint endpoint = new(null, endpointDescription, endpointConfiguration);
        if (!OPCNode.UserName.IsNullOrEmpty())
        {
            userIdentity = new UserIdentity(OPCNode.UserName, OPCNode.Password);
        }
        else
        {
            userIdentity = new UserIdentity(new AnonymousIdentityToken());
        }
        //创建本地证书
        await m_application.CheckApplicationInstanceCertificate(true, 0, 1200);
        m_session = await Opc.Ua.Client.Session.Create(
     m_configuration,
    endpoint,
    false,
    false,
    (string.IsNullOrEmpty(OPCUAName)) ? m_configuration.ApplicationName : OPCUAName,
    60000,
    userIdentity,
    Array.Empty<string>());

        typeSystem = new ComplexTypeSystem(m_session);
        Log.Debug("连接成功");

        m_session.KeepAliveInterval = OPCNode.KeepAliveInterval == 0 ? 60000 : OPCNode.KeepAliveInterval;
        m_session.KeepAlive += new KeepAliveEventHandler(Session_KeepAlive);

        //如果是订阅模式，连接时添加订阅组
        if (OPCNode.ActiveSubscribe)
            await AddSubscriptionAsync(Guid.NewGuid().ToString(), Variables.ToArray());
        return m_session;
    }


    private async Task<OperResult<Dictionary<string, List<OPCNodeAttribute>>>> ReadNoteAttributeAsync(BrowseDescriptionCollection nodesToBrowse, ReadValueIdCollection nodesToRead, CancellationToken token)
    {
        int startOfProperties = nodesToRead.Count;

        ReferenceDescriptionCollection references = await FormUtils.BrowseAsync(m_session, nodesToBrowse, false, token);

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

            ReadValueId nodeToRead = new()
            {
                NodeId = (NodeId)references[ii].NodeId,
                AttributeId = Attributes.Value
            };
            nodesToRead.Add(nodeToRead);
        }

        var result = await m_session.ReadAsync(
            null,
            0,
            TimestampsToReturn.Neither,
            nodesToRead, token);

        ClientBase.ValidateResponse(result.Results, nodesToRead);
        ClientBase.ValidateDiagnosticInfos(result.DiagnosticInfos, nodesToRead);

        Dictionary<string, List<OPCNodeAttribute>> nodeAttributes = new();
        for (int ii = 0; ii < result.Results.Count; ii++)
        {
            DataValue nodeValue = result.Results[ii];
            var nodeToRead = nodesToRead[ii];
            OPCNodeAttribute item = new();
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
        if (!Object.ReferenceEquals(sender, m_reConnectHandler))
        {
            return;
        }

        m_session = m_reConnectHandler.Session;
        m_reConnectHandler.SafeDispose();
        m_reConnectHandler = null;

    }

    private void Session_KeepAlive(ISession session, KeepAliveEventArgs e)
    {
        if (checkLock.IsWaitting) { return; }
        checkLock.Wait();
        try
        {
            if (!Object.ReferenceEquals(session, m_session))
            {
                return;
            }

            if (ServiceResult.IsBad(e.Status))
            {
                Log.Warning($"心跳检测错误：{e.Status}");

                if (m_reConnectHandler == null)
                {
                    m_reConnectHandler = new SessionReconnectHandler();
                    m_reConnectHandler.BeginReconnect(m_session, m_session.KeepAliveInterval, Server_ReconnectComplete);
                }

                return;
            }

            Log.Trace($"心跳检测正常 [{session.Endpoint.EndpointUrl}]");
        }
        finally
        {
            checkLock.Release();
        }
    }



    #endregion
}

using Mapster;

using Microsoft.Extensions.DependencyInjection;

using Opc.Ua;
using Opc.Ua.Server;

using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.OPCUA;


/// <summary>
/// 数据节点
/// </summary>
public class ThingsGatewayNodeManager : CustomNodeManager2
{
    private const string ReferenceServer = "https://diego2098.gitee.io/thingsgateway/";

    private GlobalCollectDeviceData _globalCollectDeviceData;

    /// <summary>
    /// OPC和网关对应表
    /// </summary>
    private Dictionary<NodeId, OPCUATag> _idTags = new Dictionary<NodeId, OPCUATag>();
    private RpcSingletonService _rpcCore;
    private IServiceScope _serviceScope;
    private TypeAdapterConfig _config;
    /// <inheritdoc cref="ThingsGatewayNodeManager"/>
    public ThingsGatewayNodeManager(IServiceScope serviceScope, IServerInternal server, ApplicationConfiguration configuration) : base(server, configuration, ReferenceServer)
    {
        _serviceScope = serviceScope;
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _config = new TypeAdapterConfig();
        _config.ForType<ValueHis, DataValue>()
.Map(dest => dest.WrappedValue, (src) => new Variant(src.Value))
.Map(dest => dest.SourceTimestamp, (src) => DateTime.SpecifyKind(src.CollectTime, DateTimeKind.Utc))
.Map(dest => dest.StatusCode, (src) => src.Quality == 192 ? StatusCodes.Good : StatusCodes.Bad);
    }



    /// <summary>
    /// 创建服务目录结构
    /// </summary>
    /// <param name="externalReferences"></param>
    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        lock (Lock)
        {
            IList<IReference> references = null;
            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out references))
            {
                externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
            }
            //首节点
            FolderState rootFolder = CreateFolder(null, "ThingsGateway", "ThingsGateway");
            rootFolder.AddReference(ReferenceTypes.Organizes, true, ObjectIds.ObjectsFolder);
            references.Add(new NodeStateReference(ReferenceTypes.Organizes, false, rootFolder.NodeId));
            rootFolder.EventNotifier = EventNotifiers.SubscribeToEvents;
            AddRootNotifier(rootFolder);

            //创建设备树
            var _geviceGroup = global::CollectDeviceServiceHelpers.GetTree(_globalCollectDeviceData.CollectDevices.ToList().Adapt<List<CollectDevice>>());
            // 开始寻找设备信息，并计算一些节点信息
            foreach (var item in _geviceGroup)
            {
                //设备树会有两层
                FolderState fs = CreateFolder(rootFolder, item.Name, item.Name);
                fs.AddReference(ReferenceTypes.Organizes, true, ObjectIds.ObjectsFolder);
                fs.EventNotifier = EventNotifiers.SubscribeToEvents;
                if (item.Childrens?.Count > 0)
                {
                    foreach (var item2 in item.Childrens)
                    {
                        AddTagNode(fs, item2.Name);
                    }
                }
                else
                {
                    AddTagNode(fs, item.Name);
                }

            }

            AddPredefinedNode(SystemContext, rootFolder);

        }

    }


    /// <summary>
    /// 读取历史数据
    /// </summary>
    public override void HistoryRead(OperationContext context,
        HistoryReadDetails details,
        TimestampsToReturn timestampsToReturn,
        bool releaseContinuationPoints,
        IList<HistoryReadValueId> nodesToRead,
        IList<HistoryReadResult> results,
        IList<ServiceResult> errors)
    {
        base.HistoryRead(context, details, timestampsToReturn, releaseContinuationPoints, nodesToRead, results, errors);
        var readDetail = details as ReadRawModifiedDetails;
        //必须带有时间范围
        if (readDetail == null || readDetail.StartTime == DateTime.MinValue || readDetail.EndTime == DateTime.MinValue)
        {
            errors[0] = StatusCodes.BadHistoryOperationUnsupported;
            return;
        }
        var service = _serviceScope.GetBackgroundService<ValueHisWorker>();
        if (!service.StatuString.IsSuccess)
        {
            errors[0] = StatusCodes.BadHistoryOperationUnsupported;
            return;
        }

        var db = service.GetHisDbAsync().GetAwaiter().GetResult();
        if (!db.IsSuccess)
        {
            errors[0] = StatusCodes.BadHistoryOperationUnsupported;
            return;
        }
        var startTime = readDetail.StartTime.ToUniversalTime();
        var endTime = readDetail.EndTime.ToUniversalTime();

        for (int i = 0; i < nodesToRead.Count; i++)
        {
            var historyRead = nodesToRead[i];
            if (_idTags.TryGetValue(historyRead.NodeId, out OPCUATag tag))
            {
                var data = db.Content.Queryable<ValueHis>()
                    .Where(a => a.Name == tag.SymbolicName)
                    .Where(a => a.CollectTime >= startTime)
                    .Where(a => a.CollectTime <= endTime)
                    .ToList();

                if (data.Count > 0)
                {

                    var hisDataValue = data.Adapt<List<DataValue>>(_config);
                    HistoryData hisData = new HistoryData();
                    hisData.DataValues.AddRange(hisDataValue);
                    errors[i] = StatusCodes.Good;
                    //切记Processed设为true，否则客户端会报错
                    historyRead.Processed = true;
                    results[i] = new HistoryReadResult()
                    {
                        StatusCode = StatusCodes.Good,
                        HistoryData = new ExtensionObject(hisData)
                    };
                }
                else
                {
                    results[i] = new HistoryReadResult()
                    {
                        StatusCode = StatusCodes.GoodNoData
                    };
                }
            }
            else
            {
                results[i] = new HistoryReadResult()
                {
                    StatusCode = StatusCodes.BadNotFound
                };
            }
        }
    }
    /// <inheritdoc/>
    public override NodeId New(ISystemContext context, NodeState node)
    {
        BaseInstanceState instance = node as BaseInstanceState;
        if (instance != null && instance.Parent != null)
        {
            string id = instance.Parent.NodeId.Identifier?.ToString();
            if (id != null)
            {
                //用下划线分割
                return new NodeId(id + "_" + instance.SymbolicName, instance.Parent.NodeId.NamespaceIndex);
            }
        }
        return node.NodeId;
    }

    /// <summary>
    /// 更新变量
    /// </summary>
    /// <param name="variable"></param>
    public void UpVariable(VariableData variable)
    {
        var uaTag = _idTags.Values.FirstOrDefault(it => it.SymbolicName == variable.name);
        if (uaTag == null) return;
        object initialItemValue = null;
        initialItemValue = variable.value;
        if (initialItemValue != null)
        {
            var code = variable.quality == 192 ? StatusCodes.Good : StatusCodes.Bad;
            if (uaTag.Value != initialItemValue)
                ChangeNodeData(uaTag.NodeId.ToString(), initialItemValue, variable.changeTime);
            if (uaTag.StatusCode != code)
                uaTag.SetStatusCode(SystemContext, code, variable.changeTime);
        }
    }

    /// <summary>
    /// 添加变量节点
    /// </summary>
    /// <param name="fs">设备组节点</param>
    /// <param name="name">设备名称</param>
    private void AddTagNode(FolderState fs, string name)
    {
        var device = _globalCollectDeviceData.CollectDevices.Where(a => a.Name == name).FirstOrDefault();
        if (device != null)
        {
            foreach (var item in device.DeviceVariableRunTimes)
            {
                CreateVariable(fs, item);
            }
        }
    }

    /// <summary>
    /// 在服务器端直接更改对应数据节点的值，并通知客户端
    /// </summary>
    private void ChangeNodeData(string nodeId, object value, DateTime dateTime)
    {
        if (_idTags.ContainsKey(nodeId))
        {
            lock (Lock)
            {
                _idTags[nodeId].Value = value;
                _idTags[nodeId].Timestamp = dateTime;
                _idTags[nodeId].ClearChangeMasks(SystemContext, false);
            }
        }
    }

    /// <summary>
    /// 创建文件夹
    /// </summary>
    private FolderState CreateFolder(NodeState parent, string name, string description)
    {
        FolderState folder = new FolderState(parent);

        folder.SymbolicName = name;
        folder.ReferenceTypeId = ReferenceTypes.Organizes;
        folder.TypeDefinitionId = ObjectTypeIds.FolderType;
        folder.Description = description;
        folder.NodeId = new NodeId(name, NamespaceIndex);
        folder.BrowseName = new QualifiedName(name, NamespaceIndex);
        folder.DisplayName = new LocalizedText(name);
        folder.WriteMask = AttributeWriteMask.None;
        folder.UserWriteMask = AttributeWriteMask.None;
        folder.EventNotifier = EventNotifiers.None;

        if (parent != null)
        {
            parent.AddChild(folder);
        }

        return folder;
    }

    /// <summary>
    /// 创建一个值节点，类型需要在创建的时候指定
    /// </summary>
    private OPCUATag CreateVariable(NodeState parent, CollectVariableRunTime variableRunTime)
    {
        OPCUATag variable = new OPCUATag(parent);

        variable.SymbolicName = variableRunTime.Name;
        variable.ReferenceTypeId = ReferenceTypes.Organizes;
        variable.TypeDefinitionId = VariableTypeIds.BaseDataVariableType;
        variable.NodeId = new NodeId(variableRunTime.Name, NamespaceIndex);
        variable.Description = variableRunTime.Description;
        variable.BrowseName = new QualifiedName(variableRunTime.Name, NamespaceIndex);
        variable.DisplayName = new LocalizedText(variableRunTime.Name);
        variable.WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description;
        variable.UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description;
        variable.ValueRank = ValueRanks.Scalar;
        variable.Id = variableRunTime.Id;
        variable.DataType = DataNodeType(variableRunTime);
        var level = ProtectTypeTrans(variableRunTime);
        variable.AccessLevel = level;
        variable.UserAccessLevel = level;
        variable.Historizing = variableRunTime.HisEnable;
        variable.StatusCode = StatusCodes.Good;
        variable.Timestamp = DateTime.UtcNow;
        variable.Value = Opc.Ua.TypeInfo.GetDefaultValue(variable.DataType, ValueRanks.Scalar, Server.TypeTree);
        variable.OnWriteValue = OnWriteDataValue;
        if (parent != null)
        {
            parent.AddChild(variable);
        }
        _idTags.AddOrUpdate(variable.NodeId, variable);
        return variable;
    }

    /// <summary>
    /// 网关转OPC数据类型
    /// </summary>
    /// <param name="variableRunTime"></param>
    /// <returns></returns>
    private NodeId DataNodeType(CollectVariableRunTime variableRunTime)
    {
        var tp = variableRunTime.DataType;
        if (tp == typeof(bool))
            return DataTypeIds.Boolean;
        if (tp == typeof(byte))
            return DataTypeIds.Byte;
        if (tp == typeof(sbyte))
            return DataTypeIds.SByte;
        if (tp == typeof(Int16))
            return DataTypeIds.Int16;
        if (tp == typeof(UInt16))
            return DataTypeIds.UInt16;
        if (tp == typeof(Int32))
            return DataTypeIds.Int32;
        if (tp == typeof(UInt32))
            return DataTypeIds.UInt32;
        if (tp == typeof(Int64))
            return DataTypeIds.Int64;
        if (tp == typeof(UInt64))
            return DataTypeIds.UInt64;
        if (tp == typeof(float))
            return DataTypeIds.Float;
        if (tp == typeof(Double))
            return DataTypeIds.Double;
        if (tp == typeof(String))
            return DataTypeIds.String;
        if (tp == typeof(DateTime))
            return DataTypeIds.TimeString;
        return DataTypeIds.ObjectNode;
    }

    private ServiceResult OnWriteDataValue(ISystemContext context, NodeState node, NumericRange indexRange, QualifiedName dataEncoding, ref object value, ref StatusCode statusCode, ref DateTime timestamp)
    {
        try
        {
            var context1 = context as ServerSystemContext;
            if(context1.UserIdentity.TokenType==UserTokenType.Anonymous)
            {
                return StatusCodes.BadUserAccessDenied;
            }
            OPCUATag variable = node as OPCUATag;
            // 验证数据类型。
            //Opc.Ua.TypeInfo typeInfo = Opc.Ua.TypeInfo.IsInstanceOfDataType(
            //    value,
            //    variable.DataType,
            //    variable.ValueRank,
            //    context.NamespaceUris,
            //    context.TypeTable);

            //if (typeInfo == null || typeInfo == Opc.Ua.TypeInfo.Unknown)
            //{
            //    return StatusCodes.BadTypeMismatch;
            //}
            // 检查索引范围。
            if (_idTags.TryGetValue(variable.NodeId, out OPCUATag tag))
            {
                if (StatusCode.IsGood(variable.StatusCode))
                {
                    //仅当指定了值时才将值写入
                    if (variable.Value != null)
                    {
                        var nv = new NameValue() { Name = variable.SymbolicName, Value = value?.ToString() };

                        var result = _rpcCore.InvokeDeviceMethodAsync("OPCUASERVER-" + context1?.OperationContext?.Session?.Identity?.DisplayName, nv).GetAwaiter().GetResult();
                        if (result.IsSuccess)
                        {
                            return StatusCodes.Good;
                        }
                    }
                }

            }
            return StatusCodes.BadWaitingForResponse;
        }
        catch
        {
            return StatusCodes.BadTypeMismatch;
        }

    }

    private byte ProtectTypeTrans(CollectVariableRunTime variableRunTime)
    {
        byte result = 0;
        switch (variableRunTime.ProtectTypeEnum)
        {
            case ProtectTypeEnum.ReadOnly:
                result = (byte)(result | AccessLevels.CurrentRead);
                break;
            case ProtectTypeEnum.ReadWrite:
                result = (byte)(result | AccessLevels.CurrentReadOrWrite);
                break;
            default:
                result = (byte)(result | AccessLevels.CurrentRead);
                break;
        }
        if (variableRunTime.HisEnable)
        {
            result = (byte)(result | AccessLevels.HistoryRead);
        }
        return result;
    }

}

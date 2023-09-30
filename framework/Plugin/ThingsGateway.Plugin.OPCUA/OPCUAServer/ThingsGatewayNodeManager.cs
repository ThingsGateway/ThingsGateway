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

using Mapster;

using Newtonsoft.Json.Linq;

using Opc.Ua;
using Opc.Ua.Server;

using ThingsGateway.Foundation.Adapter.OPCUA;

namespace ThingsGateway.Plugin.OPCUA;


/// <summary>
/// 数据节点
/// </summary>
public class ThingsGatewayNodeManager : CustomNodeManager2
{
    private const string ReferenceServer = "https://diego2098.gitee.io/thingsgateway-docs/";
    private readonly TypeAdapterConfig _config;
    private readonly UploadDevice _device;
    private readonly GlobalDeviceData _globalDeviceData;
    private readonly RpcSingletonService _rpcCore;
    private readonly ILog LogMessage;
    /// <summary>
    /// OPC和网关对应表
    /// </summary>
    private readonly Dictionary<NodeId, OPCUATag> NodeIdTags = new();
    /// <inheritdoc cref="ThingsGatewayNodeManager"/>
    public ThingsGatewayNodeManager(UploadDevice device, ILog log, IServerInternal server, ApplicationConfiguration configuration) : base(server, configuration, ReferenceServer)
    {
        _device = device;
        LogMessage = log;
        _rpcCore = App.GetService<RpcSingletonService>();
        _globalDeviceData = App.GetService<GlobalDeviceData>();
        _config = new TypeAdapterConfig();
        _config.ForType<HistoryValue, DataValue>()
.Map(dest => dest.WrappedValue, (src) => new Variant(src.Value))
.Map(dest => dest.SourceTimestamp, src => DateTime.SpecifyKind(src.CollectTime, DateTimeKind.Utc))
.Map(dest => dest.StatusCode, (src) =>
src.IsOnline ? StatusCodes.Good : StatusCodes.Bad);
    }



    /// <summary>
    /// 创建服务目录结构
    /// </summary>
    /// <param name="externalReferences"></param>
    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        lock (Lock)
        {
            if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out IList<IReference> references))
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
            var _geviceGroup = _globalDeviceData.CollectDevices.ToList().Adapt<List<CollectDevice>>().GetTree();
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
            FolderState memoryfs = CreateFolder(null, "ThingsGateway中间变量", "ThingsGateway中间变量");
            var variableRunTimes = _globalDeviceData.MemoryVariables;
            foreach (var item in variableRunTimes)
            {
                CreateVariable(memoryfs, item);
            }
            AddPredefinedNode(SystemContext, rootFolder);

        }

    }
    /// <summary>
    /// 获取变量的属性值
    /// </summary>
    public string GetPropertyValue(DeviceVariableRunTime variableRunTime, string propertyName)
    {
        if (variableRunTime.VariablePropertys.ContainsKey(_device.Id))
        {
            var data = variableRunTime.VariablePropertys[_device.Id].FirstOrDefault(a =>
                  a.PropertyName == propertyName);
            if (data != null)
            {
                return data.Value;
            }
        }
        return null;
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
        //必须带有时间范围
        if (details is not ReadRawModifiedDetails readDetail || readDetail.StartTime == DateTime.MinValue || readDetail.EndTime == DateTime.MinValue)
        {
            errors[0] = StatusCodes.BadHistoryOperationUnsupported;
            return;
        }
        var service = BackgroundServiceUtil.GetBackgroundService<HistoryValueWorker>();
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
        var startTime = readDetail.StartTime;
        var endTime = readDetail.EndTime;

        for (int i = 0; i < nodesToRead.Count; i++)
        {
            var historyRead = nodesToRead[i];
            if (NodeIdTags.TryGetValue(historyRead.NodeId, out OPCUATag tag))
            {
                var data = db.Content.Queryable<HistoryValue>()
                    .Where(a => a.Name == tag.SymbolicName)
                    .Where(a => a.CollectTime >= startTime)
                    .Where(a => a.CollectTime <= endTime)
                    .ToList();

                if (data.Count > 0)
                {

                    var hisDataValue = data.Adapt<List<DataValue>>(_config);
                    HistoryData hisData = new();
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
        if (node is BaseInstanceState instance && instance.Parent != null)
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
        var uaTag = NodeIdTags.Values.FirstOrDefault(it => it.SymbolicName == variable.Name);
        if (uaTag == null) return;
        object initialItemValue = null;
        initialItemValue = variable.Value;
        if (initialItemValue != null)
        {
            var code = variable.IsOnline ? StatusCodes.Good : StatusCodes.Bad;
            if (code == StatusCodes.Good)
            {
                ChangeNodeData(uaTag, initialItemValue, variable.ChangeTime);
            }

            if (uaTag.StatusCode != code)
            {
                uaTag.StatusCode = code;
            }
            uaTag.ClearChangeMasks(SystemContext, false);

        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        NodeIdTags.Clear();
        base.Dispose(disposing);
    }
    private static NodeId DataNodeType(Type tp)
    {
        if (tp == typeof(bool))
            return DataTypeIds.Boolean;
        if (tp == typeof(byte))
            return DataTypeIds.Byte;
        if (tp == typeof(sbyte))
            return DataTypeIds.SByte;
        if (tp == typeof(short))
            return DataTypeIds.Int16;
        if (tp == typeof(ushort))
            return DataTypeIds.UInt16;
        if (tp == typeof(int))
            return DataTypeIds.Int32;
        if (tp == typeof(uint))
            return DataTypeIds.UInt32;
        if (tp == typeof(long))
            return DataTypeIds.Int64;
        if (tp == typeof(ulong))
            return DataTypeIds.UInt64;
        if (tp == typeof(float))
            return DataTypeIds.Float;
        if (tp == typeof(double))
            return DataTypeIds.Double;
        if (tp == typeof(string))
            return DataTypeIds.String;
        if (tp == typeof(DateTime))
            return DataTypeIds.DateTime;
        return DataTypeIds.String;
    }

    /// <summary>
    /// 添加变量节点
    /// </summary>
    /// <param name="fs">设备组节点</param>
    /// <param name="name">设备名称</param>
    private void AddTagNode(FolderState fs, string name)
    {
        var device = _globalDeviceData.CollectDevices.Where(a => a.Name == name).FirstOrDefault();
        if (device != null)
        {
            foreach (var item in device.DeviceVariableRunTimes)
            {
                CreateVariable(fs, item);
            }
        }
    }

    /// <summary>
    /// 在服务器端直接更改对应数据节点的值
    /// </summary>
    private void ChangeNodeData(OPCUATag tag, object value, DateTime dateTime)
    {
        object newValue;
        try
        {
            if (!tag.IsDataTypeInit)
            {
                if (tag.DataType == DataTypeIds.String)
                {
                    if (value != null)
                    {
                        SetDataType(tag, value);
                    }
                }
                else
                {
                    SetRank(tag, value);
                }
            }
            var jToken = JToken.FromObject((tag.DataType == DataTypeIds.String ? value?.ToString() : value));
            var dataValue = JsonUtils.DecoderObject(
               this.Server.MessageContext,
           tag.DataType,
                TypeInfo.GetBuiltInType(tag.DataType, this.SystemContext.TypeTable),
                jToken.CalculateActualValueRank(),
                jToken
                );
            newValue = dataValue;
        }
        catch (Exception ex)
        {
            LogMessage.LogWarning(ex, "转化值错误");
            newValue = value;
        }
        tag.Value = newValue;
        tag.Timestamp = dateTime;


        void SetDataType(OPCUATag tag, object value)
        {
            tag.IsDataTypeInit = true;
            var tp = value.GetType();
            if (tp == typeof(JArray))
            {
                try
                {
                    tp = ((JValue)((JArray)value).FirstOrDefault()).Value.GetType();
                    tag.ValueRank = ValueRanks.OneOrMoreDimensions;
                }
                catch
                {
                }
            }
            if (tp == typeof(JValue))
            {
                tp = ((JValue)value).Value.GetType();
                tag.ValueRank = ValueRanks.Scalar;
            }
            tag.DataType = DataNodeType(tp);

            tag.ClearChangeMasks(SystemContext, false);
        }

        void SetRank(OPCUATag tag, object value)
        {
            tag.IsDataTypeInit = true;
            var tp = value.GetType();
            if (tp == typeof(JArray))
            {
                try
                {
                    tp = ((JValue)((JArray)value).FirstOrDefault()).Value.GetType();
                    tag.ValueRank = ValueRanks.OneOrMoreDimensions;
                }
                catch
                {
                }
            }
            tag.ClearChangeMasks(SystemContext, false);
        }
    }

    /// <summary>
    /// 创建文件夹
    /// </summary>
    private FolderState CreateFolder(NodeState parent, string name, string description)
    {
        FolderState folder = new(parent)
        {
            SymbolicName = name,
            ReferenceTypeId = ReferenceTypes.Organizes,
            TypeDefinitionId = ObjectTypeIds.FolderType,
            Description = description,
            NodeId = new NodeId(name, NamespaceIndex),
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText(name),
            WriteMask = AttributeWriteMask.None,
            UserWriteMask = AttributeWriteMask.None,
            EventNotifier = EventNotifiers.None
        };

        parent?.AddChild(folder);

        return folder;
    }

    /// <summary>
    /// 创建一个值节点，类型需要在创建的时候指定
    /// </summary>
    private OPCUATag CreateVariable(NodeState parent, DeviceVariableRunTime variableRunTime)
    {
        OPCUATag variable = new(parent)
        {
            SymbolicName = variableRunTime.Name,
            ReferenceTypeId = ReferenceTypes.Organizes,
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            NodeId = new NodeId(variableRunTime.Name, NamespaceIndex),
            Description = variableRunTime.Description,
            BrowseName = new QualifiedName(variableRunTime.Name, NamespaceIndex),
            DisplayName = new LocalizedText(variableRunTime.Name),
            WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
            UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description,
            ValueRank = ValueRanks.Scalar,
            Id = variableRunTime.Id,
            DataType = DataNodeType(variableRunTime)
        };
        var level = ProtectTypeTrans(variableRunTime);
        variable.AccessLevel = level;
        variable.UserAccessLevel = level;
        variable.Historizing = variableRunTime.HisEnable;
        variable.Value = Opc.Ua.TypeInfo.GetDefaultValue(variable.DataType, ValueRanks.Any, Server.TypeTree);
        var code = variableRunTime.IsOnline ? StatusCodes.Good : StatusCodes.Bad;
        variable.StatusCode = code;
        variable.Timestamp = variableRunTime.CollectTime;
        variable.OnWriteValue = OnWriteDataValue;
        parent?.AddChild(variable);
        NodeIdTags.AddOrUpdate(variable.NodeId, variable);
        return variable;
    }
    /// <summary>
    /// 网关转OPC数据类型
    /// </summary>
    /// <param name="variableRunTime"></param>
    /// <returns></returns>
    private NodeId DataNodeType(DeviceVariableRunTime variableRunTime)
    {
        var str = GetPropertyValue(variableRunTime, nameof(OPCUAServerVariableProperty.DataTypeEnum));
        Type tp;
        if (Enum.TryParse<DataTypeEnum>(str, out DataTypeEnum result))
        {
            tp = result.GetSystemType();
        }
        else
        {
            tp = variableRunTime.DataTypeEnum.GetSystemType(); ;
        }

        return DataNodeType(tp);
    }
    private ServiceResult OnWriteDataValue(ISystemContext context, NodeState node, NumericRange indexRange, QualifiedName dataEncoding, ref object value, ref StatusCode statusCode, ref DateTime timestamp)
    {
        try
        {
            var context1 = context as ServerSystemContext;
            if (context1.UserIdentity.TokenType == UserTokenType.Anonymous)
            {
                return StatusCodes.BadUserAccessDenied;
            }
            OPCUATag variable = node as OPCUATag;
            if (NodeIdTags.TryGetValue(variable.NodeId, out OPCUATag tag))
            {
                if (StatusCode.IsGood(variable.StatusCode))
                {
                    //仅当指定了值时才将值写入
                    if (variable.Value != null)
                    {

                        var result = _rpcCore.InvokeDeviceMethodAsync("OPCUASERVER-" + context1?.OperationContext?.Session?.Identity?.DisplayName,
                            new()
                            {
                                { variable.SymbolicName, value?.ToString() }
                            }


                            ).ConfigureAwait(true).GetAwaiter().GetResult();
                        if (result.Values.FirstOrDefault().IsSuccess)
                        {
                            return StatusCodes.Good;
                        }
                        else
                        {
                            return new(StatusCodes.BadWaitingForResponse, result.Values.FirstOrDefault().Message);
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

    private byte ProtectTypeTrans(DeviceVariableRunTime variableRunTime)
    {
        byte result = 0;
        result = variableRunTime.ProtectTypeEnum switch
        {
            ProtectTypeEnum.ReadOnly => (byte)(result | AccessLevels.CurrentRead),
            ProtectTypeEnum.ReadWrite => (byte)(result | AccessLevels.CurrentReadOrWrite),
            _ => (byte)(result | AccessLevels.CurrentRead),
        };
        if (variableRunTime.HisEnable)
        {
            result = (byte)(result | AccessLevels.HistoryRead);
        }
        return result;
    }

}

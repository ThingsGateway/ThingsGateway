#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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
    private const string ReferenceServer = "https://diego2098.gitee.io/thingsgateway-docs/";

    private TypeAdapterConfig _config;
    private UploadDevice _device;
    private GlobalDeviceData _globalDeviceData;

    /// <summary>
    /// OPC和网关对应表
    /// </summary>
    private Dictionary<NodeId, OPCUATag> _idTags = new Dictionary<NodeId, OPCUATag>();
    private RpcSingletonService _rpcCore;
    private IServiceScope _serviceScope;
    /// <inheritdoc cref="ThingsGatewayNodeManager"/>
    public ThingsGatewayNodeManager(IServiceScope serviceScope, UploadDevice device, IServerInternal server, ApplicationConfiguration configuration) : base(server, configuration, ReferenceServer)
    {
        _serviceScope = serviceScope; _device = device;
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        _globalDeviceData = serviceScope.ServiceProvider.GetService<GlobalDeviceData>();
        _config = new TypeAdapterConfig();
        _config.ForType<HistoryValue, DataValue>()
.Map(dest => dest.WrappedValue, (src) => new Variant(src.Value))
.Map(dest => dest.SourceTimestamp, (src) => DateTime.SpecifyKind(src.CollectTime, DateTimeKind.Utc))
.Map(dest => dest.StatusCode, (src) => src.IsOnline ? StatusCodes.Good : StatusCodes.Bad);
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

            AddPredefinedNode(SystemContext, rootFolder);

        }

    }

    protected override void Dispose(bool disposing)
    {
        _idTags.Clear();
        _idTags = null;
        base.Dispose(disposing);
    }
    /// <summary>
    /// 获取变量的默认值
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : "";
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
        var readDetail = details as ReadRawModifiedDetails;
        //必须带有时间范围
        if (readDetail == null || readDetail.StartTime == DateTime.MinValue || readDetail.EndTime == DateTime.MinValue)
        {
            errors[0] = StatusCodes.BadHistoryOperationUnsupported;
            return;
        }
        var service = _serviceScope.GetBackgroundService<HistoryValueWorker>();
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
                var data = db.Content.Queryable<HistoryValue>()
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
        var uaTag = _idTags.Values.FirstOrDefault(it => it.SymbolicName == variable.Name);
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
            newValue = Convert.ChangeType(value, tag.NETDataType);
        }
        catch
        {
            newValue = value;
        }
        tag.Value = newValue;
        tag.Timestamp = dateTime;
        //_idTags[nodeId].ClearChangeMasks(SystemContext, false);
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
    private OPCUATag CreateVariable(NodeState parent, DeviceVariableRunTime variableRunTime)
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
        variable.NETDataType = NETDataNodeType(variableRunTime);
        var level = ProtectTypeTrans(variableRunTime);
        variable.AccessLevel = level;
        variable.UserAccessLevel = level;
        variable.Historizing = variableRunTime.HisEnable;
        variable.Value = Opc.Ua.TypeInfo.GetDefaultValue(variable.DataType, ValueRanks.Scalar, Server.TypeTree);
        var code = variableRunTime.IsOnline ? StatusCodes.Good : StatusCodes.Bad;
        variable.StatusCode = code;
        variable.Timestamp = variableRunTime.CollectTime;
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
    private NodeId DataNodeType(DeviceVariableRunTime variableRunTime)
    {
        Type tp;
        var str = GetPropertyValue(variableRunTime, nameof(OPCUAServerVariableProperty.DataTypeEnum));
        if (Enum.TryParse<DataTypeEnum>(str, out DataTypeEnum result))
        {
            tp = result.GetSystemType();
        }
        else
        {

            if (variableRunTime.Value != null)
            {
                tp = variableRunTime.Value.GetType();
            }
            else
            {
                if (variableRunTime.ReadExpressions.IsNullOrEmpty())
                {
                    tp = variableRunTime.DataTypeEnum.GetSystemType();
                }
                else
                {
                    var tp1 = variableRunTime.DataTypeEnum.GetSystemType();
                    var data = variableRunTime.ReadExpressions.GetExpressionsResult(GetDefaultValue(tp1));
                    tp = data.GetType();
                }
            }
        }

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
    /// <summary>
    /// 网关转OPC数据类型
    /// </summary>
    /// <param name="variableRunTime"></param>
    /// <returns></returns>
    private Type NETDataNodeType(DeviceVariableRunTime variableRunTime)
    {
        Type tp;
        var str = GetPropertyValue(variableRunTime, nameof(OPCUAServerVariableProperty.DataTypeEnum));
        if (Enum.TryParse<DataTypeEnum>(str, out DataTypeEnum result))
        {
            tp = result.GetSystemType();
        }
        else
        {

            if (variableRunTime.Value != null)
            {
                tp = variableRunTime.Value.GetType();
            }
            else
            {
                if (variableRunTime.ReadExpressions.IsNullOrEmpty())
                {
                    tp = variableRunTime.DataTypeEnum.GetSystemType();
                }
                else
                {
                    var tp1 = variableRunTime.DataTypeEnum.GetSystemType();
                    var data = variableRunTime.ReadExpressions.GetExpressionsResult(GetDefaultValue(tp1));
                    tp = data.GetType();
                }
            }
        }

        return tp;
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
                        var nv = new KeyValuePair<string, string>(variable.SymbolicName, value?.ToString());

                        var result = _rpcCore.InvokeDeviceMethodAsync("OPCUASERVER-" + context1?.OperationContext?.Session?.Identity?.DisplayName, nv, CancellationToken.None).GetAwaiter().GetResult();
                        if (result.IsSuccess)
                        {
                            return StatusCodes.Good;
                        }
                        else
                        {
                            return new(StatusCodes.BadWaitingForResponse, result.Message);
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

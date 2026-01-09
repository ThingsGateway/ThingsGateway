//////------------------------------------------------------------------------------
//////此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
////// 此代码版权（除特别声明外的代码）归作者本人Diego所有
////// 源代码使用协议遵循本仓库的开源协议及附加协议
////// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
////// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
////// 使用文档：https://thingsgateway.cn/
////// QQ群：605534569
////// ------------------------------------------------------------------------------

//using ThingsGateway.Foundation;
//using ThingsGateway.Gateway.Application;
//using ThingsGateway.Foundation.Common.Extension;
//public class TestKafkaDynamicModel : DynamicModelBase
//{
//    private Dictionary<string, VariableRuntime> variableRuntimes = new();

//    private long id = 0;

//    public TestKafkaDynamicModel()
//    {
//        var name = "kafka1";
//        if (GlobalData.TryGetDeviceRuntime(name, out var kafka1))
//        {
//            id = kafka1.Id;

//            foreach (var item in kafka1.Driver?.IdVariableRuntimes)
//            {
//                //变量备注1作为Key(AE报警SourceId)
//                var data1 = item.Value.GetPropertyValue(id, nameof(BusinessVariableProperty.Data1));
//                if (!data1.IsNullOrEmpty())
//                {
//                    variableRuntimes.Add(data1, item.Value);
//                }
//            }

//        }
//        else
//        {
//            throw new Exception($"找不到设备 {name}");
//        }

//    }
//    public override IEnumerable<dynamic> GetList(IEnumerable<object> datas)
//    {
//        if (datas == null) return null;
//        var pluginEventDatas = datas.Cast<PluginEventData>();
//        var opcDatas = pluginEventDatas.Select(
//            a =>
//            {
//                if (a.ObjectValue == null)
//                {
//                    a.ObjectValue = a.Value.ToObject(Type.GetType(a.ValueType));
//                }
//                return a.ObjectValue is ThingsGateway.Plugin.OpcAe.OpcAeEventData opcData ? opcData : null;
//            }
//            ).Where(a => a != null).ToList();

//        List<KafkaAlarmEntity> alarmEntities = new List<KafkaAlarmEntity>();
//        if (opcDatas.Count == 0)
//        {
//            Logger?.LogInformation("没有OPCAE数据");
//            return alarmEntities;
//        }


//        foreach (var opcAeEventData in opcDatas)
//        {
//            //一般只需要条件报警
//            //if (opcAeEventData.EventType != Opc.Ae.EventType.Condition)
//            //    continue;
//            //重连时触发的事件，可以跳过不处理
//            //if(opcAeEventData.Refresh)
//            //    continue;
//            var sourceName = opcAeEventData.SourceID;
//            if (variableRuntimes.TryGetValue(sourceName, out var variableRuntime))
//            {
//                var ack = opcAeEventData.EventType != Opc.Ae.EventType.Condition ? false : ((Opc.Ae.ConditionState)opcAeEventData.NewState).HasFlag(Opc.Ae.ConditionState.Acknowledged);

//                bool isRecover = opcAeEventData.EventType != Opc.Ae.EventType.Condition ? false : !((Opc.Ae.ConditionState)opcAeEventData.NewState).HasFlag(Opc.Ae.ConditionState.Active);

//                //构建告警实体
//                KafkaAlarmEntity alarmEntity = new KafkaAlarmEntity
//                {
//                    AlarmCode = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data2)), //唯一编码
//                    ResourceCode = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data3)), //资源编码
//                    ResourceName = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data4)), //资源名称  
//                    MetricCode = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data5)), //指标编码
//                    MetricName = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data6)), //指标名称
//                    Content = $"{variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data4))}，{opcAeEventData.Message}", //告警内容，设备名称+告警内容（包含阈值信息），可能opcae里没有带阈值信息，那么就需要录入固定值，可选Data10
//                    AlarmType = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data7)), // opcAeEventData.Severity 告警类型，子系统产生告警的类型，可能需要固定备注值

//                    ConfirmedTime = ack ? opcAeEventData.Time.DateTimeToUnixTimestamp() : null, //告警确认时间
//                    FixTime = isRecover ? opcAeEventData.Time.DateTimeToUnixTimestamp() : null, //解除告警时间
//                    LastTime = opcAeEventData.AlarmTime.DateTimeToUnixTimestamp(), //产生告警时间
//                    Status = isRecover ? "FIXED" : "UNFIXED", //告警状态
//                    AlarmLevel = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data8)), //opcAeEventData.Severity.ToString(), //告警等级,可能需要固定备注值
//                    SubSystemCode = variableRuntime.GetPropertyValue(id, nameof(BusinessVariableProperty.Data9)), //子系统编码
//                    Type = "SUB_SYSTEM_ALARM", //默认填写字段
//                    ConfirmAccount = opcAeEventData.ActorID, //告警确认人
//                    ClearAccount = opcAeEventData.ActorID, //告警清除人
//                    ProcessInstruction = null //告警处理说明，OPCAE不带有
//                };
//                alarmEntities.Add(alarmEntity);
//            }
//            else
//            {
//                Logger?.LogInformation($"找不到相关变量{sourceName}");
//            }
//        }

//        return alarmEntities;
//    }
//}


///// <summary>
///// 告警实体
///// </summary>
//public class KafkaAlarmEntity
//{
//    /// <summary>
//    /// 告警编码唯一 (非空)
//    /// 示例："8e8a118ac452fd04da8c26fa588a7cab"
//    /// </summary>
//    public string AlarmCode { get; set; }

//    /// <summary>
//    /// 资源编码，唯一编码，需要按照映射表上传 (非空)
//    /// 示例："RS_A6K9MUSG19V"
//    /// </summary>
//    public string ResourceCode { get; set; }

//    /// <summary>
//    /// 资源名称，需要按照映射表上传 (非空)
//    /// 示例："MB-A7"
//    /// </summary>
//    public string ResourceName { get; set; }

//    /// <summary>
//    /// 指标编码唯一，需要按照映射表上传 (非空)
//    /// 示例："ActivePowerPa"
//    /// </summary>
//    public string MetricCode { get; set; }

//    /// <summary>
//    /// 指标名称，需要按照映射表上传 (非空)
//    /// 示例："有功功率Pa"
//    /// </summary>
//    public string MetricName { get; set; }

//    /// <summary>
//    /// 告警内容：设备名称+告警内容（包含阈值信息） (非空)
//    /// 示例："MB-A7，有功功率Pa > 30"
//    /// </summary>
//    public string Content { get; set; }

//    /// <summary>
//    /// 告警类型，子系统产生告警的类型 (非空)
//    /// 示例："0101" 表示高限报警
//    /// </summary>
//    public string AlarmType { get; set; }

//    /// <summary>
//    /// 告警确认时间 (可空，时间戳)
//    /// 示例：1586152800000
//    /// </summary>
//    public long? ConfirmedTime { get; set; }

//    /// <summary>
//    /// 解除告警时间 (可空，时间戳)
//    /// 示例：1586152800000
//    /// </summary>
//    public long? FixTime { get; set; }

//    /// <summary>
//    /// 产生告警时间 (非空，时间戳)
//    /// 示例：1586152800000
//    /// </summary>
//    public long LastTime { get; set; }

//    /// <summary>
//    /// 告警状态 (非空)
//    /// 可选值：UNFIXED（新增告警）、FIXED（解除告警）
//    /// </summary>
//    public string Status { get; set; }

//    /// <summary>
//    /// 告警等级，需要按照映射表上传 (非空)
//    /// 示例："1"
//    /// </summary>
//    public string AlarmLevel { get; set; }

//    /// <summary>
//    /// 子系统编码 (非空)
//    /// 示例："MS_NEW_PD_DCIM_001"
//    /// </summary>
//    public string SubSystemCode { get; set; }

//    /// <summary>
//    /// 默认填写字段 (非空)
//    /// 固定值："SUB_SYSTEM_ALARM"
//    /// </summary>
//    public string Type { get; set; }

//    /// <summary>
//    /// 告警确认人 (可空)
//    /// 示例："admin3"
//    /// </summary>
//    public string ConfirmAccount { get; set; }

//    /// <summary>
//    /// 告警清除人 (可空)
//    /// 示例："admin"
//    /// </summary>
//    public string ClearAccount { get; set; }

//    /// <summary>
//    /// 告警处理说明 (可空)
//    /// 示例："admin"
//    /// </summary>
//    public string ProcessInstruction { get; set; }
//}

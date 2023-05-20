
using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;
/// <summary>
/// 上传DTO
/// </summary>
public class VariableData
{
    /// <inheritdoc cref="PrimaryIdEntity.Id"/>
    public long id { get; set; }
    /// <inheritdoc cref="MemoryVariable.Name"/>
    public string name { get; set; }
    /// <inheritdoc cref="MemoryVariable.Description"/>
    public object description { get; set; }
    /// <inheritdoc cref="CollectVariableRunTime.DeviceName"/>
    public string deviceName { get; set; }
    /// <inheritdoc cref="CollectVariableRunTime.RawValue"/>
    public string rawValue { get; set; }
    /// <inheritdoc cref="CollectVariableRunTime.Value"/>
    public object value { get; set; }
    /// <inheritdoc cref="CollectVariableRunTime.ChangeTime"/>
    public DateTime changeTime { get; set; }
    /// <inheritdoc cref="CollectVariableRunTime.CollectTime"/>
    public DateTime collectTime { get; set; }
    /// <inheritdoc cref="CollectVariableRunTime.Quality"/>
    public int quality { get; set; }
    /// <inheritdoc cref="CollectDeviceVariable.ReadExpressions"/>
    public string readExpressions { get; set; }
    /// <inheritdoc cref="CollectDeviceVariable.WriteExpressions"/>
    public string writeExpressions { get; set; }
    /// <inheritdoc cref="CollectDeviceVariable.IntervalTime"/>
    public int intervalTime { get; set; }
    /// <inheritdoc cref="CollectDeviceVariable.OtherMethod"/>
    public object otherMethod { get; set; }
    /// <inheritdoc cref="CollectDeviceVariable.VariableAddress"/>
    public string variableAddress { get; set; }

    /// <inheritdoc cref="MemoryVariable.ProtectTypeEnum"/>
    public int protectTypeEnum { get; set; }
    /// <inheritdoc cref="MemoryVariable.DataTypeEnum"/>
    public int dataTypeEnum { get; set; }
}


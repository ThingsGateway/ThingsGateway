#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion


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


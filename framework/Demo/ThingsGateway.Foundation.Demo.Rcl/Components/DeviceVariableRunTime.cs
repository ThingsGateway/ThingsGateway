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

using System.ComponentModel;

namespace ThingsGateway.Foundation.Demo;

/// <inheritdoc/>
public class DeviceVariableRunTime : IDeviceVariableRunTime
{
    /// <inheritdoc/>
    [Description("读取间隔")]
    public int IntervalTime { get; set; }
    /// <inheritdoc/>
    [Description("变量地址")]
    public string VariableAddress { get; set; }
    /// <inheritdoc/>
    public int Index { get; set; }
    /// <inheritdoc/>
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }
    /// <inheritdoc/>
    [Description("数据类型")]
    public DataTypeEnum DataTypeEnum { get; set; }
    /// <inheritdoc/>
    [Description("实时值")]
    public object Value { get; set; }
    /// <inheritdoc/>
    public OperResult SetValue(object value, DateTime dateTime = default, bool isOnline = true)
    {
        Value = value;
        return OperResult.CreateSuccessResult();
    }
}

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Core.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessBase : DriverBase
{
    /// <summary>
    /// 插件配置项 ，继承实现<see cref="VariablePropertyBase"/>后，返回继承类，如果不存在，返回null
    /// </summary>
    public abstract VariablePropertyBase VariablePropertys { get; }

    public override CollectPropertyBase DriverPropertys => _businessPropertyBase;

    protected abstract BusinessPropertyBase _businessPropertyBase { get; }

    /// <summary>
    /// 当前关联的采集设备
    /// </summary>
    public List<CollectDeviceRunTime> CollectDevices { get; protected set; }

    internal override void Init(DeviceRunTime device)
    {
        base.Init(device);
        var variables = GlobalData.AllVariables.Where(a =>
        a.VariablePropertys.ContainsKey(device.Id)).ToList();
        device.VariableRunTimes = variables;
        CollectDevices = GlobalData.CollectDevices.Where(a => device.VariableRunTimes.Select(b => b.DeviceId).Contains(a.Id)).ToList();
        lock (GlobalData.BusinessDevices)
        {
            GlobalData.BusinessDevices.RemoveWhere(it => it.Id == device.Id);
            GlobalData.BusinessDevices.Add(CurrentDevice);
        }
        if (device.IntervalTime <= 50) device.IntervalTime = 50;
    }

    public override async Task AfterStopAsync()
    {
        await base.AfterStopAsync();
    }
}
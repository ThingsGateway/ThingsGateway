//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Localization;

using ThingsGateway.Core.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessBase : DriverBase
{
    /// <summary>
    /// 当前关联的采集设备
    /// </summary>
    public IReadOnlyDictionary<string, CollectDeviceRunTime> CollectDevices { get; protected set; }

    public override DriverPropertyBase DriverProperties => _businessPropertyBase;

    public List<IEditorItem> PluginVariablePropertyEditorItems
    {
        get
        {
            if (CurrentDevice?.PluginName?.IsNullOrWhiteSpace() == true)
            {
                var result = PluginService.GetVariablePropertyTypes(CurrentDevice.PluginName, this);
                return result.EditorItems.ToList();
            }
            else
            {
                var editorItems = PluginServiceUtil.GetEditorItems(VariablePropertys?.GetType());
                return editorItems.ToList();
            }
        }
    }

    /// <summary>
    /// 插件配置项 ，继承实现<see cref="VariablePropertyBase"/>后，返回继承类，如果不存在，返回null
    /// </summary>
    public abstract VariablePropertyBase VariablePropertys { get; }

    protected abstract BusinessPropertyBase _businessPropertyBase { get; }
    protected IStringLocalizer BusinessBaseLocalizer { get; private set; }

    /// <summary>
    /// 初始化方法，用于初始化设备运行时。
    /// </summary>
    /// <param name="device">设备运行时实例。</param>
    internal override void Init(DeviceRunTime device)
    {
        BusinessBaseLocalizer = App.CreateLocalizerByType(typeof(BusinessBase))!;
        base.Init(device); // 调用基类的初始化方法

        // 获取与当前设备相关的变量
        var variables = GlobalData.Variables.Where(a => a.Value.VariablePropertys?.ContainsKey(device.Id) == true);

        // 将变量与设备关联，并保存到设备运行时的变量字典中
        device.VariableRunTimes = variables.ToDictionary(a => a.Key, a => a.Value);

        // 获取当前设备需要采集的设备
        CollectDevices = GlobalData.CollectDevices
                                .Where(a => device.VariableRunTimes.Select(b => b.Value.DeviceId).Contains(a.Value.Id))
                                .ToDictionary(a => a.Key, a => a.Value);

        // 使用线程安全的方式更新全局业务设备字典
        //lock (GlobalData.BusinessDevices)
        {
            // 移除全局业务设备中与当前设备相同Id的项
            GlobalData.BusinessDevices.RemoveWhere(it => it.Value.Id == device.Id);

            // 添加当前设备到全局业务设备字典中
            GlobalData.BusinessDevices.TryAdd(CurrentDevice.Name, CurrentDevice);
        }

        // 如果设备的采集间隔小于等于50毫秒，则将其设置为50毫秒
        if (device.IntervalTime <= 50)
            device.IntervalTime = 50;
    }

    /// <summary>
    /// 默认延时
    /// </summary>
    protected async Task Delay(CancellationToken cancellationToken)
    {
        await Task.Delay(Math.Max(CurrentDevice.IntervalTime - ChannelThread.CycleInterval, ChannelThread.CycleInterval), cancellationToken).ConfigureAwait(false);
    }
}

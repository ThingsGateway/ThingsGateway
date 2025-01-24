//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Localization;

using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessBase : DriverBase
{
    /// <summary>
    /// 当前关联的采集设备
    /// </summary>
    public IReadOnlyDictionary<long, DeviceRuntime> CollectDevices { get; protected set; }

    /// <summary>
    /// 变量属性UI Type，如果不存在，返回null
    /// </summary>
    public virtual Type DriverVariablePropertyUIType { get; }

    public sealed override DriverPropertyBase DriverProperties => _businessPropertyBase;

    private List<IEditorItem> pluginVariablePropertyEditorItems;
    public List<IEditorItem> PluginVariablePropertyEditorItems
    {
        get
        {
            if (pluginVariablePropertyEditorItems == null)
            {
                pluginVariablePropertyEditorItems = PluginServiceUtil.GetEditorItems(VariablePropertys?.GetType()).ToList();
            }
            return pluginVariablePropertyEditorItems;
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
    protected override void ProtectedInitDevice(DeviceRuntime device)
    {
        BusinessBaseLocalizer = App.CreateLocalizerByType(typeof(BusinessBase))!;
        base.ProtectedInitDevice(device); // 调用基类的初始化方法
    }

    public override void AfterVariablesChanged()
    {
        // 获取与当前设备相关的变量,CurrentDevice.VariableRuntimes并不适用于业务插件
        var variableRuntimes = GlobalData.GetEnableVariables().Where(a =>
        {
            if (!a.Value.Enable) return false;
            if (a.Value.VariablePropertys?.TryGetValue(DeviceId, out var values) == true)
            {
                if (values.TryGetValue("Enable", out var Enable))
                {
                    return Enable.ToBoolean(true);
                }
                else if (values.TryGetValue("enable", out var enable))
                {
                    return enable.ToBoolean(true);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        );


        // 获取当前设备需要采集的设备
        CollectDevices = GlobalData.GetEnableDevices().Where(a => CurrentDevice.VariableRuntimes.Select(b => b.Value.DeviceId).ToHashSet().Contains(a.Key)).ToDictionary(a => a.Key, a => a.Value);

        VariableRuntimes = variableRuntimes.ToDictionary();
    }
}

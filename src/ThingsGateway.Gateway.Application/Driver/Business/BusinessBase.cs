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
using System.Collections.Frozen;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessBase : DriverBase
{
    public virtual bool RefreshRuntimeAlways { get; set; } = false;
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
            return pluginVariablePropertyEditorItems ??= PluginServiceUtil.GetEditorItems(VariablePropertys?.GetType()).ToList();
        }
    }

    /// <summary>
    /// 插件配置项 ，继承实现<see cref="VariablePropertyBase"/>后，返回继承类，如果不存在，返回null
    /// </summary>
    public abstract VariablePropertyBase VariablePropertys { get; }

    protected abstract BusinessPropertyBase _businessPropertyBase { get; }

    /// <summary>
    /// 当前关联的变量
    /// </summary>
    protected IReadOnlyDictionary<string, List<VariableRuntime>> VariableRuntimeGroups { get; set; } = FrozenDictionary<string, List<VariableRuntime>>.Empty;



    public override Task AfterVariablesChangedAsync(CancellationToken cancellationToken)
    {
        LogMessage?.LogInformation("Refresh variable");
        // 获取与当前设备相关的变量,CurrentDevice.VariableRuntimes并不适用于业务插件

        var variableRuntimes = GlobalData.IdVariables.Where(a =>
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
        IdVariableRuntimes = variableRuntimes.ToFrozenDictionary();
        CollectDevices = IdVariableRuntimes.Select(a => a.Value.DeviceRuntime).Where(a => !a.IsMemory && a.IsCollect == true).DistinctBy(a => a.Id).ToFrozenDictionary(a => a.Id, a => a);

        VariableRuntimeGroups = IdVariableRuntimes.Where(a => !a.Value.BusinessGroup.IsNullOrEmpty()).GroupBy(a => a.Value.BusinessGroup ?? string.Empty).ToFrozenDictionary(a => a.Key, a => a.Select(a => a.Value).ToList());

        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取任务
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>表示异步操作结果的枚举。</returns>
    protected override List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LogMessage?.LogInformation("Get business tasks");

        var setDeviceStatusTask = new ScheduledSyncTask(3000, SetDeviceStatus, null, LogMessage, cancellationToken);

        var executeTask = ScheduledTaskHelper.GetTask(CurrentDevice.IntervalTime, ProtectedExecuteAsync, null, LogMessage, cancellationToken);

        return new List<IScheduledTask>()
            {
                setDeviceStatusTask,
                executeTask
            };
    }



    /// <summary>
    /// 间隔执行
    /// </summary>
    protected abstract Task ProtectedExecuteAsync(object? state, CancellationToken cancellationToken);

    private void SetDeviceStatus(object? state, CancellationToken cancellationToken)
    {
        // 获取设备连接状态并更新设备活动时间
        if (IsConnected())
        {
            CurrentDevice?.SetDeviceStatus(TimerX.Now, false);
        }
        else
        {
            CurrentDevice?.SetDeviceStatus(TimerX.Now, true);
        }
    }
}

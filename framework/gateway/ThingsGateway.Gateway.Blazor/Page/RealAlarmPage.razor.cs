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

using Mapster;

using SqlSugar;

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// 实时报警
/// </summary>
public partial class RealAlarmPage
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(3));
    private IAppDataTable _datatable;
    private AlarmWorker _alarmHostService { get; set; }
    private VariablePageInput _search { get; set; } = new();

    /// <inheritdoc/>
    public override void Dispose()
    {
        _periodicTimer.Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _alarmHostService = BackgroundServiceUtil.GetBackgroundService<AlarmWorker>();
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private async Task DatatableQuery()
    {
        await _datatable?.QueryClickAsync();
    }

    private async Task<ISqlSugarPagedList<AlarmVariable>> QueryCallAsync(VariablePageInput input)
    {
        var devices = _alarmHostService.RealAlarmDeviceVariables.Adapt<List<AlarmVariable>>();
        var data = devices
            .WhereIF(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName.Contains(input.DeviceName))
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name))
            .ToList().ToPagedList(input);
        await Task.CompletedTask;
        return data;
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await InvokeAsync(DatatableQuery);
            }
            catch
            {
            }
        }
    }
}
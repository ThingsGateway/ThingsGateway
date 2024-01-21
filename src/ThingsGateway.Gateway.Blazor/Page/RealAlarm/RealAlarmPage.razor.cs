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

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor;
using ThingsGateway.Core;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class RealAlarmPage
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(2));
    private readonly AlarmVariablePageInput _search = new();
    private AlarmWorker AlarmWorker { get; set; }
    private IAppDataTable _datatable;

    protected override void OnInitialized()
    {
        AlarmWorker = WorkerUtil.GetWoker<AlarmWorker>();
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await InvokeAsync(async () => await _datatable?.QueryClickAsync());
            }
            catch
            {
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        _periodicTimer.Dispose();
        base.Dispose(disposing);
    }

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }

    private Task<SqlSugarPagedList<AlarmVariable>> QueryCallAsync(AlarmVariablePageInput input)
    {
        var devices = AlarmWorker.RealAlarmVariables.Adapt<List<AlarmVariable>>();
        var data = devices
          .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(!string.IsNullOrEmpty(input.RegisterAddress), u => u.RegisterAddress.Contains(input.RegisterAddress))
         .WhereIF(!string.IsNullOrEmpty(input.DeviceName), u => u.DeviceName == input.DeviceName)
            .ToList().ToPagedList(input);
        return Task.FromResult(data);
    }
}
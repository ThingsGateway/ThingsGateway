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

using SqlSugar;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor;
using ThingsGateway.Core;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class VariableStatus
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _periodicTimer?.Dispose();
        base.Dispose(disposing);
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }
        }
    }

    private readonly VariablePageInput _search = new();
    private IAppDataTable _datatable;

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }

    [Inject]
    private GlobalData GlobalData { get; set; }

    private Task<SqlSugarPagedList<VariableRunTime>> QueryCallAsync(VariablePageInput input)
    {
        return Task.Run(() =>
         {
             var data = GlobalData.AllVariables
   .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
  .WhereIF(!string.IsNullOrEmpty(input.RegisterAddress), u => u.RegisterAddress.Contains(input.RegisterAddress))
  .WhereIF(input.DeviceId > 0, u => u.DeviceId == input.DeviceId)
  .WhereIF(input.BusinessDeviceId > 0, u => u.VariablePropertys.ContainsKey(input.BusinessDeviceId!.Value))
     .ToList().ToPagedList(input);
             return data;
         });
    }
}

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------


using NewLife.Extension;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Json.Extension;
using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public partial class VariableRuntimePage : IDisposable
{
    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    public bool Disposed { get; set; }

    public void Dispose()
    {
        Disposed = true;
        DeviceDispatchService.UnSubscribe(Notify);
        GC.SuppressFinalize(this);
    }

    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                await InvokeAsync(StateHasChanged);
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }

    private VariableRunTime? SearchModel { get; set; } = new();
    protected IEnumerable<SelectedItem> CollectDeviceNames;

    [Inject]
    [NotNull]
    private IDispatchService<DeviceRunTime>? DeviceDispatchService { get; set; }

    protected override Task OnInitializedAsync()
    {
        DeviceDispatchService.Subscribe(Notify);
        return base.OnInitializedAsync();
    }

    private async Task Notify(DispatchEntry<DeviceRunTime> entry)
    {
        await Change();
        await InvokeAsync(StateHasChanged);
    }

    private async Task Change()
    {
        await OnParametersSetAsync();
    }

    protected override Task OnParametersSetAsync()
    {
        CollectDeviceNames = GlobalData.ReadOnlyCollectDevices.Keys.Select(a => new SelectedItem(a, a)).Concat(new List<SelectedItem>() { new SelectedItem(string.Empty, "none") });
        return base.OnParametersSetAsync();
    }

    #region 查询

    private Task<QueryData<VariableRunTime>> OnQueryAsync(QueryPageOptions options)
    {
        var data = GlobalData.ReadOnlyVariables.Values
            .WhereIF(!options.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(options.SearchText))
            .WhereIF(!SearchModel.Name.IsNullOrWhiteSpace(), a => a.Name.Contains(SearchModel.Name))
            .WhereIF(!SearchModel.RegisterAddress.IsNullOrWhiteSpace(), a => a.RegisterAddress.Contains(SearchModel.RegisterAddress))
            .WhereIF(!SearchModel.DeviceName.IsNullOrWhiteSpace(), a => a.DeviceName.Contains(SearchModel.DeviceName))

            .GetQueryData(options);
        return Task.FromResult(data);
    }

    #endregion 查询

    #region 写入变量

    private string WriteValue { get; set; }

    private async Task OnWriteVariable(VariableRunTime variableRunTime)
    {
        try
        {
            var data = await variableRunTime.SetValueToDeviceAsync(WriteValue);
            if (!data.IsSuccess)
            {
                await ToastService.Warning(null, data.ErrorMessage);
            }
            else
            {
                await ToastService.Default();
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    #endregion 写入变量

    /// <summary>
    /// IntFormatter
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    private static Task<string> JsonFormatter(object? d)
    {
        var ret = "";
        if (d is TableColumnContext<VariableRunTime, object?> data && data?.Value != null)
        {
            ret = data.Value.ToSystemTextJsonString();
        }
        return Task.FromResult(ret);
    }
}

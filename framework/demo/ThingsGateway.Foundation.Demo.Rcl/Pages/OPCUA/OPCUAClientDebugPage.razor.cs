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

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

using Newtonsoft.Json.Linq;

using Opc.Ua;

using ThingsGateway.Foundation.Adapter.OPCUA;

namespace ThingsGateway.Foundation.Demo;
/// <summary>
/// OPCUA调试页面
/// </summary>
public partial class OPCUAClientDebugPage
{
    private ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient _plc;
    private DriverDebugUIPage _driverDebugUIPage;
    bool IsShowImportVariableList;
    private OPCUAClientPage opcUAClientPage;
    private OPCUAImportVariable ImportVariable { get; set; }
    [Inject]
    private InitTimezone InitTimezone { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void Dispose()
    {
        _plc.SafeDispose();
        opcUAClientPage.SafeDispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return nameof(OPCUAClient);
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (opcUAClientPage != null)
            {
                opcUAClientPage.LogAction = _driverDebugUIPage.LogOut;
                _plc = opcUAClientPage.OPC;
                _plc.DataChangedHandler += Plc_DataChangedHandler;
            }
            //载入配置
            StateHasChanged();
            _driverDebugUIPage.Sections.Clear();

        }

        base.OnAfterRender(firstRender);
    }
    private async Task Add()
    {
        if (_plc.Connected)
            await _plc.AddSubscriptionAsync(Guid.NewGuid().ToString(), new[] { _driverDebugUIPage.Address });
        else
        {
            _driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)} - 未连接"));
        }
    }

#if Plugin
    private async Task DeviceImport()
    {
        isDownLoading = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            var data = await ImportVariable?.GetImportVariableListAsync();
            if (data.Item2?.Count == 0)
            {
                await PopupService.EnqueueSnackbarAsync("无可用变量", AlertTypes.Warning);
                return;
            }
            await _serviceScope.ServiceProvider.GetService<ICollectDeviceService>().AddAsync(data.Item1);
            await _serviceScope.ServiceProvider.GetService<VariableService>().AddBatchAsync(data.Item2);
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
        }
        finally
        {
            isDownLoading = false;
        }

    }
    private IJSObjectReference JSObjectReference;
    private bool isDownLoading;
    /// <inheritdoc/>
    [Inject]
    protected IJSRuntime JSRuntime { get; set; }
    private async Task DownDeviceExport()
    {
        isDownLoading = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            var data = await ImportVariable?.GetImportVariableListAsync();
            if (data.Item2?.Count == 0)
            {
                await PopupService.EnqueueSnackbarAsync("无可用变量", AlertTypes.Warning);
                return;
            }

            await DownDeviceExportAsync(data.Item1);
            await DownDeviceVariableExportAsync(data.Item2, data.Item1.Name);
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
        }
        finally
        {
            isDownLoading = false;
        }

    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceExportAsync(CollectDevice data)
    {
        using var memoryStream = await _serviceScope.ServiceProvider.GetService<ICollectDeviceService>().ExportFileAsync(new List<CollectDevice>() { data });
        using var streamRef = new DotNetStreamReference(stream: memoryStream);
        JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
        await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"设备导出{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx", streamRef);
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceVariableExportAsync(List<DeviceVariable> data, string devName)
    {
        using var memoryStream = await _serviceScope.ServiceProvider.GetService<VariableService>().ExportFileAsync(data, devName);
        using var streamRef = new DotNetStreamReference(stream: memoryStream);
        JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
        await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"变量导出{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx", streamRef);
    }

#endif

    private void Plc_DataChangedHandler((VariableNode variableNode, DataValue dataValue, Newtonsoft.Json.Linq.JToken jToken) item)
    {
        _driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug,
                        $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)}  - {item.variableNode.NodeId}：{item.jToken}"));
        if (_driverDebugUIPage.Messages.Count > 2500)
        {
            _driverDebugUIPage.Messages.Clear();
        }

    }
    private async Task ReadAsync()
    {
        if (_plc.Connected)
        {
            try
            {
                var data = await _plc.ReadJTokenValueAsync(new string[] { _driverDebugUIPage.Address });
                _driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug,
                        $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)}  - {data[0].Item1}：{data[0].Item3}"));
            }
            catch (Exception ex)
            {

                _driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning,
                        $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)}  - {ex}"));
            }
        }
        else
        {
            _driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning,
                        $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)}  - 未连接"));
        }
    }
    private void Remove()
    {
        if (_plc.Connected)
            _plc.RemoveSubscription("");
        else
        {
            _driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning,
                        $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)}  - 未连接"));
        }
    }

    private async Task WriteAsync()
    {
        try
        {
            if (_plc.Connected)
            {
                var data = await _plc.WriteNodeAsync(
                    new()
                    {
                        { _driverDebugUIPage.Address, JToken.Parse(_driverDebugUIPage.WriteValue)}
                    }
                    );

                foreach (var item in data)
                {
                    _driverDebugUIPage.Messages.Add((item.Value.Item1 ? Microsoft.Extensions.Logging.LogLevel.Warning : Microsoft.Extensions.Logging.LogLevel.Information,
                        $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)}  - {item.Value.Item2}"));
                }
            }
            else
            {
                _driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning,
                        $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)}  - 未连接"));
            }
        }
        catch (Exception ex)
        {
            _driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error,
                        $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)}  - {ex}"));
        }
    }
}
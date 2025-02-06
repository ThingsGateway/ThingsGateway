//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class DeviceRuntimeInfo1 : IDisposable
{
    [Inject]
    IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }

    [Parameter, EditorRequired]
    public DeviceRuntime DeviceRuntime { get; set; }
    private string Name => $"{DeviceRuntime.ToString()}  -  {(DeviceRuntime.Driver?.DeviceThreadManage == null ? "Task cancel" : "Task run")}";
    public ModelValueValidateForm PluginPropertyModel;

    protected override void OnParametersSet()
    {
        if (PluginPropertyModel?.Value == null || PluginPropertyModel?.Value != DeviceRuntime.Driver?.DriverProperties)
        {
            PluginPropertyModel = new ModelValueValidateForm()
            {
                Value = DeviceRuntime.Driver?.DriverProperties
            };
        }
        base.OnParametersSet();
    }



    private async Task ShowDriverUI()
    {
        var driver = DeviceRuntime.Driver?.DriverUIType;
        if (driver == null)
        {
            return;
        }
        await DialogService.Show(new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraExtraLarge,
            Title = DeviceRuntime.Name,
            Component = BootstrapDynamicComponent.CreateComponent(driver, new Dictionary<string, object?>()
        {
            {nameof(IDriverUIBase.Driver),DeviceRuntime.Driver},
        })
        });
    }
    private async Task DeviceRedundantThreadAsync()
    {
        if (GlobalData.TryGetDeviceThreadManage(DeviceRuntime, out var deviceThreadManage))
        {
            await deviceThreadManage.DeviceRedundantThreadAsync(DeviceRuntime.Id);
        }
    }
    private async Task RestartDeviceAsync(bool deleteCache)
    {
        if (GlobalData.TryGetDeviceThreadManage(DeviceRuntime, out var deviceThreadManage))
        {
            await deviceThreadManage.RestartDeviceAsync(DeviceRuntime, deleteCache);
        }
    }
    private void PauseThread()
    {
        if (DeviceRuntime.Driver != null)
            DeviceRuntime.Driver.PauseThread(!DeviceRuntime.Pause);
    }

    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private bool Disposed;
    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                await InvokeAsync(() =>
                {
                    OnParametersSet();
                    StateHasChanged();
                });
            }
            catch (Exception ex)
            {
                NewLife.Log.XTrace.WriteException(ex);
            }
            finally
            {
                await Task.Delay(5000);
            }
        }
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}

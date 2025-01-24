//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Modbus;
using ThingsGateway.Razor;

namespace ThingsGateway.Debug;

public partial class ModbusAddressComponent : ComponentBase, IAddressUIBase
{
    [Parameter]
    public string Model { get; set; }
    [Parameter]
    public Action<string> ModelChanged { get; set; }
    ModbusAddress Value = new();
    public ConverterConfig ConverterConfig { get; set; }
    protected override void OnParametersSet()
    {
        ConverterConfig = new ConverterConfig(Model);
        var data = ModbusAddress.ParseFrom(Model, isCache: false);
        if (data != null) Value = data;
        base.OnParametersSet();
    }
    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }
    private async Task OnValidSubmit(EditContext editContext)
    {
        try
        {

            Model = $"{ConverterConfig.ToString()}{Value?.ToString()}";

            if (ModelChanged != null)
                ModelChanged.Invoke(Model);
            if (OnCloseAsync != null)
                await OnCloseAsync();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }
    [Inject]
    IStringLocalizer<ThingsGateway.Razor._Imports> RazorLocalizer { get; set; }
    [Inject]
    ToastService ToastService { get; set; }
}

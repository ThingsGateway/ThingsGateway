//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

using System.Collections.Concurrent;

using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

public partial class VariableEditComponent
{
    public long ChoiceBusinessDeviceId;

    [Parameter]
    public bool BatchEditEnable { get; set; }

    private IEnumerable<SelectedItem> BusinessDeviceItems { get; set; }

    private IEnumerable<SelectedItem> CollectDeviceItems { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
        CollectDeviceItems = devices.Where(a => a.Value.IsCollect == true).Select(a => a.Value).BuildDeviceSelectList();
        BusinessDeviceItems = devices.Where(a => a.Value.IsCollect == false).Select(a => a.Value).BuildDeviceSelectList();
        base.OnParametersSet();
    }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public Variable? Model { get; set; }

    [Parameter]
    public Func<Task> OnValidSubmit { get; set; }


    [Parameter]
    public bool ValidateEnable { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    public IEnumerable<SelectedItem> OtherMethods { get; set; }
    private ConcurrentDictionary<long, IEnumerable<IEditorItem>>? VariablePropertyEditors { get; set; } = new();
    private ConcurrentDictionary<long, RenderFragment>? VariablePropertyRenderFragments { get; set; } = new();

    public async Task ValidSubmit(EditContext editContext)
    {
        try
        {
            if (OnValidSubmit != null)
                await OnValidSubmit.Invoke();
            if (OnCloseAsync != null)
                await OnCloseAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Model.VariablePropertys ??= new();
        foreach (var item in Model.VariablePropertys)
        {
            await RefreshBusinessPropertyClickAsync(item.Key);
        }
    }


    private async Task OnDeviceChanged(SelectedItem selectedItem)
    {
        try
        {
            if (GlobalData.ReadOnlyDevices.TryGetValue(selectedItem.Value.ToLong(), out var device))
            {
                var data = GlobalData.PluginService.GetDriverMethodInfos(device.PluginName);
                OtherMethods = new List<SelectedItem>() { new SelectedItem(string.Empty, "none") }.Concat(data.Select(a => new SelectedItem(a.Name, a.Description)));


                AddressUIType = GlobalData.PluginService.GetAddressUI(device.PluginName);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }
    private BootstrapDynamicComponent AddressDynamicComponent;
    private Type AddressUIType;

    private async Task ShowAddressUI()
    {

        if (AddressUIType != null)
        {
            AddressDynamicComponent = new BootstrapDynamicComponent(AddressUIType, new Dictionary<string, object?>
            {
                [nameof(IAddressUIBase.Model)] = Model.RegisterAddress,

                [nameof(IAddressUIBase.ModelChanged)] =
                     (string address) =>
                    {
                        Model.RegisterAddress = address;
                    }
            });
        }
        else
            return;
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = $"{Model.Name} Address",
            ShowFooter = false,
            ShowCloseButton = false,
        };
        op.Component = AddressDynamicComponent;
        await DialogService.Show(op);
    }


    [Inject]
    private IStringLocalizer<Device> DeviceLocalizer { get; set; }
    private async Task AddDevice(MouseEventArgs args)
    {
        Device oneModel = new();

        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = DeviceLocalizer["SaveDevice"],
            ShowFooter = false,
            ShowCloseButton = false,
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<DeviceEditComponent>(new Dictionary<string, object?>
        {
             {nameof(DeviceEditComponent.OnValidSubmit), async () =>
             {
                await GlobalData.DeviceRuntimeService.SaveDeviceAsync(oneModel,ItemChangedType.Add,AutoRestartThread);
                 OnParametersSet();
            }},
            {nameof(DeviceEditComponent.Model),oneModel },
            {nameof(DeviceEditComponent.ValidateEnable),true },
            {nameof(DeviceEditComponent.BatchEditEnable),false },
        });

        await DialogService.Show(op);
    }

    private async Task RefreshBusinessPropertyClickAsync(long id)
    {
        if (id > 0)
        {
            if (GlobalData.ReadOnlyDevices.TryGetValue(id, out var device))
            {
                var data = GlobalData.PluginService.GetVariablePropertyTypes(device.PluginName);
                Model.VariablePropertyModels ??= new();
                Model.VariablePropertyModels.AddOrUpdate(id, (a) => new ModelValueValidateForm() { Value = data.Model }, (a, b) => new ModelValueValidateForm() { Value = data.Model });
                VariablePropertyEditors.TryAdd(id, data.EditorItems);

                if (data.VariablePropertyUIType != null)
                {
                    var component = new BootstrapDynamicComponent(data.VariablePropertyUIType, new Dictionary<string, object?>
                    {
                        [nameof(VariableEditComponent.Model)] = Model,
                        [nameof(DeviceEditComponent.PluginPropertyEditorItems)] = data.EditorItems,
                    });
                    VariablePropertyRenderFragments.AddOrUpdate(id, component.Render());
                }

                if (Model.VariablePropertys.TryGetValue(id, out var dict))
                {
                    PluginServiceUtil.SetModel(data.Model, dict);
                }
            }
        }
        else
        {
            await ToastService.Warning(null, GatewayLocalizer["RefreshBusinessPropertyError"]);
        }
    }
    [Inject]
    IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }
    [Parameter]
    public bool AutoRestartThread { get; set; }
}

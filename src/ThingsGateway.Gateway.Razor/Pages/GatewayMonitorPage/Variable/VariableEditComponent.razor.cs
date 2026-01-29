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

using ThingsGateway.Debug;
using ThingsGateway.Foundation.Common.Json.Extension;
using ThingsGateway.Foundation.Common.StringExtension;



namespace ThingsGateway.Gateway.Razor;

public partial class VariableEditComponent
{
    /// <summary>
    /// IntFormatter
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    private static string JsonFormatter(object? d)
    {
        var ret = "";
        if (d != null)
        {
            ret = d.ToSystemTextJsonString();
        }
        return ret;
    }
    public long ChoiceBusinessDeviceId;

    [Parameter]
    public bool BatchEditEnable { get; set; }

    //private IEnumerable<SelectedItem> BusinessDeviceItems { get; set; }

    //private IEnumerable<SelectedItem> CollectDeviceItems { get; set; }
    [Inject]
    IDevicePageService DevicePageService { get; set; }
    private string DeviceName;
    private string ChoiceBusinessDeviceName;
    private bool first = false;


    private bool _initialized;


    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
        if (DeviceName.IsNullOrEmpty() && first == false)
        {
            first = true;
            DeviceName = await DevicePageService.GetDeviceNameAsync(Model?.DeviceId ?? 0);
            if (!_initialized)
            {
                _initialized = true;

                OnInitialized();
                await OnInitializedAsync();
                OnParametersSet();

                ChoiceBusinessDeviceId = ChoiceBusinessDeviceId > 0 ? ChoiceBusinessDeviceId : (await DevicePageService.OnDeviceSelectedItemQueryAsync(new VirtualizeQueryOption() { Count = 1 }, false).ConfigureAwait(false)).Items.FirstOrDefault()?.Value?.ToLong() ?? 0;
                ChoiceBusinessDeviceName = (await DevicePageService.GetDeviceNameAsync(ChoiceBusinessDeviceId)) ?? string.Empty;

                await InvokeAsync(StateHasChanged);
                await OnParametersSetAsync();
            }
            else
            {
                OnParametersSet();

                ChoiceBusinessDeviceId = ChoiceBusinessDeviceId > 0 ? ChoiceBusinessDeviceId : (await DevicePageService.OnDeviceSelectedItemQueryAsync(new VirtualizeQueryOption() { Count = 1 }, false).ConfigureAwait(false)).Items.FirstOrDefault()?.Value?.ToLong() ?? 0;
                ChoiceBusinessDeviceName = (await DevicePageService.GetDeviceNameAsync(ChoiceBusinessDeviceId)) ?? string.Empty;
                await InvokeAsync(StateHasChanged);

                await OnParametersSetAsync();
            }


            await OnParametersSetAsync();

        }
    }

    protected override async Task OnParametersSetAsync()
    {

        if (Model.DeviceId > 0 && AddressUIType == null)
        {
            await SetAddressUI(Model.DeviceId);
        }

        await base.OnParametersSetAsync();
    }
    private Task<QueryData<SelectedItem>> OnCollectDeviceSelectedItemQueryAsync(VirtualizeQueryOption option)
    {
        return DevicePageService.OnDeviceSelectedItemQueryAsync(option, true);
    }
    private Task<QueryData<SelectedItem>> OnBusinessDeviceSelectedItemQueryAsync(VirtualizeQueryOption option)
    {
        return DevicePageService.OnDeviceSelectedItemQueryAsync(option, false);
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

    private NonBlockingDictionary<long, IEnumerable<IEditorItem>>? VariablePropertyEditors { get; set; } = new();
    private NonBlockingDictionary<long, RenderFragment>? VariablePropertyRenderFragments { get; set; } = new();

    public async Task ValidSubmit(EditContext editContext)
    {
        try
        {
            if (await Model.AlarmPropertysValidateForm?.ValidateAsync() == false)
            {
                return;
            }

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

    private Dictionary<long, Tuple<string, string>> DeviceIdNames { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Model.AlarmPropertys ??= new();
        Model.VariablePropertys ??= new();

        DeviceIdNames = await DevicePageService.GetDeviceIdNamesAsync();
        foreach (var item in Model.VariablePropertys)
        {
            await RefreshBusinessPropertyClickAsync(item.Key);
        }
    }

    Dictionary<string, string> OtherMethods = new Dictionary<string, string>();
    Dictionary<string, string> OtherMethodRemarks = new Dictionary<string, string>();
    public IEnumerable<SelectedItem> OtherMethodSelectedItems { get; set; }

    private async Task OnDeviceChanged(SelectedItem selectedItem)
    {
        var deviceId = selectedItem.Value.ToLong();
        await SetAddressUI(deviceId);
    }
    [Inject]
    IPluginService PluginService { get; set; }
    private async Task SetAddressUI(long deviceId)
    {
        try
        {
            var pluginName = await DevicePageService.GetDevicePluginNameAsync(deviceId);
            if (!pluginName.IsNullOrWhiteSpace())
            {
                OtherMethods = PluginService.GetDriverMethodInfos(pluginName).ToDictionary(a => a.Name, a => a.Description);
                OtherMethodRemarks = PluginService.GetDriverMethodInfos(pluginName).ToDictionary(a => a.Name, a => a.Remark);
                OtherMethodSelectedItems = OtherMethods.Select(a => new SelectedItem(a.Key, a.Value));
                AddressDesc = PluginService.GetDriver(pluginName) is CollectBase collectBase ? collectBase.GetAddressDescription() : string.Empty;
                AddressUIType = PluginService.GetAddressUI(pluginName);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }
    private Task OnOtherMethodChanged(SelectedItem selectedItem)
    {

        if (OtherMethodRemarks.TryGetValue(selectedItem.Value, out var remark))
            AddressDesc = remark;

        return Task.CompletedTask;
    }
    private BootstrapDynamicComponent AddressDynamicComponent;
    private Type AddressUIType;
    private string AddressDesc;

    private async Task ShowAddressUI()
    {
        if (AddressUIType != null)
        {
            AddressDynamicComponent = new BootstrapDynamicComponent(AddressUIType, new Dictionary<string, object?>
            {
                [nameof(IAddressUIBase.Model)] = Model.RegisterAddress,
                [nameof(IAddressUIBase.ModelChanged)] =
                     (Action<string>)(address => Model.RegisterAddress = address)
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
    IStringLocalizer<Variable> VariableLocalizer { get; set; }
    private async Task ShowExpressionsUI(bool read)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.Large,
            Title = $"{Model.Name} {(read ? VariableLocalizer[nameof(Variable.ReadExpressions)] : VariableLocalizer[nameof(Variable.WriteExpressions)])}",
            ShowFooter = false,
            ShowCloseButton = false,
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<ValueTransformConfigPage>(new Dictionary<string, object?>
        {
             {nameof(ValueTransformConfigPage.ExpressionsChanged),  EventCallback.Factory.Create<string>(this,a =>
             {
                  if(read)
                 {
                     Model.ReadExpressions = a;
                 }
                else
                 {
                     Model.WriteExpressions=a;
                 }
            })
},
            {nameof(ValueTransformConfigPage.Expressions),read?Model.ReadExpressions:Model.WriteExpressions },
        }).Render(),
        };
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
                await Task.Run(() =>DevicePageService.SaveDeviceAsync(oneModel,ItemChangedType.Add,AutoRestartThread));
                 OnParametersSet();
            }},
            {nameof(DeviceEditComponent.Model),oneModel },
            {nameof(DeviceEditComponent.AutoRestartThread),AutoRestartThread },
            {nameof(DeviceEditComponent.ValidateEnable),true },
            {nameof(DeviceEditComponent.BatchEditEnable),false },
        });

        await DialogService.Show(op);
    }

    private async Task RefreshBusinessPropertyClickAsync(long id)
    {
        if (id > 0)
        {
            var pluginName = await DevicePageService.GetDevicePluginNameAsync(id);
            if (!pluginName.IsNullOrWhiteSpace())
            {
                var data = PluginService.GetVariablePropertyTypes(pluginName);
                Model.VariablePropertyModels ??= new();
                Model.VariablePropertyModels.AddOrUpdate(id, (a) => new ModelValueValidateForm() { Value = data.Model }, (a, b) =>
                { b.Value = data.Model; return b; });
                VariablePropertyEditors.TryAdd(id, data.EditorItems);

                if (data.VariablePropertyUIType != null)
                {
                    var component = new BootstrapDynamicComponent(data.VariablePropertyUIType, new Dictionary<string, object?>
                    {
                        [nameof(IPropertyUIBase.Model)] = Model,
                        [nameof(IPropertyUIBase.PluginPropertyEditorItems)] = data.EditorItems,
                    });
                    VariablePropertyRenderFragments.AddOrUpdate(id, (a) => component.Render(), (a, b) => component.Render());
                }

                if (Model.VariablePropertys?.TryGetValue(id, out var dict) == true)
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

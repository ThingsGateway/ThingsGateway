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

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Common.Extension;
using ThingsGateway.Foundation.Common.StringExtension;

namespace ThingsGateway.Gateway.Razor;

public partial class DeviceEditComponent
{
    [Inject]
    IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> GatewayLocalizer { get; set; }

    [Parameter]
    public bool BatchEditEnable { get; set; }

    [Parameter]
    [EditorRequired]
    public Device Model { get; set; }

    [Parameter]
    public bool AutoRestartThread { get; set; }

    [Parameter]
    public Func<Task> OnValidSubmit { get; set; }

    [Parameter]
    public bool ValidateEnable { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    [Inject]
#if Management
    ThingsGateway.Management.Application.IChannelPageService ChannelPageService { get; set; }
#else
    ThingsGateway.Gateway.Application.IChannelPageService ChannelPageService { get; set; }
#endif
    private Task<QueryData<SelectedItem>> OnChannelSelectedItemQueryAsync(VirtualizeQueryOption option)
    {
        return ChannelPageService.OnChannelSelectedItemQueryAsync(option);
    }


    public async Task ValidSubmit(EditContext editContext)
    {
        try
        {
            var result = (!PluginServiceUtil.HasDynamicProperty(Model.ModelValueValidateForm.Value)) || (Model.ModelValueValidateForm.ValidateForm != null && await Model.ModelValueValidateForm.ValidateForm.ValidateAsync() != false);
            if (!result)
            {
                // 进行设备对象属性的验证
                var validationContext = new ValidationContext(Model.ModelValueValidateForm.Value);
                var validationResults = new List<ValidationResult>();
                validationContext.ValidateProperty(validationResults);

                if (validationResults.Any(v => !string.IsNullOrEmpty(v.ErrorMessage)))
                    return;
            }

            Model.DevicePropertys = PluginServiceUtil.SetDict(Model.ModelValueValidateForm.Value);

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

    [Inject]
    private IStringLocalizer<Channel> ChannelLocalizer { get; set; }
    private async Task AddChannel(MouseEventArgs args)
    {
        Channel oneModel = new();

        var op = new DialogOption()
        {
            IsScrolling = false,
            ShowMaximizeButton = true,
            Size = Size.ExtraLarge,
            Title = ChannelLocalizer["SaveChannel"],
            ShowFooter = false,
            ShowCloseButton = false,
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelEditComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelEditComponent.OnValidSubmit), async () =>
             {
                await Task.Run(() =>ChannelPageService.SaveChannelAsync(oneModel,ItemChangedType.Add,AutoRestartThread));
                 OnParametersSet();
            }},
            {nameof(ChannelEditComponent.Model),oneModel },
            {nameof(ChannelEditComponent.ValidateEnable),true },
            {nameof(ChannelEditComponent.BatchEditEnable),false },
            {nameof(ChannelEditComponent.PluginType),  null },
        });

        await DialogService.Show(op);
    }
    [Inject]
    IDevicePageService DevicePageService { get; set; }

    private Task<QueryData<SelectedItem>> OnRedundantDevicesQuery(VirtualizeQueryOption option, Device device)
    {
        return DevicePageService.OnRedundantDevicesQueryAsync(option, device.Id, device.ChannelId);
    }

    private string ChannelName;
    private string DeviceName;
    private bool _initialized;
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
        if (ChannelName.IsNullOrEmpty())
        {
            ChannelName = await ChannelPageService.GetChannelNameAsync(Model?.ChannelId ?? 0);
            DeviceName = await DevicePageService.GetDeviceNameAsync(Model?.RedundantDeviceId ?? 0);
            if (!_initialized)
            {
                _initialized = true;

                OnInitialized();
                await OnInitializedAsync();
                OnParametersSet();
                StateHasChanged();
                await OnParametersSetAsync();
            }
            else
            {
                OnParametersSet();
                StateHasChanged();
                await OnParametersSetAsync();
            }
        }

    }




    internal IEnumerable<IEditorItem> PluginPropertyEditorItems;
    private RenderFragment PluginPropertyRenderFragment;

    [Inject]
    IPluginService PluginService { get; set; }
    private async Task OnChannelChanged(SelectedItem selectedItem)
    {
        try
        {
            var pluginName = await ChannelPageService.GetPluginNameAsync(selectedItem.Value.ToLong());
            if (pluginName.IsNullOrEmpty()) return;

            var data = PluginService.GetDriverPropertyTypes(pluginName);
            Model.ModelValueValidateForm = new ModelValueValidateForm() { Value = data.Model };
            PluginPropertyEditorItems = data.EditorItems;
            if (data.PropertyUIType != null)
            {
                var component = new BootstrapDynamicComponent(data.PropertyUIType, new Dictionary<string, object?>
                {
                    [nameof(IPropertyUIBase.Id)] = Model.Id.ToString(),
                    [nameof(IPropertyUIBase.CanWrite)] = true,
                    [nameof(IPropertyUIBase.Model)] = Model.ModelValueValidateForm,
                    [nameof(IPropertyUIBase.PluginPropertyEditorItems)] = PluginPropertyEditorItems,
                });
                PluginPropertyRenderFragment = component.Render();
            }
            if (Model.DevicePropertys?.Count > 0)
            {
                PluginServiceUtil.SetModel(Model.ModelValueValidateForm.Value, Model.DevicePropertys);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }
}

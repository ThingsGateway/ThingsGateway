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

using ThingsGateway.Extension.Generic;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Extension;

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
    public Func<Task> OnValidSubmit { get; set; }

    [Parameter]
    public bool ValidateEnable { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }
    private IEnumerable<SelectedItem> _channelItems;

    protected override async Task OnParametersSetAsync()
    {
        var channels = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
        _channelItems = channels.Select(a => a.Value).BuildChannelSelectList();
        base.OnParametersSet();
    }

    public ModelValueValidateForm PluginPropertyModel;

    public async Task ValidSubmit(EditContext editContext)
    {
        try
        {
            var result = (!PluginServiceUtil.HasDynamicProperty(PluginPropertyModel.Value)) || (PluginPropertyModel.ValidateForm?.Validate() != false);
            if (!result) return;

            Model.DevicePropertys = PluginServiceUtil.SetDict(PluginPropertyModel.Value);

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
                await GlobalData.ChannelRuntimeService.SaveChannelAsync(oneModel,ItemChangedType.Add);
                 OnParametersSet();
            }},
            {nameof(ChannelEditComponent.Model),oneModel },
            {nameof(ChannelEditComponent.ValidateEnable),true },
            {nameof(ChannelEditComponent.BatchEditEnable),false },
            {nameof(ChannelEditComponent.PluginType),  null },
        });

        await DialogService.Show(op);
    }

    private static async Task<QueryData<SelectedItem>> OnRedundantDevicesQuery(VirtualizeQueryOption option, Device device)
    {
        var ret = new QueryData<SelectedItem>()
        {
            IsSorted = false,
            IsFiltered = false,
            IsAdvanceSearch = false,
            IsSearch = !option.SearchText.IsNullOrWhiteSpace()
        };

        var devices = await GlobalData.GetCurrentUserDevices().ConfigureAwait(false);
        var pluginName = GlobalData.ReadOnlyChannels.TryGetValue(device.ChannelId, out var channel) ? channel.PluginName : string.Empty;
        var items = new List<SelectedItem>() { new SelectedItem(string.Empty, "none") }.Concat(devices.WhereIf(!option.SearchText.IsNullOrWhiteSpace(), a => a.Value.Name.Contains(option.SearchText))
            .Where(a => a.Value.PluginName == pluginName && a.Value.Id != device.Id).Select(a => a.Value).BuildDeviceSelectList()
            );

        ret.TotalCount = items.Count();
        ret.Items = items;
        return ret;
    }

    internal IEnumerable<IEditorItem> PluginPropertyEditorItems;
    private RenderFragment PluginPropertyRenderFragment;

    private async Task OnChannelChanged(SelectedItem selectedItem)
    {
        try
        {
            var pluginName = GlobalData.ReadOnlyChannels.TryGetValue(selectedItem.Value.ToLong(), out var channel) ? channel.PluginName : string.Empty;

            var data = GlobalData.PluginService.GetDriverPropertyTypes(pluginName);
            PluginPropertyModel = new ModelValueValidateForm() { Value = data.Model };
            PluginPropertyEditorItems = data.EditorItems;
            if (data.PropertyUIType != null)
            {
                var component = new BootstrapDynamicComponent(data.PropertyUIType, new Dictionary<string, object?>
                {
                    [nameof(IPropertyUIBase.Id)] = Model.Id.ToString(),
                    [nameof(IPropertyUIBase.CanWrite)] = true,
                    [nameof(IPropertyUIBase.Model)] = PluginPropertyModel,
                    [nameof(IPropertyUIBase.PluginPropertyEditorItems)] = PluginPropertyEditorItems,
                });
                PluginPropertyRenderFragment = component.Render();
            }
            if (Model.DevicePropertys?.Count > 0)
            {
                PluginServiceUtil.SetModel(PluginPropertyModel.Value, Model.DevicePropertys);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }


}

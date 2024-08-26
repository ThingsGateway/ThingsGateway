//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Razor;

public partial class DeviceEditComponent
{
    private IEnumerable<IEditorItem> PluginPropertyEditorItems;

    [Parameter]
    public bool BatchEditEnable { get; set; }

    [NotNull]
    public IEnumerable<SelectedItem> Channels { get; set; }

    [Parameter]
    public Dictionary<long, string> DeviceDict { get; set; } = new();

    [Parameter]
    [EditorRequired]
    [NotNull]
    public Device? Model { get; set; }

    [Parameter]
    public Func<Task> OnValidSubmit { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public IEnumerable<SelectedItem> PluginNames { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public PluginTypeEnum PluginType { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public Func<VirtualizeQueryOption, Device, Task<QueryData<SelectedItem>>> RedundantDevicesQuery { get; set; }

    [Parameter]
    public bool ValidateEnable { get; set; }

    [Inject]
    private IStringLocalizer<Channel> ChannelLocalizer { get; set; }

    [Inject]
    [NotNull]
    private IChannelService ChannelService { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    [Inject]
    [NotNull]
    private IPluginService PluginService { get; set; }

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
            await ToastService.Warning(ex.Message);
        }
    }

    protected override void OnParametersSet()
    {
        Channels = ChannelService.GetAll().BuildChannelSelectList();
        base.OnParametersSet();
    }

    private async Task AddChannel(MouseEventArgs args)
    {
        Channel channel = new();

        var op = new DialogOption()
        {
            IsScrolling = false,
            Title = ChannelLocalizer["SaveChannel"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelEditComponent>(new Dictionary<string, object?>
        {
            [nameof(ChannelEditComponent.Model)] = channel,
            [nameof(ChannelEditComponent.ValidateEnable)] = true,
            [nameof(ChannelEditComponent.OnValidSubmit)] = async () =>
            {
                await ChannelService.SaveChannelAsync(channel, ItemChangedType.Add);
                Model.ChannelId = channel.Id;
            },
        });
        await DialogService.Show(op);
    }

    private Task OnPluginNameChanged(SelectedItem selectedItem)
    {
        try
        {
            var data = PluginService.GetDriverPropertyTypes(selectedItem?.Value);
            Model.PluginPropertyModel = new ModelValueValidateForm() { Value = data.Model };
            PluginPropertyEditorItems = data.EditorItems;
            if (Model.DevicePropertys?.Any() == true)
            {
                PluginServiceUtil.SetModel(Model.PluginPropertyModel.Value, Model.DevicePropertys);
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
        }
        return Task.CompletedTask;
    }

    private async Task<QueryData<SelectedItem>> OnRedundantDevicesQuery(VirtualizeQueryOption option)
    {
        if (RedundantDevicesQuery != null)
            return await RedundantDevicesQuery.Invoke(option, Model);
        else
            return new();
    }

    private async Task CheckScript(BusinessPropertyWithCacheIntervalScript businessProperty, string pname)
    {
        IEnumerable<object> data = null;
        string script = null;
        if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel))
        {
            data = new List<AlarmVariable>() { new() {
                    Name = "testName",
                    DeviceName = "testDevice",
                    AlarmCode = "1",
                    AlarmTime = DateTime.Now,
                    EventTime = DateTime.Now,
                    AlarmLimit = "3",
                    AlarmType = AlarmTypeEnum.L,
                    EventType=EventTypeEnum.Alarm,
                    Remark1="1",
                    Remark2="2",
                    Remark3="3",
                    Remark4="4",
                    Remark5="5",
                },
                 new() {
                    Name = "testName2",
                    DeviceName = "testDevice",
                    AlarmCode = "1",
                    AlarmTime = DateTime.Now,
                    EventTime = DateTime.Now,
                    AlarmLimit = "3",
                    AlarmType = AlarmTypeEnum.L,
                    EventType=EventTypeEnum.Alarm,
                    Remark1="1",
                    Remark2="2",
                    Remark3="3",
                    Remark4="4",
                    Remark5="5",
                }};
            script = businessProperty.BigTextScriptAlarmModel;
        }
        else if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptVariableModel))
        {
            data = new List<VariableBasicData>() { new() {
                    Name = "testName",
                    DeviceName = "testDevice",
                    Value = "1",
                    ChangeTime = DateTime.Now,
                    CollectTime = DateTime.Now,
                    Remark1="1",
                    Remark2="2",
                    Remark3="3",
                    Remark4="4",
                    Remark5="5",
                } ,
                 new() {
                    Name = "testName2",
                    DeviceName = "testDevice",
                    Value = "1",
                    ChangeTime = DateTime.Now,
                    CollectTime = DateTime.Now,
                    Remark1="1",
                    Remark2="2",
                    Remark3="3",
                    Remark4="4",
                    Remark5="5",
                } };
            script = businessProperty.BigTextScriptVariableModel;

        }
        else if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel))
        {
            data = new List<DeviceBasicData>() { new() {
                    Name = "testDevice",
                    ActiveTime = DateTime.Now,
                    Remark1="1",
                    Remark2="2",
                    Remark3="3",
                    Remark4="4",
                    Remark5="5",
                } ,
                new() {
                    Name = "testDevice2",
                    ActiveTime = DateTime.Now,
                    Remark1="1",
                    Remark2="2",
                    Remark3="3",
                    Remark4="4",
                    Remark5="5",
                }};

            script = businessProperty.BigTextScriptDeviceModel;
        }
        else
        {
            return;
        }


        var op = new DialogOption()
        {
            IsScrolling = false,
            Title = Localizer["Check"],
            ShowFooter = false,

            ShowCloseButton = false,
            Size = Size.ExtraLarge,
        };

        op.Component = BootstrapDynamicComponent.CreateComponent<ScriptCheck>(new Dictionary<string, object?>
        {
            {nameof(ScriptCheck.Data),data },
            {nameof(ScriptCheck.Script),script },
            {nameof(ScriptCheck.ScriptChanged),EventCallback.Factory.Create<string>(this, v =>
            {
                     if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel))
        {
                businessProperty.BigTextScriptAlarmModel=v;

        }
        else if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptVariableModel))
        {
               businessProperty.BigTextScriptVariableModel=v;


        }
        else if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel))
        {
            businessProperty.BigTextScriptDeviceModel=v;
        }

            }) },

        });
        await DialogService.Show(op);

    }

}

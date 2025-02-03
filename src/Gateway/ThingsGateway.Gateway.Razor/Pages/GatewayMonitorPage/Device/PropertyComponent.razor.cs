// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;
public partial class PropertyComponent : IPropertyUIBase
{
    [Parameter, EditorRequired]
    public string Id { get; set; }
    [Parameter, EditorRequired]
    public bool CanWrite { get; set; }
    [Parameter, EditorRequired]
    public ModelValueValidateForm Model { get; set; }
    [Parameter, EditorRequired]
    public IEnumerable<IEditorItem> PluginPropertyEditorItems { get; set; }

    private IStringLocalizer PropertyComponentLocalizer { get; set; }

    protected override Task OnParametersSetAsync()
    {
        if (Model?.Value != null)
            PropertyComponentLocalizer = App.CreateLocalizerByType(Model.Value.GetType());
        return base.OnParametersSetAsync();
    }

    [Inject]
    private IStringLocalizer<DeviceEditComponent> Localizer { get; set; }

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
            IsScrolling = true,
            Title = Localizer["Check"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraExtraLarge,
            FullScreenSize = FullScreenSize.None
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


    [Inject]
    private DialogService DialogService { get; set; }
}

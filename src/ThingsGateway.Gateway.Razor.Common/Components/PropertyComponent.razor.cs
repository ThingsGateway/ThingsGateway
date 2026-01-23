// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.Text;

using ThingsGateway.Foundation.Common.Json.Extension;

using TouchSocket.Core;

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

    public static async Task CheckScript(BusinessPropertyWithCacheIntervalScript businessProperty, string pname, string title, object receiver, DialogService dialogService)
    {
        string script = null;
        if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel))
        {
            script = businessProperty.BigTextScriptAlarmModel;
        }
        else if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptVariableModel))
        {
            script = businessProperty.BigTextScriptVariableModel;
        }
        else if (pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel))
        {
            script = businessProperty.BigTextScriptDeviceModel;
        }
        else
        {
            return;
        }

        var op = new DialogOption()
        {
            IsScrolling = true,
            Title = title,
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraExtraLarge,
            FullScreenSize = FullScreenSize.None
        };

        op.Component = BootstrapDynamicComponent.CreateComponent<ScriptCheck>(new Dictionary<string, object?>
    {
        {nameof(ScriptCheck.Script),script },
        {nameof(ScriptCheck.GetResult), (string input,string script)=>
        {
                StringBuilder stringBuilder=new();
               var logger=new EasyLogger((a)=>stringBuilder.AppendLine(a));


                var type=  script == businessProperty.BigTextScriptDeviceModel?typeof(List<DeviceBasicData>):script ==businessProperty.BigTextScriptAlarmModel?typeof(List<AlarmVariable>):typeof(List<VariableBasicData>);

                var  data = (IEnumerable<object>)Newtonsoft.Json.JsonConvert.DeserializeObject(input, type);
                var value = data.GetDynamicModel(script,logger,TimeSpan.FromHours(1));
                stringBuilder.AppendLine(value.ToSystemTextJsonString());
              return Task.FromResult( stringBuilder.ToString());
        }},

        {nameof(ScriptCheck.OnGetDemo),()=>
                {
                    return
                    pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptDeviceModel)?
                    """
                    using ThingsGateway.Foundation;
                    
                    using System.Dynamic;
                    using TouchSocket.Core;
                    public class S1 : IDynamicModel
                    {
                        public IEnumerable<dynamic> GetList(IEnumerable<object> datas)
                        {
                            if(datas==null) return null;
                            List<ExpandoObject> deviceObjs = new List<ExpandoObject>();
                            foreach (var v in datas)
                            {
                                var device = (DeviceBasicData)v;
                                var expando = new ExpandoObject();
                                var deviceObj = new ExpandoObject();

                                deviceObj.TryAdd(nameof(Device.Description), device.Description);
                                deviceObj.TryAdd(nameof(DeviceBasicData.ActiveTime), device.ActiveTime);
                                deviceObj.TryAdd(nameof(DeviceBasicData.DeviceStatus), device.DeviceStatus.ToString());
                                deviceObj.TryAdd(nameof(DeviceBasicData.LastErrorMessage), device.LastErrorMessage);
                                deviceObj.TryAdd(nameof(DeviceBasicData.PluginName), device.PluginName);
                                deviceObj.TryAdd(nameof(DeviceBasicData.Remark1), device.Remark1);
                                deviceObj.TryAdd(nameof(DeviceBasicData.Remark2), device.Remark2);
                                deviceObj.TryAdd(nameof(DeviceBasicData.Remark3), device.Remark3);
                                deviceObj.TryAdd(nameof(DeviceBasicData.Remark4), device.Remark4);
                                deviceObj.TryAdd(nameof(DeviceBasicData.Remark5), device.Remark5);


                                expando.TryAdd(nameof(Device.Name), deviceObj);

                            }
                            return deviceObjs;
                        }
                    }
                    
                    """
                    :

                    pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptVariableModel)?

                    """
                    using System.Dynamic;
                    using ThingsGateway.Foundation;
                    using TouchSocket.Core;
                    public class S2 : IDynamicModel
                    {
                        public IEnumerable<dynamic> GetList(IEnumerable<object> datas)
                        {
                            if(datas==null) return null;
                            List<ExpandoObject> deviceObjs = new List<ExpandoObject>();
                            //按设备名称分组
                            var groups = datas.Where(a => !string.IsNullOrEmpty(((VariableBasicData)a).DeviceName)).GroupBy(a => ((VariableBasicData)a).DeviceName, a => ((VariableBasicData)a));
                            foreach (var group in groups)
                            {
                                //按采集时间分组
                                var data = group.GroupBy(a => a.CollectTime.DateTimeToUnixTimestamp());
                                var deviceObj = new ExpandoObject();
                                List<ExpandoObject> expandos = new List<ExpandoObject>();
                                foreach (var item in data)
                                {
                                    var expando = new ExpandoObject();
                                    expando.TryAdd("ts", item.Key);
                                    var variableObj = new ExpandoObject();
                                    foreach (var tag in item)
                                    {
                                        variableObj.TryAdd(tag.Name, tag.Value);
                                    }
                                    expando.TryAdd("values", variableObj);

                                    expandos.Add(expando);
                                }
                                deviceObj.TryAdd(group.Key, expandos);
                                deviceObjs.Add(deviceObj);
                            }

                            return deviceObjs;
                        }
                    }

                    """
                    :

                    pname == nameof(BusinessPropertyWithCacheIntervalScript.BigTextScriptAlarmModel)?

                    """
                    using System.Dynamic;
                    using ThingsGateway.Foundation;
                                        
                    using TouchSocket.Core;
                    public class DeviceScript : IDynamicModel
                    {
                        public IEnumerable<dynamic> GetList(IEnumerable<object> datas)
                        {
                            if(datas==null) return null;
                            List<ExpandoObject> deviceObjs = new List<ExpandoObject>();
                            //按设备名称分组
                            var groups = datas.Where(a => !string.IsNullOrEmpty(((AlarmVariable)a).DeviceName)).GroupBy(a => ((AlarmVariable)a).DeviceName, a => ((AlarmVariable)a));
                            foreach (var group in groups)
                            {
                                //按采集时间分组
                                var data = group.GroupBy(a => a.AlarmTime.DateTimeToUnixTimestamp());
                                var deviceObj = new ExpandoObject();
                                List<ExpandoObject> expandos = new List<ExpandoObject>();
                                foreach (var item in data)
                                {
                                    var expando = new ExpandoObject();
                                    expando.TryAdd("ts", item.Key);
                                    var variableObj = new ExpandoObject();
                                    foreach (var tag in item)
                                    {
                                        var alarmObj = new ExpandoObject();
                                        alarmObj.TryAdd(nameof(tag.AlarmCode), tag.AlarmCode);
                                        alarmObj.TryAdd(nameof(tag.AlarmText), tag.AlarmText);
                                        alarmObj.TryAdd(nameof(tag.AlarmType), tag.AlarmType);
                                        alarmObj.TryAdd(nameof(tag.AlarmLimit), tag.AlarmLimit);
                                        alarmObj.TryAdd(nameof(tag.EventTime), tag.EventTime);
                                        alarmObj.TryAdd(nameof(tag.EventType), tag.EventType);

                                        variableObj.TryAdd(tag.Name, alarmObj);
                                    }
                                    expando.TryAdd("alarms", variableObj);

                                    expandos.Add(expando);
                                }
                                deviceObj.TryAdd(group.Key, expandos);
                                deviceObjs.Add(deviceObj);
                            }

                            return deviceObjs;
                        }
                    }
                    """
                    :
                    ""
                    ;
                }
            },
        {nameof(ScriptCheck.ScriptChanged),EventCallback.Factory.Create<string>(receiver, v =>
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
        await dialogService.Show(op);
    }

    [Inject]
    private DialogService DialogService { get; set; }
}

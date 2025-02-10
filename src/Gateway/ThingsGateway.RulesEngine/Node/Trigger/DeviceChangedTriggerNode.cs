
using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Trigger", ImgUrl = "_content/ThingsGateway.RulesEngine/img/ValueChanged.svg", Desc = nameof(DeviceChangedTriggerNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(TextWidget))]
public class DeviceChangedTriggerNode : TextNode, ITriggerNode, IDisposable
{
    public DeviceChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "DeviceChangedTriggerNode"; Placeholder = "DeviceChangedTriggerNode.Placeholder"; }


    private Func<NodeOutput, Task> Func { get; set; }
    Task ITriggerNode.StartAsync(Func<NodeOutput, Task> func)
    {
        Func = func;
        FuncDict.Add(this, func);
        if (!DeviceChangedTriggerNodeDict.TryGetValue(Text, out var list))
        {
            var list1 = new ConcurrentList<DeviceChangedTriggerNode>();
            list1.Add(this);
            DeviceChangedTriggerNodeDict.Add(Text, list1);
        }
        else
        {
            list.Add(this);
        }
        return Task.CompletedTask;
    }
    public static Dictionary<string, ConcurrentList<DeviceChangedTriggerNode>> DeviceChangedTriggerNodeDict = new();
    public static Dictionary<DeviceChangedTriggerNode, Func<NodeOutput, Task>> FuncDict = new();

    public static BlockingCollection<DeviceData> DeviceDatas = new();
    static DeviceChangedTriggerNode()
    {
        _ = RunAsync();
        GlobalData.DeviceStatusChangeEvent += GlobalData_DeviceStatusChangeEvent;
    }
    private static void GlobalData_DeviceStatusChangeEvent(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {
        if (DeviceChangedTriggerNodeDict.TryGetValue(deviceData.Name, out var list) && list?.Count > 0)
        {
            if (!DeviceDatas.IsAddingCompleted)
            {
                try
                {
                    DeviceDatas.Add(deviceData);
                    return;
                }
                catch (InvalidOperationException) { }
                catch { }
            }
        }
    }
    static Task RunAsync()
    {
        return DeviceDatas.GetConsumingEnumerable().ParallelForEachAsync((async (deviceDatas, token) =>

            {

                if (DeviceChangedTriggerNodeDict.TryGetValue(deviceDatas.Name, out var valueChangedTriggerNodes))
                {
                    foreach (var item in valueChangedTriggerNodes)
                    {
                        try
                        {
                            if (FuncDict.TryGetValue(item, out var func))
                            {
                                item.LogMessage?.Trace($"Device changed: {item.Text}");
                                await func.Invoke(new NodeOutput() { Value = deviceDatas }).ConfigureAwait(false);

                            }
                        }
                        catch (Exception ex)
                        {
                            item.LogMessage?.LogWarning(ex);
                        }
                    }
                }
            }), Environment.ProcessorCount / 2 <= 1 ? 2 : Environment.ProcessorCount / 2, default);
    }


    public void Dispose()
    {
        FuncDict.Remove(this);
        if (DeviceChangedTriggerNodeDict.TryGetValue(Text, out var list))
        {
            list.Remove(this);
        }
    }
}



//using ThingsGateway.Gateway.Application;

//using TouchSocket.Core;

//namespace ThingsGateway.RulesEngine;

//[CategoryNode(Category = "Trigger", ImgUrl = "_content/ThingsGateway.RulesEngine/img/ValueChanged.svg", Desc = nameof(DeviceChangedTriggerNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(TextWidget))]
//public class DeviceChangedTriggerNode : TextNode, ITriggerNode, IDisposable
//{
//    public DeviceChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "DeviceChangedTriggerNode"; Placeholder = "DeviceChangedTriggerNode.Placeholder"; }


//    private Func<NodeOutput, Task> Func { get; set; }
//    Task ITriggerNode.StartAsync(Func<NodeOutput, Task> func)
//    {
//        Func = func;
//        GlobalData.DeviceStatusChangeEvent += GlobalData_DeviceStatusChangeEvent;
//        return Task.CompletedTask;
//    }

//    private void GlobalData_DeviceStatusChangeEvent(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
//    {
//        if (deviceRunTime.Name == Text)
//        {
//            LogMessage?.Trace($"Device changed: {Text}");
//            _ = Func?.Invoke(new NodeOutput() { Value = deviceRunTime });
//        }
//    }


//    public void Dispose()
//    {
//        GlobalData.DeviceStatusChangeEvent -= GlobalData_DeviceStatusChangeEvent;
//    }
//}



using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Trigger", ImgUrl = "_content/ThingsGateway.RulesEngine/img/ValueChanged.svg", Desc = nameof(AlarmChangedTriggerNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(TextWidget))]
public class AlarmChangedTriggerNode : TextNode, ITriggerNode, IDisposable
{
    public AlarmChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "AlarmChangedTriggerNode"; Placeholder = "AlarmChangedTriggerNode.Placeholder"; }


    private Func<NodeOutput, Task> Func { get; set; }
    Task ITriggerNode.StartAsync(Func<NodeOutput, Task> func)
    {
        Func = func;
        FuncDict.Add(this, func);
        if (!AlarmChangedTriggerNodeDict.TryGetValue(Text, out var list))
        {
            var list1 = new ConcurrentList<AlarmChangedTriggerNode>();
            list1.Add(this);
            AlarmChangedTriggerNodeDict.Add(Text, list1);
        }
        else
        {
            list.Add(this);
        }
        return Task.CompletedTask;
    }
    public static Dictionary<string, ConcurrentList<AlarmChangedTriggerNode>> AlarmChangedTriggerNodeDict = new();
    public static Dictionary<AlarmChangedTriggerNode, Func<NodeOutput, Task>> FuncDict = new();

    public static BlockingCollection<AlarmVariable> AlarmVariables = new();
    static AlarmChangedTriggerNode()
    {
        _ = RunAsync();
        GlobalData.AlarmChangedEvent += AlarmHostedService_OnAlarmChanged;
    }
    private static void AlarmHostedService_OnAlarmChanged(AlarmVariable alarmVariable)
    {
        if (AlarmChangedTriggerNodeDict.TryGetValue(alarmVariable.Name, out var list) && list?.Count > 0)
        {
            if (!AlarmVariables.IsAddingCompleted)
            {
                try
                {
                    AlarmVariables.Add(alarmVariable);
                    return;
                }
                catch (InvalidOperationException) { }
                catch { }
            }
        }
    }
    static Task RunAsync()
    {
        return AlarmVariables.GetConsumingEnumerable().ParallelForEachAsync((async (alarmVariables, token) =>
            {

                if (AlarmChangedTriggerNodeDict.TryGetValue(alarmVariables.Name, out var valueChangedTriggerNodes))
                {
                    foreach (var item in valueChangedTriggerNodes)
                    {
                        try
                        {
                            if (FuncDict.TryGetValue(item, out var func))
                            {
                                item.LogMessage?.Trace($"Alarm changed: {item.Text}");
                                await func.Invoke(new NodeOutput() { Value = alarmVariables }).ConfigureAwait(false);

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
        if (AlarmChangedTriggerNodeDict.TryGetValue(Text, out var list))
        {
            list.Remove(this);
        }
    }
}


//using ThingsGateway.Gateway.Application;

//using TouchSocket.Core;

//namespace ThingsGateway.RulesEngine;

//[CategoryNode(Category = "Trigger", ImgUrl = "_content/ThingsGateway.RulesEngine/img/ValueChanged.svg", Desc = nameof(AlarmChangedTriggerNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(TextWidget))]
//public class AlarmChangedTriggerNode : TextNode, ITriggerNode, IDisposable
//{
//    public AlarmChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "AlarmChangedTriggerNode"; Placeholder = "AlarmChangedTriggerNode.Placeholder"; }

//    private Func<NodeOutput, Task> Func { get; set; }
//    Task ITriggerNode.StartAsync(Func<NodeOutput, Task> func)
//    {
//        Func = func;
//        GlobalData.AlarmHostedService.OnAlarmChanged += AlarmHostedService_OnAlarmChanged;
//        return Task.CompletedTask;
//    }

//    private void AlarmHostedService_OnAlarmChanged(AlarmVariable alarmVariable)
//    {
//        if (alarmVariable.Name == Text)
//        {
//            LogMessage?.Trace($"Alarm changed: {Text}");
//            _ = Func?.Invoke(new NodeOutput() { Value = alarmVariable });
//        }
//    }

//    public void Dispose()
//    {
//        GlobalData.AlarmHostedService.OnAlarmChanged -= AlarmHostedService_OnAlarmChanged;
//    }
//}

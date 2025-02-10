using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Trigger", ImgUrl = "_content/ThingsGateway.RulesEngine/img/ValueChanged.svg", Desc = nameof(ValueChangedTriggerNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(TextWidget))]
public class ValueChangedTriggerNode : TextNode, ITriggerNode, IDisposable
{
    public ValueChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "ValueChangedTriggerNode"; Placeholder = "ValueChangedTriggerNode.Placeholder"; }

    private Func<NodeOutput, Task> Func { get; set; }
    Task ITriggerNode.StartAsync(Func<NodeOutput, Task> func)
    {
        Func = func;
        FuncDict.Add(this, func);
        if (!ValueChangedTriggerNodeDict.TryGetValue(Text, out var list))
        {
            var list1 = new ConcurrentList<ValueChangedTriggerNode>();
            list1.Add(this);
            ValueChangedTriggerNodeDict.Add(Text, list1);
        }
        else
        {
            list.Add(this);
        }
        return Task.CompletedTask;
    }
    public static Dictionary<string, ConcurrentList<ValueChangedTriggerNode>> ValueChangedTriggerNodeDict = new();
    public static Dictionary<ValueChangedTriggerNode, Func<NodeOutput, Task>> FuncDict = new();

    public static BlockingCollection<VariableBasicData> VariableBasicDatas = new();
    static ValueChangedTriggerNode()
    {
        _ = RunAsync();
        GlobalData.VariableValueChangeEvent += GlobalData_VariableValueChangeEvent;
    }
    private static void GlobalData_VariableValueChangeEvent(VariableRuntime variableRuntime, VariableBasicData variableData)
    {
        if (ValueChangedTriggerNodeDict.TryGetValue(variableData.Name, out var list) && list?.Count > 0)
        {
            if (!VariableBasicDatas.IsAddingCompleted)
            {
                try
                {
                    VariableBasicDatas.Add(variableData);
                    return;
                }
                catch (InvalidOperationException) { }
                catch { }
            }
        }
    }
    static Task RunAsync()
    {
        return VariableBasicDatas.GetConsumingEnumerable().ParallelForEachAsync((async (variableBasicData, token) =>
        {

            if (ValueChangedTriggerNodeDict.TryGetValue(variableBasicData.Name, out var valueChangedTriggerNodes))
            {
                foreach (var item in valueChangedTriggerNodes)
                {
                    try
                    {
                        if (FuncDict.TryGetValue(item, out var func))
                        {
                            item.LogMessage?.Trace($"Variable changed: {item.Text}");
                            await func.Invoke(new NodeOutput() { Value = variableBasicData }).ConfigureAwait(false);

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
        if (ValueChangedTriggerNodeDict.TryGetValue(Text, out var list))
        {
            list.Remove(this);
        }
    }
}
//using ThingsGateway.Gateway.Application;

//using TouchSocket.Core;

//namespace ThingsGateway.RulesEngine;

//[CategoryNode(Category = "Trigger", ImgUrl = "_content/ThingsGateway.RulesEngine/img/ValueChanged.svg", Desc = nameof(ValueChangedTriggerNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(TextWidget))]
//public class ValueChangedTriggerNode : TextNode, ITriggerNode, IDisposable
//{
//    public ValueChangedTriggerNode(string id, Point? position = null) : base(id, position) { Title = "ValueChangedTriggerNode"; Placeholder = "ValueChangedTriggerNode.Placeholder"; }

//    private Func<NodeOutput, Task> Func { get; set; }
//    Task ITriggerNode.StartAsync(Func<NodeOutput, Task> func)
//    {
//        Func = func;
//        GlobalData.VariableValueChangeEvent += GlobalData_VariableValueChangeEvent; ;
//        return Task.CompletedTask;
//    }

//    private void GlobalData_VariableValueChangeEvent(VariableRuntime variableRuntime, VariableBasicData variableData)
//    {
//        if (variableRuntime.Name == Text)
//        {
//            LogMessage?.Trace($"Variable changed: {Text}");
//            _ = Func?.Invoke(new NodeOutput() { Value = variableRuntime });
//        }
//    }


//    public void Dispose()
//    {
//        GlobalData.VariableValueChangeEvent -= GlobalData_VariableValueChangeEvent; ;
//    }
//}

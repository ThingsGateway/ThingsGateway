using ThingsGateway.Foundation;
using ThingsGateway.NewLife;

using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

[CategoryNode(Category = "Trigger", ImgUrl = "_content/ThingsGateway.RulesEngine/img/TimeInterval.svg", Desc = nameof(TimeIntervalTriggerNode), LocalizerType = typeof(ThingsGateway.RulesEngine._Imports), WidgetType = typeof(TextWidget))]
public class TimeIntervalTriggerNode : TextNode, ITriggerNode, IDisposable
{
    public TimeIntervalTriggerNode(string id, Point? position = null) : base(id, position) { Title = "TimeIntervalTriggerNode"; Placeholder = "TimeIntervalTriggerNode.Placeholder"; }

    private TimeTick TimeTick;
    private Func<NodeOutput, Task> Func { get; set; }
    private bool Disposed;
    Task ITriggerNode.StartAsync(Func<NodeOutput, Task> func)
    {
        Func = func;
        if (int.TryParse(Text, out int delay))
        {
            if (delay <= 500)
                Text = "500";
        }
        TimeTick = new TimeTick(Text);
        _ = Timer();
        return Task.CompletedTask;
    }

    private async Task Timer()
    {
        while (!Disposed)
        {
            try
            {
                if (TimeTick.IsTickHappen())
                {
                    if (Func != null)
                    {
                        LogMessage?.Trace($"Timer: {Text}");
                        await Func.Invoke(new NodeOutput() { Value = TimeTick.LastTime }).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex);
            }
            finally
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
    }


    public void Dispose()
    {
        Disposed = true;
    }
}

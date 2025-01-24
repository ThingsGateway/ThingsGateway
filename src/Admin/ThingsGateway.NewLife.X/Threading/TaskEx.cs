namespace ThingsGateway.NewLife.Extension;

#if NET452
/// <summary>任务扩展</summary>
public static class TaskEx
{
    private static readonly Task s_preCompletedTask = Task.FromResult(false);
    /// <summary>已完成任务</summary>
    public static Task CompletedTask => s_preCompletedTask;
}
#endif

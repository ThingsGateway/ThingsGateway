using ThingsGateway.NewLife.Threading;

namespace ThingsGateway.NewLife;

/// <summary>
/// 最小时间间隔 10 毫秒
/// </summary>
public class TimeTick
{
    /// <summary>
    /// 时间间隔（毫秒）
    /// </summary>
    private int _intervalMilliseconds = 1000;

    private readonly Cron? cron;

    /// <inheritdoc cref="TimeTick"/>
    public TimeTick(string delay)
    {
        // 尝试解析延迟时间
        if (int.TryParse(delay, out int intervalMilliseconds))
        {
            _intervalMilliseconds = intervalMilliseconds < 10 ? 10 : intervalMilliseconds;
            LastTime = DateTime.UtcNow.AddMilliseconds(-_intervalMilliseconds); // 初始化上次时间
        }
        else
        {
            cron = new Cron(delay); // 解析 Cron 表达式
        }
    }

    /// <summary>
    /// 上次触发时间
    /// </summary>
    public DateTime LastTime { get; private set; } = DateTime.Now;

    /// <summary>
    /// 是否触发时间刻度
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <returns>是否触发时间刻度</returns>
    public bool IsTickHappen(DateTime currentTime)
    {
        // 在没有 Cron 表达式的情况下，使用固定间隔
        if (cron == null)
        {
            var nextTime = LastTime.AddMilliseconds(_intervalMilliseconds);
            var diffMilliseconds = (currentTime - nextTime).TotalMilliseconds;

            var result = diffMilliseconds >= 0;
            if (result)
            {
                if (diffMilliseconds > _intervalMilliseconds)
                    LastTime = currentTime;
                else
                    LastTime = nextTime;
            }
            return result;
        }
        // 使用 Cron 表达式
        else
        {
            var nextTime = cron.GetNext(LastTime);
            if (currentTime >= nextTime)
            {
                LastTime = nextTime;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 是否到达设置的时间间隔
    /// </summary>
    /// <returns>是否到达设置的时间间隔</returns>
    public bool IsTickHappen() => IsTickHappen(DateTime.UtcNow);
}

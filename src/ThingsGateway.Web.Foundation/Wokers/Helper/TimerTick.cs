namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 计时器
/// </summary>
public class TimerTick
{
    /// <summary>
    /// 时间差
    /// </summary>
    private int milliSeconds = 1000;

    /// <inheritdoc cref="TimerTick"/>
    public TimerTick(int milliSeconds = 1000)
    {
        if (milliSeconds < 20)
            milliSeconds = 20;
        LastTime = DateTime.UtcNow.AddMilliseconds(-milliSeconds);
        this.milliSeconds = milliSeconds;
    }

    /// <summary>
    /// 上次操作时间
    /// </summary>
    public DateTime LastTime { get; private set; }

    /// <summary>
    /// 是否触发时间刻度
    /// </summary>
    /// <returns></returns>
    public bool IsTickHappen(DateTime currentTime)
    {
        DateTime dateTime = LastTime.AddMilliseconds(milliSeconds);
        if (currentTime < dateTime)
            return false;
        LastTime = dateTime;
        return true;
    }

    /// <summary>
    /// 是否到达设置时间
    /// </summary>
    /// <returns></returns>
    public bool IsTickHappen() => IsTickHappen(DateTime.UtcNow);

}

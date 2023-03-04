namespace ThingsGateway.Web.Foundation;
public class TimerTick
{

    /// <summary>
    /// 误差
    /// </summary>
    private int offsetTime = 60;
    /// <summary>
    /// 时间差
    /// </summary>
    private int milliSeconds = 1000;

    public TimerTick(int milliSeconds = 1000)
    {
        if (milliSeconds < 20)
            milliSeconds = 20;
        LastTime = GetExactTime(DateTime.Now.AddMilliseconds(-milliSeconds));
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
        LastTime = (currentTime - dateTime).TotalMilliseconds <= offsetTime ?
            dateTime : GetExactTime(currentTime);
        return true;
    }

    public bool IsTickHappen() => IsTickHappen(DateTime.Now);

    /// <summary>
    /// 此时实际时间已经大于计算时间差，需获取准确的时间，趋近整秒
    /// </summary>
    /// <param name="dateTime">当前时间</param>
    /// <returns></returns>
    private DateTime GetExactTime(DateTime dateTime)
    {
        if (milliSeconds % 1000 == 0)
        {
            //少于300毫秒的直接返回整秒时间
            if (dateTime.Millisecond < 300)
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);

            dateTime.AddSeconds(1.0);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }
        return dateTime;
    }

}

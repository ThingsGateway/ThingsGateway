namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 连读报文信息
/// </summary>
public class DeviceVariableSourceRead
{
    /// <summary>
    /// 间隔时间实现
    /// </summary>
    private TimerTick exTimerTick;
    /// <summary>
    /// 传入连读间隔
    /// </summary>
    /// <param name="milliSeconds"></param>
    public DeviceVariableSourceRead(int milliSeconds = 1000)
    {
        exTimerTick = new TimerTick(milliSeconds);
    }
    /// <summary>
    /// 读取地址，传入时需要去除额外信息
    /// </summary>
    public string Address { get; set; }
    /// <summary>
    /// 读取长度
    /// </summary>
    public string Length { get; set; }
    /// <summary>
    /// 需分配的变量列表
    /// </summary>
    public List<CollectVariableRunTime> DeviceVariables { get; set; } = new();
    /// <summary>
    /// 检测是否达到读取间隔
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool CheckIfRequestAndUpdateTime(DateTime time) => exTimerTick.IsTickHappen(time);

}

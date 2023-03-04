using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 特殊方法变量信息
/// </summary>
public class DeviceVariableMedRead
{
    /// <summary>
    /// 间隔时间实现
    /// </summary>
    private TimerTick exTimerTick;
    /// <summary>
    /// 传入连读间隔
    /// </summary>
    /// <param name="milliSeconds"></param>
    public DeviceVariableMedRead(int milliSeconds = 1000)
    {
        exTimerTick = new TimerTick(milliSeconds);
        Converter = new TouchSocket.Core.StringConverter();
        Converter.Add(new StringToEncodingConverter());
    }
    /// <summary>
    /// 字符串转换器，默认支持基础类型和Json。可以自定义。
    /// </summary>
    public TouchSocket.Core.StringConverter Converter { get; }
    /// <summary>
    /// 方法参数
    /// </summary>
    public object[] MedObj { get; set; }
    /// <summary>
    /// 方法参数
    /// </summary>
    public string MedStr { get; set; }
    /// <summary>
    /// 方法
    /// </summary>
    public Method MedInfo { get; set; }
    /// <summary>
    /// 需分配的变量
    /// </summary>
    public CollectVariableRunTime DeviceVariable { get; set; } = new();
    /// <summary>
    /// 检测是否达到读取间隔
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool CheckIfRequestAndUpdateTime(DateTime time) => exTimerTick.IsTickHappen(time);

}

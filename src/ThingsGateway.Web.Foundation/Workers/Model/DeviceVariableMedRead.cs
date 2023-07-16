#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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
    /// 需分配的变量
    /// </summary>
    public DeviceVariableRunTime DeviceVariable { get; set; } = new();

    /// <summary>
    /// 方法
    /// </summary>
    public Method MedInfo { get; set; }

    /// <summary>
    /// 方法参数
    /// </summary>
    public object[] MedObj { get; set; }
    /// <summary>
    /// 地址参数，以;分割参数值
    /// </summary>
    public string MedStr { get; set; }
    /// <summary>
    /// 检测是否达到读取间隔
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool CheckIfRequestAndUpdateTime(DateTime time) => exTimerTick.IsTickHappen(time);

}

/// <summary>
/// 特殊方法变量信息，不参与轮询执行
/// </summary>
public class DeviceVariableMedSource
{
    /// <summary>
    /// 传入连读间隔
    /// </summary>
    public DeviceVariableMedSource()
    {
        Converter = new TouchSocket.Core.StringConverter();
        Converter.Add(new StringToEncodingConverter());
    }
    /// <summary>
    /// 字符串转换器，默认支持基础类型和Json。可以自定义。
    /// </summary>
    public TouchSocket.Core.StringConverter Converter { get; }
    /// <summary>
    /// 需分配的变量
    /// </summary>
    public DeviceVariableRunTime DeviceVariable { get; set; } = new();

    /// <summary>
    /// 方法
    /// </summary>
    public Method MedInfo { get; set; }

    /// <summary>
    /// 方法参数
    /// </summary>
    public object[] MedObj { get; set; }
    /// <summary>
    /// 地址参数
    /// </summary>
    public string MedStr { get; set; }

}

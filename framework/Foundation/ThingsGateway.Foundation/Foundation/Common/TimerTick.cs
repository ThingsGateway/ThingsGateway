#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation;

/// <summary>
/// TimerTick
/// </summary>
public class TimerTick
{
    /// <summary>
    /// 时间差
    /// </summary>
    private readonly int milliSeconds = 1000;

    /// <inheritdoc cref="TimerTick"/>
    public TimerTick(int milliSeconds = 1000)
    {
        if (milliSeconds < 10)
            milliSeconds = 10;
        LastTime = DateTimeExtensions.CurrentDateTime.AddMilliseconds(-milliSeconds);
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
        var dateTime = LastTime.AddMilliseconds(milliSeconds);
        if (currentTime < dateTime)
            return false;
        LastTime = dateTime;
        return true;
    }

    /// <summary>
    /// 是否到达设置时间
    /// </summary>
    /// <returns></returns>
    public bool IsTickHappen() => IsTickHappen(DateTimeExtensions.CurrentDateTime);

}

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// 时间刻度器,最小时间间隔10毫秒
/// </summary>
public class TimeTick
{
    /// <summary>
    /// 时间间隔（毫秒）
    /// </summary>
    private readonly int intervalMilliseconds = 1000;

    /// <inheritdoc cref="TimeTick"/>
    public TimeTick(int intervalMilliseconds = 1000)
    {
        if (intervalMilliseconds < 10)
            intervalMilliseconds = 10;
        LastTime = DateTime.Now.AddMilliseconds(-intervalMilliseconds);
        this.intervalMilliseconds = intervalMilliseconds;
    }

    /// <summary>
    /// 上次触发时间
    /// </summary>
    public DateTime LastTime { get; private set; }

    /// <summary>
    /// 是否触发时间刻度
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <returns>是否触发时间刻度</returns>
    public bool IsTickHappen(DateTime currentTime)
    {
        var nextTime = LastTime.AddMilliseconds(intervalMilliseconds);
        var diffMilliseconds = (currentTime - nextTime).TotalMilliseconds;
        if (diffMilliseconds < 0)
            return false;
        else if (diffMilliseconds > intervalMilliseconds)
            LastTime = currentTime; //选择当前时间
        else
            LastTime = nextTime;
        return true;
    }

    /// <summary>
    /// 是否到达设置的时间间隔
    /// </summary>
    /// <returns>是否到达设置的时间间隔</returns>
    public bool IsTickHappen() => IsTickHappen(DateTime.Now);
}

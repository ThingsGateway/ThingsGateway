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
/// 自增
/// </summary>
public sealed class EasyIncrementCount
{
    private long current = 0;
    private readonly EasyLock easyLock;
    private long max = long.MaxValue;
    private long start = 0;
    /// <inheritdoc cref="EasyIncrementCount"/>
    public EasyIncrementCount(long max, long start = 0, int tick = 1)
    {
        this.start = start;
        this.max = max;
        current = start;
        IncreaseTick = tick;
        easyLock = new EasyLock();
    }

    /// <summary>
    /// Tick
    /// </summary>
    public int IncreaseTick { get; set; } = 1;

    /// <summary>
    /// 获取当前的计数器的最大的设置值
    /// </summary>
    public long MaxValue => max;

    /// <summary>
    /// 获取自增信息，获得数据之后，下一次获取将会自增，如果自增后大于最大值，则会重置为最小值，如果小于最小值，则会重置为最大值。
    /// </summary>
    public long GetCurrentValue()
    {
        easyLock.Wait();
        long current = this.current;
        this.current += IncreaseTick;
        if (this.current > max)
        {
            this.current = start;
        }
        else if (this.current < start)
        {
            this.current = max;
        }

        easyLock.Release();
        return current;
    }

    /// <summary>
    /// 将当前的值重置为初始值。
    /// </summary>
    public void ResetCurrentValue()
    {
        easyLock.Wait();
        current = start;
        easyLock.Release();
    }

    /// <summary>
    /// 将当前的值重置为指定值
    /// </summary>
    public void ResetCurrentValue(long value)
    {
        easyLock.Wait();
        current = value <= max ? value >= start ? value : start : max;
        easyLock.Release();
    }

    /// <summary>
    /// 重置当前序号的最大值
    /// </summary>
    public void ResetMaxValue(long max)
    {
        easyLock.Wait();
        if (max > start)
        {
            if (max < current)
            {
                current = start;
            }

            this.max = max;
        }
        easyLock.Release();
    }

    /// <summary>
    /// 重置当前序号的初始值
    /// </summary>
    public void ResetStartValue(long start)
    {
        easyLock.Wait();
        if (start < max)
        {
            if (current < start)
            {
                current = start;
            }

            this.start = start;
        }
        easyLock.Release();
    }

}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.NewLife;

/// <summary>
/// 自增数据类，用于自增数据，可以设置最大值，初始值，自增步长等。
/// </summary>
public sealed class IncrementCount
{
    private long _current = 0;
    private long _max = long.MaxValue;
    private long _start = 0;

    /// <inheritdoc cref="IncrementCount"/>
    public IncrementCount(long max, long start = 0, int tick = 1)
    {
        _start = start;
        _max = max;
        _current = start;
        IncreaseTick = tick;
    }

    /// <summary>
    /// 自增步长
    /// </summary>
    public int IncreaseTick { get; set; } = 1;

    /// <summary>
    /// 获取当前的计数器的最大的设置值
    /// </summary>
    public long MaxValue => _max;

    /// <summary>
    /// 获取自增信息，获得数据之后，下一次获取将会自增，如果自增后大于最大值，则会重置为最小值，如果小于最小值，则会重置为最大值。
    /// </summary>
    public long GetCurrentValue()
    {
        lock (this)
        {
            long current = _current;
            _current += IncreaseTick;
            if (_current > _max)
            {
                _current = _start;
            }
            else if (_current < _start)
            {
                _current = _max;
            }

            return current;
        }
    }

    /// <summary>
    /// 将当前的值重置为初始值。
    /// </summary>
    public void ResetCurrentValue()
    {
        lock (this)
        {
            _current = _start;
        }
    }

    /// <summary>
    /// 将当前的值重置为指定值
    /// </summary>
    /// <param name="value">指定值</param>
    public void ResetCurrentValue(long value)
    {
        lock (this)
        {
            _current = value <= _max ? value >= _start ? value : _start : _max;

        }
    }

    /// <summary>
    /// 重置当前序号的最大值
    /// </summary>
    public void ResetMaxValue(long max)
    {
        lock (this)
        {
            if (max > _start)
            {
                if (max < _current)
                {
                    _current = _start;
                }

                _max = max;
            }
        }
    }

    /// <summary>
    /// 重置当前序号的初始值
    /// </summary>
    /// <param name="start">初始值</param>
    public void ResetStartValue(long start)
    {
        lock (this)
        {
            if (start < _max)
            {
                if (_current < start)
                {
                    _current = start;
                }

                _start = start;
            }
        }
    }


}

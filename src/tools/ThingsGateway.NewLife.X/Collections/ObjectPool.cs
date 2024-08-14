//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

using ThingsGateway.NewLife.X.Reflection;
using ThingsGateway.NewLife.X.Threading;

namespace ThingsGateway.NewLife.X.Collections;

/// <summary>资源池。支持空闲释放，主要用于数据库连接池和网络连接池</summary>
/// <remarks>
/// 文档 https://newlifex.com/core/object_pool
/// </remarks>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> : DisposeBase, IPool<T> where T : notnull
{
    #region 属性

    /// <summary>借出去的放在这</summary>
    private readonly ConcurrentDictionary<T, Item> _busy = new();

    /// <summary>基础空闲集合。只保存最小个数，最热部分</summary>
    private readonly ConcurrentStack<Item> _free = new();

    /// <summary>扩展空闲集合。保存最小个数以外部分</summary>
    private readonly ConcurrentQueue<Item> _free2 = new();

    private readonly Object SyncRoot = new();

    private Int32 _BusyCount;

    private Int32 _FreeCount;

    /// <summary>完全空闲清理时间。最小个数之下的资源超过空闲时间时被清理，默认0s永不清理</summary>
    public Int32 AllIdleTime { get; set; } = 0;

    /// <summary>繁忙个数</summary>
    public Int32 BusyCount => _BusyCount;

    /// <summary>空闲个数</summary>
    public Int32 FreeCount => _FreeCount;

    /// <summary>空闲清理时间。最小个数之上的资源超过空闲时间时被清理，默认10s</summary>
    public Int32 IdleTime { get; set; } = 10;

    /// <summary>最大个数。默认100，0表示无上限</summary>
    public Int32 Max { get; set; } = 100;

    /// <summary>最小个数。默认1</summary>
    public Int32 Min { get; set; } = 1;

    /// <summary>名称</summary>
    public String Name { get; set; }

    #endregion 属性

    #region 构造

    private volatile Boolean _inited;

    /// <summary>实例化一个资源池</summary>
    public ObjectPool()
    {
        var str = GetType().Name;
        if (str.Contains('`')) str = str.Substring(null, "`");
        if (str != "Pool")
            Name = str;
        else
            Name = $"Pool<{typeof(T).Name}>";
    }

    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _timer.TryDispose();

        Clear();
    }

    private void Init()
    {
        if (_inited) return;

        lock (SyncRoot)
        {
            if (_inited) return;
            _inited = true;
        }
    }

    #endregion 构造

    #region 内嵌

    private class Item
    {
        /// <summary>过期时间</summary>
        public DateTime LastTime { get; set; }

        /// <summary>数值</summary>
        public T? Value { get; set; }
    }

    #endregion 内嵌

    #region 主方法

    /// <summary>清空已有对象</summary>
    public virtual Int32 Clear()
    {
        var count = _FreeCount + _BusyCount;

        //_busy.Clear();
        //_BusyCount = 0;

        //_free.Clear();
        //while (_free2.TryDequeue(out var rs)) ;
        //_FreeCount = 0;

        while (_free.TryPop(out var pi)) OnDispose(pi.Value);
        while (_free2.TryDequeue(out var pi)) OnDispose(pi.Value);
        _FreeCount = 0;

        foreach (var item in _busy)
        {
            OnDispose(item.Key);
        }
        _busy.Clear();
        _BusyCount = 0;

        return count;
    }

    /// <summary>借出</summary>
    /// <returns></returns>
    public virtual T Get()
    {
        Interlocked.Increment(ref _Total);

        var success = false;
        Item? pi = null;
        do
        {
            // 从空闲集合借一个
            if (_free.TryPop(out pi) || _free2.TryDequeue(out pi))
            {
                Interlocked.Decrement(ref _FreeCount);

                success = true;
            }
            else
            {
                // 超出最大值后，抛出异常
                var count = BusyCount;
                if (Max > 0 && count >= Max)
                {
                    var msg = $"申请失败，已有 {count:n0} 达到或超过最大值 {Max:n0}";
                    throw new Exception(Name + " " + msg);
                }

                // 借不到，增加
                pi = new Item
                {
                    Value = OnCreate(),
                };

                if (count == 0) Init();

                Interlocked.Increment(ref _NewCount);
                success = false;
            }

            // 借出时如果不可用，再次借取
        } while (pi.Value == null || !OnGet(pi.Value));

        // 最后时间
        pi.LastTime = TimerX.Now;

        // 加入繁忙集合
        _busy.TryAdd(pi.Value, pi);

        Interlocked.Increment(ref _BusyCount);
        if (success) Interlocked.Increment(ref _Success);

        return pi.Value;
    }

    /// <summary>申请资源包装项，Dispose时自动归还到池中</summary>
    /// <returns></returns>
    public PoolItem<T> GetItem() => new(this, Get());

    /// <summary>归还</summary>
    /// <param name="value"></param>
    public virtual Boolean Put(T value)
    {
        if (value == null) return false;

        // 从繁忙队列找到并移除缓存项
        if (!_busy.TryRemove(value, out var pi))
        {
            Interlocked.Increment(ref _ReleaseCount);

            return false;
        }

        Interlocked.Decrement(ref _BusyCount);

        // 是否可用
        if (!OnPut(value))
        {
            Interlocked.Increment(ref _ReleaseCount);
            return false;
        }

        if (value is DisposeBase db && db.Disposed)
        {
            Interlocked.Increment(ref _ReleaseCount);
            return false;
        }

        var min = Min;

        // 如果空闲数不足最小值，则返回到基础空闲集合
        if (_FreeCount < min /*|| _free.Count < min*/)
            _free.Push(pi);
        else
            _free2.Enqueue(pi);

        // 最后时间
        pi.LastTime = TimerX.Now;

        Interlocked.Increment(ref _FreeCount);

        // 启动定期清理的定时器
        StartTimer();

        return true;
    }

    /// <summary>归还</summary>
    /// <param name="value"></param>
    public virtual Boolean Return(T value)
    {
        if (value == null) return false;

        // 从繁忙队列找到并移除缓存项
        if (!_busy.TryRemove(value, out var pi))
        {
            Interlocked.Increment(ref _ReleaseCount);

            return false;
        }

        Interlocked.Decrement(ref _BusyCount);

        // 是否可用
        if (!OnReturn(value))
        {
            Interlocked.Increment(ref _ReleaseCount);
            return false;
        }

        if (value is DisposeBase db && db.Disposed)
        {
            Interlocked.Increment(ref _ReleaseCount);
            return false;
        }

        var min = Min;

        // 如果空闲数不足最小值，则返回到基础空闲集合
        if (_FreeCount < min /*|| _free.Count < min*/)
            _free.Push(pi);
        else
            _free2.Enqueue(pi);

        // 最后时间
        pi.LastTime = TimerX.Now;

        Interlocked.Increment(ref _FreeCount);

        // 启动定期清理的定时器
        StartTimer();

        return true;
    }

    /// <summary>销毁</summary>
    /// <param name="value"></param>
    protected virtual void OnDispose(T? value) => value.TryDispose();

    /// <summary>借出时是否可用</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual Boolean OnGet(T value) => true;

    /// <summary>归还时是否可用</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual Boolean OnPut(T value) => true;

    /// <summary>归还时是否可用</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual Boolean OnReturn(T value) => true;

    #endregion 主方法

    #region 重载

    /// <summary>创建实例</summary>
    /// <returns></returns>
    protected virtual T? OnCreate() => (T?)typeof(T).CreateInstance();

    #endregion 重载

    #region 定期清理

    private TimerX? _timer;

    private void StartTimer()
    {
        if (_timer != null) return;
        lock (this)
        {
            if (_timer != null) return;

            _timer = new TimerX(Work, null, 5000, 5000) { Async = true };
        }
    }

    private void Work(Object? state)
    {
        //// 总数小于等于最小个数时不处理
        //if (FreeCount + BusyCount <= Min) return;

        // 遍历并干掉过期项
        var count = 0;

        // 清理过期不还。避免有借没还
        if (!_busy.IsEmpty)
        {
            var exp = TimerX.Now.AddSeconds(-AllIdleTime);
            foreach (var item in _busy)
            {
                if (item.Value.LastTime < exp)
                {
                    if (_busy.TryRemove(item.Key, out _))
                    {
                        // 业务层可能故意有借没还
                        //v.TryDispose();

                        Interlocked.Decrement(ref _BusyCount);
                    }
                }
            }
        }

        // 总数小于等于最小个数时不处理
        if (IdleTime > 0 && !_free2.IsEmpty && FreeCount + BusyCount > Min)
        {
            var exp = TimerX.Now.AddSeconds(-IdleTime);
            // 移除扩展空闲集合里面的超时项
            while (_free2.TryPeek(out var pi) && pi.LastTime < exp)
            {
                // 取出来销毁
                if (_free2.TryDequeue(out pi))
                {
                    pi.Value.TryDispose();

                    count++;
                    Interlocked.Decrement(ref _FreeCount);
                }
            }
        }

        if (AllIdleTime > 0 && !_free.IsEmpty)
        {
            var exp = TimerX.Now.AddSeconds(-AllIdleTime);
            // 移除基础空闲集合里面的超时项
            while (_free.TryPeek(out var pi) && pi.LastTime < exp)
            {
                // 取出来销毁
                if (_free.TryPop(out pi))
                {
                    pi.Value.TryDispose();

                    count++;
                    Interlocked.Decrement(ref _FreeCount);
                }
            }
        }

        var ncount = _NewCount;
        var fcount = _ReleaseCount;
        if (count > 0 || ncount > 0 || fcount > 0)
        {
            Interlocked.Add(ref _NewCount, -ncount);
            Interlocked.Add(ref _ReleaseCount, -fcount);

            var p = Total == 0 ? 0 : (Double)Success / Total;
        }
    }

    #endregion 定期清理

    #region 统计

    /// <summary>新创建数</summary>
    private Int32 _NewCount;

    /// <summary>释放数</summary>
    private Int32 _ReleaseCount;

    private Int32 _Success;
    private Int32 _Total;

    /// <summary>成功数</summary>
    public Int32 Success => _Success;

    /// <summary>总请求数</summary>
    public Int32 Total => _Total;

    #endregion 统计
}

/// <summary>资源池包装项，自动归还资源到池中</summary>
/// <typeparam name="T"></typeparam>
public class PoolItem<T> : DisposeBase
{
    #region 属性

    /// <summary>池</summary>
    public IPool<T> Pool { get; }

    /// <summary>数值</summary>
    public T Value { get; }

    #endregion 属性

    #region 构造

    /// <summary>包装项</summary>
    /// <param name="pool"></param>
    /// <param name="value"></param>
    public PoolItem(IPool<T> pool, T value)
    {
        Pool = pool;
        Value = value;
    }

    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Pool.Put(Value);
    }

    #endregion 构造
}

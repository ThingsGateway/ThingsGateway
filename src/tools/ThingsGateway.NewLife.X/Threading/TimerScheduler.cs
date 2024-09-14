//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Diagnostics;

using ThingsGateway.NewLife.X.Reflection;

#nullable enable

namespace ThingsGateway.NewLife.X.Threading;

/// <summary>定时器调度器</summary>
public class TimerScheduler
{
    #region 静态

    private static readonly Dictionary<String, TimerScheduler> _cache = new();

    [ThreadStatic]
    private static TimerScheduler? _Current;

    private TimerScheduler(String name) => Name = name;

    /// <summary>当前调度器</summary>
    public static TimerScheduler? Current { get => _Current; private set => _Current = value; }

    /// <summary>默认调度器</summary>
    public static TimerScheduler Default { get; } = Create("Default");

    /// <summary>创建指定名称的调度器</summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static TimerScheduler Create(String name)
    {
        if (_cache.TryGetValue(name, out var ts)) return ts;
        lock (_cache)
        {
            if (_cache.TryGetValue(name, out ts)) return ts;

            ts = new TimerScheduler(name);
            _cache[name] = ts;

            return ts;
        }
    }

    #endregion 静态

    #region 属性

    private Int32 _tid;

    private Thread? thread;

    private TimerX[] Timers = [];

    /// <summary>定时器个数</summary>
    public Int32 Count { get; private set; }

    /// <summary>最大耗时。超过时报警告日志，默认500ms</summary>
    public Int32 MaxCost { get; set; } = 500;

    /// <summary>名称</summary>
    public String Name { get; private set; }

    #endregion 属性

    private Int32 _period = 10;

    private AutoResetEvent? _waitForTimer;

    /// <summary>把定时器加入队列</summary>
    /// <param name="timer"></param>
    public void Add(TimerX timer)
    {
        if (timer == null) throw new ArgumentNullException(nameof(timer));

        timer.Id = Interlocked.Increment(ref _tid);

        lock (this)
        {
            var list = new List<TimerX>(Timers);
            if (list.Contains(timer)) return;
            list.Add(timer);

            Timers = list.ToArray();

            Count++;

            if (thread == null)
            {
                thread = new Thread(Process)
                {
                    Name = Name == "Default" ? "T" : Name,
                    IsBackground = true
                };
                thread.Start();
            }

            Wake();
        }
    }

    /// <summary>从队列删除定时器</summary>
    /// <param name="timer"></param>
    /// <param name="reason"></param>
    public void Remove(TimerX timer, String reason)
    {
        if (timer == null || timer.Id == 0) return;

        lock (this)
        {
            timer.Id = 0;

            var list = new List<TimerX>(Timers);
            if (list.Contains(timer))
            {
                list.Remove(timer);
                Timers = list.ToArray();

                Count--;
            }
        }
    }

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString() => Name;

    /// <summary>唤醒处理</summary>
    public void Wake()
    {
        var e = _waitForTimer;
        if (e != null)
        {
            var swh = e.SafeWaitHandle;
            if (swh != null && !swh.IsClosed) e.Set();
        }
    }

    /// <summary>检查定时器是否到期</summary>
    /// <param name="timer"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    private Boolean CheckTime(TimerX timer, Int64 now)
    {
        // 删除过期的，为了避免占用过多CPU资源，TimerX禁止小于10ms的任务调度
        var p = timer.Period;
        if (p is < 10 and > 0)
        {
            // 周期0表示只执行一次
            timer.Dispose();
            return false;
        }

        var ts = timer.NextTick - now;
        if (ts > 0)
        {
            // 缩小间隔，便于快速调用
            if (ts < _period) _period = (Int32)ts;

            return false;
        }

        return true;
    }

    /// <summary>处理每一个定时器</summary>
    /// <param name="state"></param>
    private void Execute(Object? state)
    {
        if (state is not TimerX timer) return;

        TimerX.Current = timer;

        timer.hasSetNext = false;
        var sw = Stopwatch.StartNew();
        try
        {
            // 弱引用判断
            var target = timer.Target.Target;
            if (target == null && !timer.Method.IsStatic)
            {
                Remove(timer, "委托已不存在（GC回收委托所在对象）");
                timer.Dispose();
                return;
            }

            var func = timer.Method.As<TimerCallback>(target);
            func!(timer.State);
        }
        catch (ThreadAbortException) { throw; }
        catch (ThreadInterruptedException) { throw; }
        // 如果用户代码没有拦截错误，则这里拦截，避免出错了都不知道怎么回事
        catch
        {
        }
        finally
        {
            sw.Stop();
            OnExecuted(timer, (Int32)sw.ElapsedMilliseconds);
        }
    }

    /// <summary>处理每一个定时器</summary>
    /// <param name="state"></param>
    private async Task ExecuteAsync(Object? state)
    {
        if (state is not TimerX timer) return;

        TimerX.Current = timer;

        timer.hasSetNext = false;

        var sw = Stopwatch.StartNew();
        try
        {
            // 弱引用判断
            var target = timer.Target.Target;
            if (target == null && !timer.Method.IsStatic)
            {
                Remove(timer, "委托已不存在（GC回收委托所在对象）");
                timer.Dispose();
                return;
            }

            var func = timer.Method.As<Func<Object?, Task>>(target);
            await func!(timer.State).ConfigureAwait(false);
        }
        catch (ThreadAbortException) { throw; }
        catch (ThreadInterruptedException) { throw; }
        // 如果用户代码没有拦截错误，则这里拦截，避免出错了都不知道怎么回事
        catch
        {
        }
        finally
        {
            sw.Stop();

            OnExecuted(timer, (Int32)sw.ElapsedMilliseconds);
        }
    }

    private void OnExecuted(TimerX timer, Int32 ms)
    {
        timer.Cost = timer.Cost == 0 ? ms : (timer.Cost + ms) / 2;

        timer.Timers++;
        OnFinish(timer);

        timer.Calling = false;

        TimerX.Current = null;

        if (timer.Cost > 100)
            // 调度线程可能在等待，需要唤醒
            Wake();
    }

    private void OnFinish(TimerX timer)
    {
        // 如果内部设置了下一次时间，则不再递加周期
        var p = timer.SetAndGetNextTime();

        // 清理一次性定时器
        if (p <= 0)
        {
            Remove(timer, "Period<=0");
            timer.Dispose();
        }
        else if (p < _period)
            _period = p;
    }

    /// <summary>调度主程序</summary>
    /// <param name="state"></param>
    private void Process(Object? state)
    {
        Current = this;
        while (true)
        {
            // 准备好定时器列表
            var arr = Timers;

            // 如果没有任务，则销毁线程
            if (arr.Length == 0 && _period == 60_000)
            {
                var th = thread;
                thread = null;
                //th?.Abort();

                break;
            }

            try
            {
                var now = Runtime.TickCount64;

                // 设置一个较大的间隔，内部会根据处理情况调整该值为最合理值
                _period = 60_000;
                foreach (var timer in arr)
                {
                    if (!timer.Calling && CheckTime(timer, now))
                    {
                        //// 是否能够执行
                        //if (timer.CanExecute == null || timer.CanExecute())
                        //{
                        // 必须在主线程设置状态，否则可能异步线程还没来得及设置开始状态，主线程又开始了新的一轮调度
                        timer.Calling = true;
                        if (timer.IsAsyncTask)
                            Task.Factory.StartNew(ExecuteAsync, timer);
                        else if (!timer.Async)
                            Execute(timer);
                        else
                            //Task.Factory.StartNew(() => ProcessItem(timer));
                            // 不需要上下文流动，捕获所有异常
                            ThreadPool.UnsafeQueueUserWorkItem(s =>
                            {
                                try
                                {
                                    Execute(s);
                                }
                                catch
                                {
                                }
                            }, timer);
                        // 内部线程池，让异步任务有公平竞争CPU的机会
                        //ThreadPoolX.QueueUserWorkItem(Execute, timer);
                        //}
                        //// 即使不能执行，也要设置下一次的时间
                        //else
                        //{
                        //    OnFinish(timer);
                        //}
                    }
                }
            }
            catch (ThreadAbortException) { break; }
            catch (ThreadInterruptedException) { break; }
            catch { }

            _waitForTimer ??= new AutoResetEvent(false);
            if (_period > 0)
                _waitForTimer.WaitOne(_period, true);
        }
    }
}

#nullable restore

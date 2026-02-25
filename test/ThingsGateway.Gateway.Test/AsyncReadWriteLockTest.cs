using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Common.PooledAwait;
using ThingsGateway.Gateway.Application;
using TouchSocket.Core;
namespace ThingsGateway.Gateway.Test
{
    using Xunit;
    public class AsyncReadWriteLockTest
    {
        [Fact]
        public async Task Reader_Should_Not_Block_When_Writer_Releases_First()
        {
            {

#pragma warning disable CA2000 // 丢失范围之前释放对象
                var rwLock = new AsyncReadWriteLock(writeReadRatio: 3, writePriority: true);
#pragma warning restore CA2000 // 丢失范围之前释放对象
                using var cts = new CancellationTokenSource();
                var d = await rwLock.WriterLockAsync();
                List<Task> tasks = new();
                // 多读线程
                for (int i = 0; i < 2000; i++)
                {
                    await Task.Delay(2);
                    int idx = i;
                    var task = Task.Run(async () =>
                    {
                        var token = await rwLock.ReaderLockAsync(cts.Token).ConfigureAwait(false);
                        try
                        {
                        }
                        finally
                        {
                        }
                    });
                    tasks.Add(task);
                }
                await Task.Delay(10);
                d.Dispose();
                // 多写线程
                for (int i = 0; i < 20; i++)
                {
                    int idx = i;
                    _ = Task.Run(async () =>
                    {
                        using (await rwLock.WriterLockAsync().ConfigureAwait(false))
                        {
                            Console.WriteLine($"Writer {idx}");
                            await Task.Delay(1).ConfigureAwait(false); // 模拟写操作
                        }
                    });
                }

                // 运行 5 秒
                await Task.Delay(5000);
                await Task.WhenAll(tasks);
                Assert.False(rwLock.ReadWaited);
            }


#pragma warning disable CA2000 // 丢失范围之前释放对象
            var rwLock1 = new AsyncReadWriteLock1(writeReadRatio: 3, writePriority: true);
#pragma warning restore CA2000 // 丢失范围之前释放对象
            using var cts1 = new CancellationTokenSource();
            var d1 = await rwLock1.WriterLockAsync();
            List<Task> tasks1 = new();
            // 多读线程
            for (int i = 0; i < 2000; i++)
            {
                await Task.Delay(2);
                int idx = i;
                var task = Task.Run(async () =>
                {
                    var token = await rwLock1.ReaderLockAsync(cts1.Token).ConfigureAwait(false);
                    try
                    {
                    }
                    finally
                    {
                    }
                });
                tasks1.Add(task);
            }
            await Task.Delay(10);
            d1.Dispose();
            // 多写线程
            for (int i = 0; i < 20; i++)
            {
                int idx = i;
                _ = Task.Run(async () =>
                {
                    using (await rwLock1.WriterLockAsync().ConfigureAwait(false))
                    {
                        Console.WriteLine($"Writer {idx}");
                        await Task.Delay(1).ConfigureAwait(false); // 模拟写操作
                    }
                });
            }

            // 运行 5 秒
            await Task.Delay(5000);
            Assert.True(rwLock1.ReadWaited);
        }

    }
}

public class AsyncReadWriteLock1 : IAsyncDisposable
{
    private readonly int _writeReadRatio = 3; // 写3次会允许1次读，但写入也不会被阻止，具体协议取决于插件协议实现
    public AsyncReadWriteLock1(int writeReadRatio, bool writePriority)
    {
        _writeReadRatio = writeReadRatio;
        _writePriority = writePriority;
    }
    private bool _writePriority;
    private ThingsGateway.Gateway.Application.AsyncAutoResetEvent _readerLock = new ThingsGateway.Gateway.Application.AsyncAutoResetEvent(false); // 控制读计数
    private long _writerCount = 0; // 当前活跃的写线程数
    private long _readerCount = 0; // 当前被阻塞的读线程数
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    /// <summary>
    /// 获取读锁，支持多个线程并发读取，但写入时会阻止所有读取。
    /// </summary>
    public ValueTask<CancellationToken> ReaderLockAsync(CancellationToken cancellationToken)
    {
        return ReaderLockAsync(this, cancellationToken);

        static async PooledValueTask<CancellationToken> ReaderLockAsync(AsyncReadWriteLock1 @this, CancellationToken cancellationToken)
        {
            if (Interlocked.Read(ref @this._writerCount) > 0)
            {
                Interlocked.Increment(ref @this._readerCount);

                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                // 第一个读者需要获取写入锁，防止写操作
                await @this._readerLock.WaitOneAsync(cancellationToken).ConfigureAwait(false);

                Interlocked.Decrement(ref @this._readerCount);

            }
            return @this._cancellationTokenSource.Token;
        }
    }

    public bool WriteWaited => _writerCount > 0;
    public bool ReadWaited => _readerCount > 0;

    /// <summary>
    /// 获取写锁，阻止所有读取。
    /// </summary>
    public ValueTask<IDisposable> WriterLockAsync()
    {
        return WriterLockAsync(this);

        static async PooledValueTask<IDisposable> WriterLockAsync(AsyncReadWriteLock1 @this)
        {
            if (Interlocked.Increment(ref @this._writerCount) == 1)
            {
                if (@this._writePriority)
                {
                    var cancellationTokenSource = @this._cancellationTokenSource;
                    @this._cancellationTokenSource = new();
                    await cancellationTokenSource.SafeCancelAsync().ConfigureAwait(false); // 取消读取
                    cancellationTokenSource.SafeDispose();
                }
            }

            return new Writer(@this);
        }
    }
    private object lockObject = new();
    private void ReleaseWriter()
    {

        var writerCount = Interlocked.Decrement(ref _writerCount);

        // 每次释放写时，总是唤醒至少一个读
        _readerLock.Set();

        if (writerCount == 0)
        {
            var resetEvent = _readerLock;
            //_readerLock = new(false);
            Interlocked.Exchange(ref _writeSinceLastReadCount, 0);
            resetEvent.SetAll();
        }
        else
        {
            lock (lockObject)
            {

                // 读写占空比， 用于控制写操作与读操作的比率。该比率 n 次写入操作会执行一次读取操作。即使在应用程序执行大量的连续写入操作时，也必须确保足够的读取数据处理时间。相对于更加均衡的读写数据流而言，该特点使得外部写入可连续无顾忌操作
                if (_writeReadRatio > 0)
                {
                    var count = Interlocked.Increment(ref _writeSinceLastReadCount);
                    if (count >= _writeReadRatio)
                    {
                        Interlocked.Exchange(ref _writeSinceLastReadCount, 0);
                        //_readerLock.Set();
                    }
                }
                else
                {
                    //_readerLock.Set();
                }
            }

        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_cancellationTokenSource != null)
        {
            await _cancellationTokenSource.SafeCancelAsync().ConfigureAwait(false);
            _cancellationTokenSource.SafeDispose();
        }
        _readerLock.SetAll();
    }

    private int _writeSinceLastReadCount = 0;
    private struct Writer : IDisposable
    {
        private readonly AsyncReadWriteLock1 _lock;

        public Writer(AsyncReadWriteLock1 lockObj)
        {
            _lock = lockObj;
        }

        public void Dispose()
        {
            _lock.ReleaseWriter();
        }
    }
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

public class AsyncReadWriteLock
{
    private AsyncAutoResetEvent _readerLock = new AsyncAutoResetEvent(false); // 控制读计数
    private long _writerCount = 0; // 当前活跃的写线程数

    /// <summary>
    /// 获取读锁，支持多个线程并发读取，但写入时会阻止所有读取。
    /// </summary>
    public async Task ReaderLockAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Read(ref _writerCount) > 0)
        {
            // 第一个读者需要获取写入锁，防止写操作
            await _readerLock.WaitOneAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 获取写锁，阻止所有读取。
    /// </summary>
    public IDisposable WriterLock()
    {
        Interlocked.Increment(ref _writerCount);
        return new Writer(this);
    }

    private void ReleaseWriter()
    {
        Interlocked.Decrement(ref _writerCount);

        if (Interlocked.Read(ref _writerCount) == 0)
        {
            var resetEvent = _readerLock;
            _readerLock = new(false);
            resetEvent.SafeDispose();
        }
    }

    private struct Writer : IDisposable
    {
        private readonly AsyncReadWriteLock _lock;

        public Writer(AsyncReadWriteLock lockObj)
        {
            _lock = lockObj;
        }

        public void Dispose()
        {
            _lock.ReleaseWriter();
        }
    }
}

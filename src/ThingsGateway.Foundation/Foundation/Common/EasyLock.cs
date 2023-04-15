namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 简单锁，采用Interlocked和AutoResetEvent
    /// </summary>
    public sealed class EasyLock : DisposableObject
    {
        private Lazy<AsyncAutoResetEvent> m_waiterLock = new Lazy<AsyncAutoResetEvent>(() => new AsyncAutoResetEvent(false));

        private int m_waiters = 0;

        /// <summary>
        /// 当前锁是否在等待当中
        /// </summary>
        public bool IsWaitting => (uint)m_waiters > 0;
        /// <summary>
        /// 进入锁
        /// </summary>
        public void Lock()
        {
            if (Interlocked.Increment(ref m_waiters) == 1)
            {
                return;
            }
            m_waiterLock.Value.WaitOneAsync().GetAwaiter().GetResult();
        }
        /// <summary>
        /// 进入锁
        /// </summary>
        public async Task LockAsync()
        {
            if (Interlocked.Increment(ref m_waiters) == 1)
            {
                return;
            }
            await m_waiterLock.Value.WaitOneAsync();
        }
        /// <summary>
        /// 离开锁
        /// </summary>
        public void UnLock()
        {
            if (Interlocked.Decrement(ref m_waiters) == 0)
            {
                return;
            }
            m_waiterLock.Value.Set();
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            m_waiterLock.Value.Dispose();
            base.Dispose(disposing);
        }
    }
}
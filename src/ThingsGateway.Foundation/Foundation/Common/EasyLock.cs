namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 简单锁，采用Interlocked和AutoResetEvent
    /// </summary>
    public sealed class EasyLock : DisposableObject
    {
        private static long easyLockCount;

        private static long easyLockWaitCount;

        private Lazy<AutoResetEvent> m_waiterLock = new Lazy<AutoResetEvent>(() => new AutoResetEvent(false));

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
            Interlocked.Increment(ref easyLockCount);
            if (Interlocked.Increment(ref m_waiters) == 1)
            {
                return;
            }
            Interlocked.Increment(ref easyLockWaitCount);
            m_waiterLock.Value.WaitOne();
        }
        /// <summary>
        /// 离开锁
        /// </summary>
        public void UnLock()
        {
            Interlocked.Decrement(ref easyLockCount);
            if (Interlocked.Decrement(ref m_waiters) == 0)
            {
                return;
            }

            Interlocked.Decrement(ref easyLockWaitCount);
            m_waiterLock.Value.Set();
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            m_waiterLock.Value.Close();
            base.Dispose(disposing);
        }
    }
}
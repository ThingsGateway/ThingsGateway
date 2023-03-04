namespace ThingsGateway.Foundation
{
    public sealed class EasyLock : DisposableObject
    {
        private static long easyLockCount;

        private static long easyLockWaitCount;

        private Lazy<AutoResetEvent> m_waiterLock = new Lazy<AutoResetEvent>(() => new AutoResetEvent(false));

        private int m_waiters = 0;

        /// <summary>
        /// 所有进入锁的信息<br />
        /// </summary>
        public static long EasyLockCount => easyLockCount;

        /// <summary>正在等待的锁的统计信息</summary>
        public static long EasyLockWaitCount => easyLockWaitCount;

        /// <summary>当前锁是否在等待当中</summary>
        public bool IsWaitting => (uint)m_waiters > 0;

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

        protected override void Dispose(bool disposing)
        {
            m_waiterLock.Value.Close();
            base.Dispose(disposing);
        }
    }
}
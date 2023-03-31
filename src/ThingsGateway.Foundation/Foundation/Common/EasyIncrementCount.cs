namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 自增
    /// </summary>
    public sealed class EasyIncrementCount : DisposableObject
    {
        private long current = 0;
        private EasyLock easyLock;
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
            easyLock.Lock();
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

            easyLock.UnLock();
            return current;
        }

        /// <summary>
        /// 将当前的值重置为初始值。
        /// </summary>
        public void ResetCurrentValue()
        {
            easyLock.Lock();
            current = start;
            easyLock.UnLock();
        }

        /// <summary>
        /// 将当前的值重置为指定值
        /// </summary>
        public void ResetCurrentValue(long value)
        {
            easyLock.Lock();
            current = value <= max ? value >= start ? value : start : max;
            easyLock.UnLock();
        }

        /// <summary>
        /// 重置当前序号的最大值
        /// </summary>
        public void ResetMaxValue(long max)
        {
            easyLock.Lock();
            if (max > start)
            {
                if (max < current)
                {
                    current = start;
                }

                this.max = max;
            }
            easyLock.UnLock();
        }

        /// <summary>
        /// 重置当前序号的初始值
        /// </summary>
        public void ResetStartValue(long start)
        {
            easyLock.Lock();
            if (start < max)
            {
                if (current < start)
                {
                    current = start;
                }

                this.start = start;
            }
            easyLock.UnLock();
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            easyLock.Dispose();
            base.Dispose(disposing);
        }
    }
}
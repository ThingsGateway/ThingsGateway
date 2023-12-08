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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 计数器
    /// </summary>
    public class Counter
    {
        /// <summary>
        /// 周期内的累计计数值。
        /// </summary>
        protected long m_count;

        /// <summary>
        /// 最后一次递增时间
        /// </summary>
        protected DateTime m_lastIncrement;

        /// <summary>
        /// 周期内的累计计数值。
        /// </summary>
        public long Count { get => this.m_count; }

        /// <summary>
        /// 最后一次递增时间
        /// </summary>
        public DateTime LastIncrement { get => this.m_lastIncrement; }

        /// <summary>
        /// 当达到一个周期时触发。
        /// </summary>
        public Action<long> OnPeriod { get; set; }

        /// <summary>
        /// 计数周期。默认1秒。
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// 累计增加计数
        /// </summary>
        /// <param name="value"></param>
        /// <returns>返回值表示当前递增的是否在一个周期内。</returns>
        public bool Increment(long value)
        {
            bool isPeriod;
            if (DateTime.Now - this.LastIncrement > this.Period)
            {
                this.OnPeriod?.Invoke(this.m_count);
                Interlocked.Exchange(ref this.m_count, 0);
                isPeriod = false;
                this.m_lastIncrement = DateTime.Now;
            }
            else
            {
                isPeriod = true;
            }
            Interlocked.Add(ref this.m_count, value);
            return isPeriod;
        }

        /// <summary>
        /// 累计增加一个计数
        /// </summary>
        /// <returns></returns>
        public bool Increment()
        {
            return this.Increment(1);
        }

        /// <summary>
        /// 重置<see cref="Count"/>和<see cref="LastIncrement"/>
        /// </summary>
        public void Reset()
        {
            this.m_count = 0;
            this.m_lastIncrement = default;
        }
    }
}
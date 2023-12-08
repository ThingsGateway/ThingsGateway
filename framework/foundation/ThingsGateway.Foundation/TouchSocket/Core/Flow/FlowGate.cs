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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 流量控制器。
    /// </summary>
    public class FlowGate : Counter
    {
        /// <summary>
        /// 最大值
        /// </summary>
        public long Maximum { get; set; } = long.MaxValue;

        /// <summary>
        /// 最长休眠周期。默认为5s.
        /// <para>当设置为5s时，假如设置的<see cref="Maximum"/>=10，而一次递增了100，则理应会休眠10s，但是会休眠5s。反之，如果设置1，则每秒周期都会清空。</para>
        /// </summary>
        public TimeSpan MaximumWaitTime { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 检测等待
        /// </summary>
        public void AddCheckWait(long increment)
        {
            if (this.Increment(increment))
            {
                if (this.m_count > this.Maximum)
                {
                    var time = (DateTime.Now - this.LastIncrement);
                    var waitTime = this.Period - time <= TimeSpan.Zero ? TimeSpan.Zero : (this.GetBaseTime() - time);
                    Thread.Sleep(waitTime);
                }
            }
        }

        /// <summary>
        /// 检测等待
        /// </summary>
        /// <param name="increment"></param>
        /// <returns></returns>
        public async Task AddCheckWaitAsync(long increment)
        {
            if (this.Increment(increment))
            {
                if (this.m_count > this.Maximum)
                {
                    var time = (DateTime.Now - this.LastIncrement);
                    var waitTime = this.Period - time <= TimeSpan.Zero ? TimeSpan.Zero : (this.GetBaseTime() - time);
                    await Task.Delay(waitTime);
                }
            }
        }

        private TimeSpan GetBaseTime()
        {
            return TimeSpan.FromTicks(Math.Min((int)((double)this.m_count / this.Maximum * this.Period.Ticks), this.MaximumWaitTime.Ticks));
        }
    }
}
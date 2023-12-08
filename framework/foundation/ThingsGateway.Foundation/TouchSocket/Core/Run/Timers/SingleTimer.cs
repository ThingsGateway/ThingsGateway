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
    /// 不可重入的Timer
    /// </summary>
    public class SingleTimer : DisposableObject
    {
        private readonly Action<SingleTimer> m_action1;
        private readonly Action<SingleTimer, object> m_action2;
        private readonly object m_state;
        private readonly Timer m_timer;
        private readonly Action m_action3;
        private int m_signal = 1;

        /// <summary>
        /// 是否暂停执行。
        /// </summary>
        public bool Pause { get; set; }

        /// <summary>
        /// 自启动以来执行的次数。
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        public SingleTimer(int period, Action action)
        {
            this.m_timer = new Timer(this.OnTimer, null, 0, period);
            this.m_action3 = action;
            this.m_state = null;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        public SingleTimer(TimeSpan period, Action action)
        {
            this.m_timer = new Timer(this.OnTimer, null, TimeSpan.Zero, period);
            this.m_action3 = action;
            this.m_state = null;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        public SingleTimer(int period, Action<SingleTimer> action)
        {
            this.m_timer = new Timer(this.OnTimer, null, 0, period);
            this.m_action1 = action;
            this.m_state = null;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="period"></param>
        public SingleTimer(int period, Action<SingleTimer, object> action)
        {
            this.m_timer = new Timer(this.OnTimer, null, 0, period);
            this.m_action2 = action;
            this.m_state = null;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        public SingleTimer(object state, TimeSpan period, Action<SingleTimer> action)
        {
            this.m_timer = new Timer(this.OnTimer, state, TimeSpan.Zero, period);
            this.m_action1 = action;
            this.m_state = state;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        public SingleTimer(object state, TimeSpan period, Action<SingleTimer, object> action)
        {
            this.m_timer = new Timer(this.OnTimer, state, TimeSpan.Zero, period);
            this.m_action2 = action;
            this.m_state = state;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        public SingleTimer(object state, int period, Action<SingleTimer> action)
        {
            this.m_timer = new Timer(this.OnTimer, state, 0, period);
            this.m_action1 = action;
            this.m_state = state;
        }

        /// <summary>
        /// 不可重入的Timer
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        /// <param name="period"></param>
        public SingleTimer(object state, int period, Action<SingleTimer, object> action)
        {
            this.m_timer = new Timer(this.OnTimer, state, 0, period);
            this.m_action2 = action;
            this.m_state = state;
        }

        private void OnTimer(object state)
        {
            if (this.Pause)
            {
                return;
            }
            if (Interlocked.Decrement(ref this.m_signal) == 0)
            {
                try
                {
                    this.Count++;
                    this.m_action1?.Invoke(this);
                    this.m_action2?.Invoke(this, this.m_state);
                    this.m_action3?.Invoke();
                }
                catch
                {
                }
                finally
                {
                    this.m_signal = 1;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_timer.SafeDispose();
            base.Dispose(disposing);
        }
    }
}
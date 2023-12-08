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

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// 关于具有流速的操作器。
    /// </summary>
    public abstract class FlowOperator
    {
        /// <summary>
        /// 已完成长度
        /// </summary>
        protected long completedLength;

        /// <summary>
        /// 进度
        /// </summary>
        protected float m_progress;

        private readonly FlowGate m_flowGate = new FlowGate();

        private long m_speed;

        private long m_speedTemp;

        /// <summary>
        /// 已完成长度
        /// </summary>
        /// <returns></returns>
        public long CompletedLength { get => this.completedLength; }

        /// <summary>
        /// 流量控制器。
        /// </summary>
        public FlowGate FlowGate => this.m_flowGate;

        /// <summary>
        /// 由<see cref="Result"/>的结果，判断是否已结束操作。
        /// </summary>
        public virtual bool IsEnd { get => this.Result.ResultCode != ResultCode.Default; }

        /// <summary>
        /// 数据源的全部长度。
        /// </summary>
        public long Length { get; protected set; }

        /// <summary>
        /// 最大传输速度。
        /// </summary>
        public virtual long MaxSpeed { get => this.m_flowGate.Maximum; set => this.m_flowGate.Maximum = value; }

        /// <summary>
        /// 元数据
        /// </summary>
        public Metadata Metadata { get; set; }

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress => this.m_progress;

        /// <summary>
        /// 执行结果
        /// </summary>
        public Result Result { get; protected set; }

        /// <summary>
        /// 超时时间，默认10*1000ms。
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 可取消令箭
        /// </summary>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// 从上次获取到此次获得的速度
        /// </summary>
        /// <returns></returns>
        public long Speed()
        {
            this.m_speed = this.m_speedTemp;
            this.m_speedTemp = 0;
            return this.m_speed;
        }

        /// <summary>
        /// 添加流速(线程安全)
        /// </summary>
        /// <param name="flow"></param>
        protected virtual void ProtectedAddFlow(int flow)
        {
            this.m_flowGate.AddCheckWait(flow);
            Interlocked.Add(ref this.m_speedTemp, flow);
            this.m_progress = (float)((double)Interlocked.Add(ref this.completedLength, flow) / this.Length);
        }

        /// <summary>
        /// 添加流速(线程安全)
        /// </summary>
        /// <param name="flow"></param>
        /// <returns></returns>
        protected virtual async Task ProtectedAddFlowAsync(int flow)
        {
            await this.m_flowGate.AddCheckWaitAsync(flow);
            Interlocked.Add(ref this.m_speedTemp, flow);
            this.m_progress = (float)((double)Interlocked.Add(ref this.completedLength, flow) / this.Length);
        }
    }
}
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
using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 智能安全队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntelligentConcurrentQueue<T> : ConcurrentQueue<T>
    {
        private int m_count;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxCount"></param>
        public IntelligentConcurrentQueue(int maxCount)
        {
            this.MaxCount = maxCount;
        }

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public int MaxCount { get; private set; }

        /// <summary>
        /// 长度
        /// </summary>
        public new int Count => this.m_count;

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        public new void Enqueue(T item)
        {
            SpinWait.SpinUntil(this.Check);
            Interlocked.Increment(ref this.m_count);
            base.Enqueue(item);
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public new bool TryDequeue(out T result)
        {
            if (base.TryDequeue(out result))
            {
                Interlocked.Decrement(ref this.m_count);
                return true;
            }
            return false;
        }

        private bool Check()
        {
            return this.m_count < this.MaxCount;
        }
    }
}
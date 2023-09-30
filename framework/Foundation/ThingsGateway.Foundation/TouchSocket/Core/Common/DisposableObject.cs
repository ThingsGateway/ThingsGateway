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
using System.Runtime.CompilerServices;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 具有释放的对象。
    /// 并未实现析构函数相关。
    /// </summary>
    public class DisposableObject : IDisposable
    {
        /// <summary>
        /// 判断是否已释放。
        /// </summary>
        private volatile bool m_disposedValue;

        /// <summary>
        /// 标识该对象是否已被释放
        /// </summary>
        public bool DisposedValue { get => this.m_disposedValue; }

        /// <summary>
        /// 判断是否已经被释放，如果是，则抛出异常。
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfDisposed()
        {
            if (this.m_disposedValue)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        /// <summary>
        /// 调用释放，切换释放状态。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            this.m_disposedValue = true;
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
        }
    }
}
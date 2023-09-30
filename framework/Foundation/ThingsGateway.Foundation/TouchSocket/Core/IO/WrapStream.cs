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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 包装的流。为避免该流释放时，内部流也会被释放的问题
    /// </summary>

    public partial class WrapStream : Stream
    {
        private readonly Stream m_stream;

        /// <summary>
        /// 包装的流。为避免该流释放时，内部流也会被释放的问题
        /// </summary>
        /// <param name="stream"></param>
        public WrapStream(Stream stream)
        {
            this.m_stream = stream;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanRead => this.m_stream.CanRead;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSeek => this.m_stream.CanSeek;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanWrite => this.m_stream.CanWrite;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override long Length => this.m_stream.Length;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override long Position { get => this.m_stream.Position; set => this.m_stream.Position = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>

        public override void Flush()
        {
            this.m_stream.Flush();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.m_stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.m_stream.Seek(offset, origin);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            this.m_stream.SetLength(value);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.m_stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// 没有关闭效果
        /// </summary>
        public override void Close()
        {
        }

        /// <summary>
        /// 没有释放效果
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
        }
    }
}
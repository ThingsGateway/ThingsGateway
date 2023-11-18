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

namespace ThingsGateway.Foundation.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件片段句柄。
    /// </summary>
    public class FileSection : PackageBase
    {
        /// <summary>
        /// 文件句柄唯一标识
        /// </summary>
        public int ResourceHandle { get; internal set; }

        /// <summary>
        /// 分块状态。
        /// </summary>
        public FileSectionStatus Status { get; internal set; }

        /// <summary>
        /// 片段的流偏移量。
        /// </summary>
        public long Offset { get; internal set; }

        /// <summary>
        /// 存于集合的索引。
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// 片段长度
        /// </summary>
        public int Length { get; internal set; }

        /// <inheritdoc/>
        public override void Package(in ByteBlock byteBlock)
        {
            byteBlock.Write((byte)this.Status);
            byteBlock.Write(this.ResourceHandle);
            byteBlock.Write(this.Offset);
            byteBlock.Write(this.Length);
            byteBlock.Write(this.Index);
        }

        /// <inheritdoc/>
        public override void Unpackage(in ByteBlock byteBlock)
        {
            this.Status = (FileSectionStatus)byteBlock.ReadByte();
            this.ResourceHandle = byteBlock.ReadInt32();
            this.Offset = byteBlock.ReadInt64();
            this.Length = byteBlock.ReadInt32();
            this.Index = byteBlock.ReadInt32();
        }

        /// <summary>
        /// 判断基本信息是否一致。
        /// </summary>
        /// <param name="fileSection"></param>
        /// <returns></returns>
        public bool Equals(FileSection fileSection)
        {
            if (fileSection == null)
            {
                return false;
            }
            else
            {
                return this.Index == fileSection.Index &&
                this.Offset == fileSection.Offset &&
                this.Length == fileSection.Length &&
                this.ResourceHandle == fileSection.ResourceHandle;
            }
        }
    }
}
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
    /// 远程文件系统信息
    /// </summary>
    public abstract class RemoteFileSystemInfo : PackageBase
    {
        /// <summary>
        /// 当前文件或目录的特性
        /// </summary>
        public FileAttributes Attributes { get; set; }

        /// <summary>
        /// 当前文件或目录的创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 目录或文件的完整目录。
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 上次访问当前文件或目录的时间
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// 上次写入当前文件或目录的时间
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// 目录或文件的名称。
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc/>
        public override void Package(in ByteBlock byteBlock)
        {
            byteBlock.Write(this.LastWriteTime);
            byteBlock.Write(this.LastAccessTime);
            byteBlock.Write(this.CreationTime);
            byteBlock.Write((int)this.Attributes);
            byteBlock.Write(this.FullName);
            byteBlock.Write(this.Name);
        }

        /// <inheritdoc/>
        public override void Unpackage(in ByteBlock byteBlock)
        {
            this.LastWriteTime = byteBlock.ReadDateTime();
            this.LastAccessTime = byteBlock.ReadDateTime();
            this.CreationTime = byteBlock.ReadDateTime();
            this.Attributes = (FileAttributes)byteBlock.ReadInt32();
            this.FullName = byteBlock.ReadString();
            this.Name = byteBlock.ReadString();
        }
    }
}
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
    /// 文件资源定位器。
    /// </summary>
    public class FileResourceLocator : DisposableObject
    {
        /// <summary>
        /// 缓存文件的扩展名。
        /// </summary>
        public const string ExtensionName = ".dmtpfile";

        /// <summary>
        /// 根据资源信息，初始化一个仅读的定位器。
        /// </summary>
        /// <param name="fileResourceInfo"></param>
        public FileResourceLocator(FileResourceInfo fileResourceInfo)
        {
            this.FileResourceInfo = fileResourceInfo;
            this.FileAccess = FileAccess.Read;
            this.FileStorage = FilePool.GetFileStorageForRead(fileResourceInfo.FileInfo.FullName);
            this.LocatorPath = fileResourceInfo.FileInfo.FullName;
            this.LastActiveTime = DateTime.Now;
        }

        /// <summary>
        /// 根据资源信息和保存目录，初始化一个可写入的定位器。
        /// </summary>
        /// <param name="fileResourceInfo"></param>
        /// <param name="locatorPath"></param>
        /// <param name="overwrite"></param>
        /// <exception cref="ArgumentException"></exception>
        public FileResourceLocator(FileResourceInfo fileResourceInfo, string locatorPath, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(locatorPath))
            {
                throw new ArgumentException($"“{nameof(locatorPath)}”不能为 null 或空。", nameof(locatorPath));
            }
            if (overwrite)
            {
                FileUtility.Delete(locatorPath);
            }
            this.LocatorPath = locatorPath;
            this.FileResourceInfo = fileResourceInfo;
            this.FileAccess = FileAccess.Write;
            this.FileStorage = FilePool.GetFileStorageForWrite(this.LocatorPath + ExtensionName);
            this.LastActiveTime = DateTime.Now;
        }

        /// <summary>
        /// 文件访问类型。
        /// </summary>
        public FileAccess FileAccess { get; }

        /// <summary>
        /// 资源信息。
        /// </summary>
        public FileResourceInfo FileResourceInfo { get; private set; }

        /// <summary>
        /// 文件访问存储器。
        /// </summary>
        public FileStorage FileStorage { get; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime LastActiveTime { get; private set; }

        /// <summary>
        /// 定位器指向的实际文件的全名称。
        /// </summary>
        public string LocatorPath { get; }

        /// <summary>
        /// 获取一个默认状态，或者失败状态的文件片段
        /// </summary>
        /// <returns></returns>
        public FileSection GetDefaultOrFailFileSection()
        {
            this.LastActiveTime = DateTime.Now;
            if (this.FileAccess == FileAccess.Read)
            {
                return default;
            }
            else
            {
                return this.FileResourceInfo.FileSections.FirstOrDefault(a => a.Status == FileSectionStatus.Default || a.Status == FileSectionStatus.Fail);
            }
        }

        /// <summary>
        /// 获取没有完成的文件块集合。
        /// </summary>
        /// <returns></returns>
        public FileSection[] GetUnfinishedFileSection()
        {
            this.LastActiveTime = DateTime.Now;
            return this.FileAccess == FileAccess.Read
                ? (new FileSection[0])
                : this.FileResourceInfo.FileSections.Where(a => a.Status != FileSectionStatus.Finished).ToArray();
        }

        /// <summary>
        /// 读取字节。
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int ReadBytes(long pos, byte[] buffer, int offset, int length)
        {
            this.LastActiveTime = DateTime.Now;
            return this.FileStorage.Read(pos, buffer, offset, length);
        }

        /// <summary>
        /// 按文件块读取。
        /// </summary>
        /// <param name="fileSection"></param>
        /// <returns></returns>
        public FileSectionResult ReadFileSection(FileSection fileSection)
        {
            try
            {
                this.LastActiveTime = DateTime.Now;
                if (this.FileAccess != FileAccess.Read)
                {
                    return new FileSectionResult(ResultCode.Error, "该定位器是只写的。", default, fileSection);
                }
                if (fileSection.Index >= this.FileResourceInfo.FileSections.Length)
                {
                    return new FileSectionResult(ResultCode.Error, "实际数据块索引超出，可能写入的文件已经发生变化。", default, fileSection);
                }
                var oldFileSection = this.FileResourceInfo.FileSections[fileSection.Index];
                if (oldFileSection != fileSection)
                {
                    return new FileSectionResult(ResultCode.Error, "数据块不一致。", default, fileSection);
                }
                var bufferByteBlock = new ByteBlock(fileSection.Length);
                var r = this.FileStorage.Read(oldFileSection.Offset, bufferByteBlock.Buffer, 0, fileSection.Length);
                if (r != fileSection.Length)
                {
                    return new FileSectionResult(ResultCode.Error, "读取长度不一致。", default, fileSection);
                }
                else
                {
                    bufferByteBlock.SetLength(r);
                    return new FileSectionResult(ResultCode.Success, bufferByteBlock, fileSection);
                }
            }
            catch (Exception ex)
            {
                return new FileSectionResult(ResultCode.Exception, ex.Message, default, fileSection);
            }
        }

        /// <summary>
        /// 重新载入资源信息，并覆盖现有的分块的所有信息。要求资源路径必须一致。
        /// </summary>
        /// <param name="fileResourceInfo"></param>
        /// <exception cref="Exception"></exception>
        public void ReloadFileResourceInfo(FileResourceInfo fileResourceInfo)
        {
            this.LastActiveTime = DateTime.Now;
            if (fileResourceInfo.FileInfo.FullName != this.FileResourceInfo.FileInfo.FullName)
            {
                throw new Exception("资源指向的地址不一致。");
            }

            this.FileResourceInfo = fileResourceInfo;
        }

        /// <summary>
        /// 尝试完成该资源。
        /// </summary>
        /// <returns></returns>
        public Result TryFinished()
        {
            this.LastActiveTime = DateTime.Now;
            if (this.FileAccess == FileAccess.Read)
            {
                return Result.Success;
            }
            try
            {
                if (this.GetUnfinishedFileSection().Length > 0)
                {
                    return new Result(ResultCode.Fail, "还有文件块没有完成。");
                }
                if (this.FileStorage.Length != this.FileResourceInfo.FileInfo.Length)
                {
                    return new Result(ResultCode.Fail, "文件长度不一致。");
                }

                for (var i = 0; i < 10; i++)
                {
                    if (this.FileStorage.TryReleaseFile().IsSuccess())
                    {
                        break;
                    }
                }
                for (var i = 0; i < 10; i++)
                {
                    File.Move(this.LocatorPath + ExtensionName, this.LocatorPath);
                    if (new FileInfo(this.LocatorPath).Length == this.FileResourceInfo.FileInfo.Length)
                    {
                        break;
                    }
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return new Result(ex);
            }
        }

        /// <summary>
        /// 写入文件块
        /// </summary>
        /// <param name="fileSection"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Result WriteFileSection(FileSection fileSection, ArraySegment<byte> value)
        {
            try
            {
                this.LastActiveTime = DateTime.Now;
                if (this.FileAccess != FileAccess.Write)
                {
                    return new Result(ResultCode.Error, "该定位器是只读的。");
                }

                if (fileSection.Index >= this.FileResourceInfo.FileSections.Length)
                {
                    fileSection.Status = FileSectionStatus.Fail;
                    return new Result(ResultCode.Error, "实际数据块索引超出，可能写入的文件已经发生变化。");
                }

                var srcFileSection = this.FileResourceInfo.FileSections[fileSection.Index];

                if (value.Count != fileSection.Length)
                {
                    fileSection.Status = FileSectionStatus.Fail;
                    return new Result(ResultCode.Error, "实际数据长度与期望长度不一致。");
                }

                if (!srcFileSection.Equals(fileSection))
                {
                    return new Result(ResultCode.Error, "数据块不一致。");
                }
                this.FileStorage.Write(srcFileSection.Offset, value.Array, value.Offset, value.Count);
                this.FileStorage.Flush();
                fileSection.Status = FileSectionStatus.Finished;
                srcFileSection.Status = FileSectionStatus.Finished;
                return Result.Success;
            }
            catch (Exception ex)
            {
                return new Result(ex);
            }
        }

        /// <summary>
        /// 写入文件块
        /// </summary>
        /// <param name="fileSectionResult"></param>
        /// <returns></returns>
        public Result WriteFileSection(FileSectionResult fileSectionResult)
        {
            this.LastActiveTime = DateTime.Now;
            if (this.FileAccess != FileAccess.Write)
            {
                return new Result(ResultCode.Error, "该定位器是只读的。");
            }
            else
            {
                if (!fileSectionResult.IsSuccess())
                {
                    return new Result(fileSectionResult);
                }
                else
                {
                    return this.WriteFileSection(fileSectionResult.FileSection, fileSectionResult.Value);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.FileStorage.TryReleaseFile();
            base.Dispose(disposing);
        }
    }
}
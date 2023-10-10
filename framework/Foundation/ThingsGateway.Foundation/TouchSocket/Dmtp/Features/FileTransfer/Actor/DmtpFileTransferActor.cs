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

using ThingsGateway.Foundation.Resources;

namespace ThingsGateway.Foundation.Dmtp.FileTransfer
{
    /// <summary>
    /// 能够基于Dmtp协议提供文件传输功能
    /// </summary>
    public class DmtpFileTransferActor : IDmtpFileTransferActor
    {
        /// <summary>
        /// 创建一个<see cref="DmtpFileTransferActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        public DmtpFileTransferActor(IDmtpActor smtpActor)
        {
            this.DmtpActor = smtpActor;
        }

        #region 委托

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferedEventArgs.Result"/>属性值进行判断。
        /// </summary>
        public Func<IDmtpActor, FileTransferedEventArgs, Task> OnFileTransfered { get; set; }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        public Func<IDmtpActor, FileTransferingEventArgs, Task> OnFileTransfering { get; set; }

        #endregion 委托

        #region 字段

        private ushort m_finishedFileResourceInfo_Request;
        private ushort m_finishedFileResourceInfo_Response;
        private ushort m_pullFileResourceInfo_Request;
        private ushort m_pullFileResourceInfo_Response;
        private ushort m_pullFileSection_Request;
        private ushort m_pullFileSection_Response;
        private ushort m_pullSmallFile_Request;
        private ushort m_pullSmallFile_Response;
        private ushort m_pushFileResourceInfo_Request;
        private ushort m_pushFileResourceInfo_Response;
        private ushort m_pushFileSection_Request;
        private ushort m_pushFileSection_Response;
        private ushort m_pushSmallFile_Request;
        private ushort m_pushSmallFile_Response;

        #endregion 字段

        /// <summary>
        /// 处理收到的消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool InputReceivedData(DmtpMessage message)
        {
            var byteBlock = message.BodyByteBlock;
            if (message.ProtocolFlags == this.m_pullFileResourceInfo_Request)
            {
                try
                {
                    var waitFileResource = new WaitFileResource();
                    waitFileResource.UnpackageRouter(byteBlock);
                    if (waitFileResource.Route && this.DmtpActor.AllowRoute)
                    {
                        if (this.DmtpActor.TryRoute(RouteType.PullFile, waitFileResource))
                        {
                            if (this.DmtpActor.TryFindDmtpActor(waitFileResource.TargetId, out var actor))
                            {
                                actor.Send(this.m_pullFileResourceInfo_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                waitFileResource.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            waitFileResource.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                        }
                        waitFileResource.SwitchId();
                        byteBlock.Reset();
                        waitFileResource.Package(byteBlock);
                        this.DmtpActor.Send(this.m_pullFileResourceInfo_Response, byteBlock);
                    }
                    else
                    {
                        waitFileResource.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestPullFileResourceInfo, waitFileResource);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullFileResourceInfo_Response)
            {
                try
                {
                    var waitFileResource = new WaitFileResource();
                    waitFileResource.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitFileResource.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitFileResource.TargetId, out var actor))
                        {
                            actor.Send(this.m_pullFileResourceInfo_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileResource.UnpackageBody(byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(waitFileResource);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullFileSection_Request)
            {
                try
                {
                    var waitFileSection = new WaitFileSection();
                    waitFileSection.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitFileSection.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitFileSection.TargetId, out var actor))
                        {
                            actor.Send(this.m_pullFileSection_Request, byteBlock);
                        }
                        else
                        {
                            waitFileSection.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                            waitFileSection.SwitchId();
                            byteBlock.Reset();
                            waitFileSection.Package(byteBlock);
                            this.DmtpActor.Send(this.m_pullFileSection_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileSection.UnpackageBody(byteBlock);
                        this.RequestPullFileSection(waitFileSection);
                        //Task.Factory.StartNew(this.RequestPullFileSection, waitFileSection);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullFileSection_Response)
            {
                try
                {
                    var waitFileSection = new WaitFileSection();
                    waitFileSection.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitFileSection.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitFileSection.TargetId, out var actor))
                        {
                            actor.Send(this.m_pullFileSection_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileSection.UnpackageBody(byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(waitFileSection);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushFileResourceInfo_Request)
            {
                try
                {
                    var waitFileResource = new WaitFileResource();
                    waitFileResource.UnpackageRouter(byteBlock);
                    if (waitFileResource.Route && this.DmtpActor.AllowRoute)
                    {
                        if (this.DmtpActor.TryRoute(RouteType.PullFile, waitFileResource))
                        {
                            if (this.DmtpActor.TryFindDmtpActor(waitFileResource.TargetId, out var actor))
                            {
                                actor.Send(this.m_pushFileResourceInfo_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                waitFileResource.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            waitFileResource.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                        }
                        byteBlock.Reset();
                        waitFileResource.SwitchId();
                        waitFileResource.Package(byteBlock);
                        this.DmtpActor.Send(this.m_pushFileResourceInfo_Response, byteBlock);
                    }
                    else
                    {
                        waitFileResource.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestPushFileResourceInfo, waitFileResource);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushFileResourceInfo_Response)
            {
                try
                {
                    var waitFileResource = new WaitFileResource();
                    waitFileResource.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitFileResource.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitFileResource.TargetId, out var actor))
                        {
                            actor.Send(this.m_pushFileResourceInfo_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileResource.UnpackageBody(byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(waitFileResource);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushFileSection_Request)
            {
                try
                {
                    var waitFileSection = new WaitFileSection();
                    waitFileSection.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitFileSection.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitFileSection.TargetId, out var actor))
                        {
                            actor.Send(this.m_pushFileSection_Request, byteBlock);
                        }
                        else
                        {
                            waitFileSection.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                            waitFileSection.SwitchId();
                            byteBlock.Reset();
                            waitFileSection.Package(byteBlock);
                            this.DmtpActor.Send(this.m_pushFileSection_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileSection.UnpackageBody(byteBlock);
                        //this.RequestPushFileSection(waitFileSection);
                        //Task.Factory.StartNew(this.RequestPushFileSection, waitFileSection);
                        this.RequestPushFileSection(waitFileSection);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushFileSection_Response)
            {
                try
                {
                    var waitFileSection = new WaitFileSection();
                    waitFileSection.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitFileSection.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitFileSection.TargetId, out var actor))
                        {
                            actor.Send(this.m_pushFileSection_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFileSection.UnpackageBody(byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(waitFileSection);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_finishedFileResourceInfo_Request)
            {
                try
                {
                    var waitFinishedPackage = new WaitFinishedPackage();
                    waitFinishedPackage.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitFinishedPackage.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitFinishedPackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_finishedFileResourceInfo_Request, byteBlock);
                        }
                        else
                        {
                            waitFinishedPackage.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                            waitFinishedPackage.SwitchId();
                            byteBlock.Reset();
                            waitFinishedPackage.Package(byteBlock);
                            this.DmtpActor.Send(this.m_finishedFileResourceInfo_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFinishedPackage.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestFinishedFileResourceInfo, waitFinishedPackage);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_finishedFileResourceInfo_Response)
            {
                try
                {
                    var waitFinishedPackage = new WaitFinishedPackage();
                    waitFinishedPackage.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitFinishedPackage.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitFinishedPackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_finishedFileResourceInfo_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitFinishedPackage.UnpackageBody(byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(waitFinishedPackage);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullSmallFile_Request)
            {
                try
                {
                    var waitSmallFilePackage = new WaitSmallFilePackage();
                    waitSmallFilePackage.UnpackageRouter(byteBlock);
                    if (waitSmallFilePackage.Route && this.DmtpActor.AllowRoute)
                    {
                        if (this.DmtpActor.TryRoute(RouteType.PullFile, waitSmallFilePackage))
                        {
                            if (this.DmtpActor.TryFindDmtpActor(waitSmallFilePackage.TargetId, out var actor))
                            {
                                actor.Send(this.m_pullSmallFile_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                waitSmallFilePackage.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            waitSmallFilePackage.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                        }
                        byteBlock.Reset();
                        waitSmallFilePackage.SwitchId();
                        waitSmallFilePackage.Package(byteBlock);
                        this.DmtpActor.Send(this.m_pullSmallFile_Response, byteBlock);
                    }
                    else
                    {
                        waitSmallFilePackage.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestPullSmallFile, waitSmallFilePackage);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pullSmallFile_Response)
            {
                try
                {
                    var waitSmallFilePackage = new WaitSmallFilePackage();
                    waitSmallFilePackage.UnpackageRouter(byteBlock);
                    if (this.DmtpActor.AllowRoute && waitSmallFilePackage.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitSmallFilePackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_pullSmallFile_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitSmallFilePackage.UnpackageBody(byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(waitSmallFilePackage);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushSmallFile_Request)
            {
                try
                {
                    var waitSmallFilePackage = new WaitSmallFilePackage();
                    waitSmallFilePackage.UnpackageRouter(byteBlock);
                    if (waitSmallFilePackage.Route && this.DmtpActor.AllowRoute)
                    {
                        if (this.DmtpActor.TryRoute(RouteType.PullFile, waitSmallFilePackage))
                        {
                            if (this.DmtpActor.TryFindDmtpActor(waitSmallFilePackage.TargetId, out var actor))
                            {
                                actor.Send(this.m_pushSmallFile_Request, byteBlock);
                                return true;
                            }
                            else
                            {
                                waitSmallFilePackage.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                            }
                        }
                        else
                        {
                            waitSmallFilePackage.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                        }

                        byteBlock.Reset();
                        waitSmallFilePackage.SwitchId();
                        waitSmallFilePackage.Package(byteBlock);
                        this.DmtpActor.Send(this.m_pushSmallFile_Response, byteBlock);
                    }
                    else
                    {
                        waitSmallFilePackage.UnpackageBody(byteBlock);
                        Task.Factory.StartNew(this.RequestPushSmallFile, waitSmallFilePackage);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_pushSmallFile_Response)
            {
                try
                {
                    var waitSmallFilePackage = new WaitSmallFilePackage();
                    waitSmallFilePackage.UnpackageRouter(byteBlock);

                    if (this.DmtpActor.AllowRoute && waitSmallFilePackage.Route)
                    {
                        if (this.DmtpActor.TryFindDmtpActor(waitSmallFilePackage.TargetId, out var actor))
                        {
                            actor.Send(this.m_pushSmallFile_Response, byteBlock);
                        }
                    }
                    else
                    {
                        waitSmallFilePackage.UnpackageBody(byteBlock);
                        this.DmtpActor.WaitHandlePool.SetRun(waitSmallFilePackage);
                    }
                }
                catch (Exception ex)
                {
                    this.DmtpActor.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置处理协议标识的起始标识。默认为30；
        /// </summary>
        /// <param name="start"></param>
        public void SetProtocolFlags(ushort start)
        {
            this.m_pullFileResourceInfo_Request = start++;
            this.m_pullFileResourceInfo_Response = start++;
            this.m_pullFileSection_Request = start++;
            this.m_pullFileSection_Response = start++;
            this.m_pushFileResourceInfo_Request = start++;
            this.m_pushFileResourceInfo_Response = start++;
            this.m_pushFileSection_Request = start++;
            this.m_pushFileSection_Response = start++;
            this.m_finishedFileResourceInfo_Request = start++;
            this.m_finishedFileResourceInfo_Response = start++;
            this.m_pullSmallFile_Request = start++;
            this.m_pullSmallFile_Response = start++;
            this.m_pushSmallFile_Request = start++;
            this.m_pushSmallFile_Response = start++;
        }

        #region 属性

        private string m_rootPath = string.Empty;

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get; }

        /// <inheritdoc/>
        public IFileResourceController FileController { get; set; }

        /// <inheritdoc/>
        public int MaxSmallFileLength { get; set; } = 1024 * 1024;

        /// <inheritdoc/>
        public string RootPath
        {
            get => this.m_rootPath;
            set
            {
                value ??= string.Empty;
                this.m_rootPath = value;
            }
        }

        #endregion 属性

        private string GetFullPath(string path)
        {
            return this.FileController.GetFullPath(this.m_rootPath, path);
        }

        private bool TryFindDmtpFileTransferActor(string targetId, out DmtpFileTransferActor rpcActor)
        {
            if (targetId == this.DmtpActor.Id)
            {
                rpcActor = this;
                return true;
            }
            if (this.DmtpActor.TryFindDmtpActor(targetId, out var smtpActor))
            {
                if (smtpActor.GetDmtpFileTransferActor() is DmtpFileTransferActor newActor)
                {
                    rpcActor = newActor;
                    return true;
                }
            }

            rpcActor = default;
            return false;
        }

        #region Id传输

        /// <inheritdoc/>
        public FinishedResult FinishedFileResourceInfo(string targetId, FileResourceInfo fileResourceInfo, ResultCode code, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return this.PrivateFinishedFileResourceInfo(targetId, fileResourceInfo, code, metadata, timeout, token);
            }

            if (this.DmtpActor.AllowRoute && this.TryFindDmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.FinishedFileResourceInfo(fileResourceInfo, code, metadata, timeout, token);
            }
            else
            {
                return this.PrivateFinishedFileResourceInfo(targetId, fileResourceInfo, code, metadata, timeout, token);
            }
        }

        /// <inheritdoc/>
        public Task<FinishedResult> FinishedFileResourceInfoAsync(string targetId, FileResourceInfo fileResourceInfo, ResultCode code, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.FinishedFileResourceInfo(targetId, fileResourceInfo, code, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public FileResourceInfoResult PullFileResourceInfo(string targetId, string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return this.PrivatePullFileResourceInfo(targetId, path, metadata, fileSectionSize, timeout, token);
            }

            if (this.DmtpActor.AllowRoute && this.TryFindDmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PullFileResourceInfo(path, metadata, fileSectionSize, timeout, token);
            }
            else
            {
                return this.PrivatePullFileResourceInfo(targetId, path, metadata, fileSectionSize, timeout, token);
            }
        }

        /// <inheritdoc/>
        public Task<FileResourceInfoResult> PullFileResourceInfoAsync(string targetId, string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullFileResourceInfo(targetId, path, metadata, timeout, fileSectionSize, token);
            });
        }

        /// <inheritdoc/>
        public FileSectionResult PullFileSection(string targetId, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return this.PrivatePullFileSection(targetId, fileSection, timeout, token);
            }

            if (this.DmtpActor.AllowRoute && this.TryFindDmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PullFileSection(fileSection, timeout, token);
            }
            else
            {
                return this.PrivatePullFileSection(targetId, fileSection, timeout, token);
            }
        }

        /// <inheritdoc/>
        public Task<FileSectionResult> PullFileSectionAsync(string targetId, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullFileSection(targetId, fileSection, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Result PushFileResourceInfo(string targetId, string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return this.PrivatePushFileResourceInfo(targetId, savePath, fileResourceLocator, metadata, timeout, token);
            }

            if (this.DmtpActor.AllowRoute && this.TryFindDmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PushFileResourceInfo(savePath, fileResourceLocator, metadata, timeout, token);
            }
            else
            {
                return this.PrivatePushFileResourceInfo(targetId, savePath, fileResourceLocator, metadata, timeout, token);
            }
        }

        /// <inheritdoc/>
        public Task<Result> PushFileResourceInfoAsync(string targetId, string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushFileResourceInfo(targetId, savePath, fileResourceLocator, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Task<Result> PushFileSectionAsync(string targetId, FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushFileSection(targetId, fileResourceLocator, fileSection, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Result PushFileSection(string targetId, FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                return this.PrivatePushFileSection(targetId, fileResourceLocator, fileSection, timeout, token);
            }

            if (this.DmtpActor.AllowRoute && this.TryFindDmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PushFileSection(fileResourceLocator, fileSection, timeout, token);
            }
            else
            {
                return this.PrivatePushFileSection(targetId, fileResourceLocator, fileSection, timeout, token);
            }
        }

        #endregion Id传输

        #region 传输

        /// <inheritdoc/>

        public FinishedResult FinishedFileResourceInfo(FileResourceInfo fileResourceInfo, ResultCode code, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivateFinishedFileResourceInfo(default, fileResourceInfo, code, metadata, timeout, token);
        }

        /// <inheritdoc/>
        public Task<FinishedResult> FinishedFileResourceInfoAsync(FileResourceInfo fileResourceInfo, ResultCode code, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.FinishedFileResourceInfo(fileResourceInfo, code, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public FileResourceInfoResult PullFileResourceInfo(string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePullFileResourceInfo(default, path, metadata, fileSectionSize, timeout, token);
        }

        /// <inheritdoc/>
        public Task<FileResourceInfoResult> PullFileResourceInfoAsync(string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullFileResourceInfo(path, metadata, fileSectionSize, timeout, token);
            });
        }

        /// <inheritdoc/>
        public FileSectionResult PullFileSection(FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePullFileSection(default, fileSection, timeout, token);
        }

        /// <inheritdoc/>
        public Task<FileSectionResult> PullFileSectionAsync(FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullFileSection(fileSection, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Result PushFileResourceInfo(string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePushFileResourceInfo(default, savePath, fileResourceLocator, metadata, timeout, token);
        }

        /// <inheritdoc/>
        public Task<Result> PushFileResourceInfoAsync(string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushFileResourceInfo(savePath, fileResourceLocator, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Result PushFileSection(FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePushFileSection(default, fileResourceLocator, fileSection, timeout, token);
        }

        /// <inheritdoc/>
        public Task<Result> PushFileSectionAsync(FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushFileSection(fileResourceLocator, fileSection, timeout, token);
            });
        }

        #region Private

        private FinishedResult PrivateFinishedFileResourceInfo(string targetId, FileResourceInfo fileResourceInfo, ResultCode code, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            var waitFinishedPackage = new WaitFinishedPackage()
            {
                SourceId = this.DmtpActor.Id,
                TargetId = targetId,
                Route = targetId.HasValue(),
                ResourceHandle = fileResourceInfo.ResourceHandle,
                Metadata = metadata,
                Code = code
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(waitFinishedPackage);

            var byteBlock = new ByteBlock();
            try
            {
                waitFinishedPackage.Package(byteBlock);
                this.DmtpActor.Send(this.m_finishedFileResourceInfo_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFinishedPackage)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        return new FinishedResult(ResultCode.Success, waitFile.ResourceHandle);
                                    }
                                case TouchSocketDmtpStatus.ResourceHandleNotFind:
                                    {
                                        return new FinishedResult(ResultCode.Error, TouchSocketDmtpStatus.ResourceHandleNotFind.GetDescription(waitFile.ResourceHandle), waitFile.ResourceHandle);
                                    }
                                case TouchSocketDmtpStatus.HasUnFinished:
                                    {
                                        return new FinishedResult(ResultCode.Fail, TouchSocketDmtpStatus.HasUnFinished.GetDescription(), waitFile.ResourceHandle);
                                    }
                                default:
                                    {
                                        return new FinishedResult(ResultCode.Error, waitFile.Message, waitFile.ResourceHandle);
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return new FinishedResult(Result.Overtime, fileResourceInfo.ResourceHandle);
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new FinishedResult(Result.Canceled, fileResourceInfo.ResourceHandle);
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new FinishedResult(Result.UnknownFail, fileResourceInfo.ResourceHandle);
                        }
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private FileResourceInfoResult PrivatePullFileResourceInfo(string targetId, string path, Metadata metadata = default, int fileSectionSize = 1024 * 512, int timeout = 5000, CancellationToken token = default)
        {
            var waitFileResource = new WaitFileResource()
            {
                Path = path,
                TargetId = targetId,
                Route = targetId.HasValue(),
                SourceId = this.DmtpActor.Id,
                Metadata = metadata,
                FileSectionSize = fileSectionSize
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(waitFileResource);

            var byteBlock = new ByteBlock();
            try
            {
                waitFileResource.Package(byteBlock);
                this.DmtpActor.Send(this.m_pullFileResourceInfo_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFileResource)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        var resourceInfo = new FileResourceInfo(waitFile.FileInfo, fileSectionSize);
                                        resourceInfo.ResetResourceHandle(waitFile.ResourceHandle);
                                        return new FileResourceInfoResult(resourceInfo);
                                    }
                                case TouchSocketDmtpStatus.FileNotExists:
                                    {
                                        return new FileResourceInfoResult(TouchSocketDmtpStatus.FileNotExists.GetDescription(waitFile.Path));
                                    }
                                case TouchSocketDmtpStatus.RemoteRefuse:
                                default:
                                    {
                                        return new FileResourceInfoResult(waitFile.Status.ToStatus().GetDescription());
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return new FileResourceInfoResult(Result.Overtime);
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new FileResourceInfoResult(Result.Canceled);
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new FileResourceInfoResult(Result.UnknownFail);
                        }
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private FileSectionResult PrivatePullFileSection(string targetId, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            fileSection.Status = FileSectionStatus.Transfering;
            var waitFileSection = new WaitFileSection()
            {
                SourceId = this.DmtpActor.Id,
                TargetId = targetId,
                Route = targetId.HasValue(),
                FileSection = fileSection
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(waitFileSection);

            var byteBlock = new ByteBlock();
            try
            {
                waitFileSection.Package(byteBlock);
                this.DmtpActor.Send(this.m_pullFileSection_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFileSection)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        return new FileSectionResult(ResultCode.Success, waitFile.Value, fileSection);
                                    }
                                case TouchSocketDmtpStatus.ResourceHandleNotFind:
                                    {
                                        return new FileSectionResult(ResultCode.Error, TouchSocketDmtpStatus.ResourceHandleNotFind.GetDescription(waitFile.FileSection.ResourceHandle), default, fileSection);
                                    }
                                case TouchSocketDmtpStatus.ClientNotFind:
                                    {
                                        return new FileSectionResult(ResultCode.Error, TouchSocketDmtpStatus.ClientNotFind.GetDescription(targetId), default, fileSection);
                                    }
                                case TouchSocketDmtpStatus.LengthErrorWhenRead:
                                    {
                                        return new FileSectionResult(ResultCode.Error, TouchSocketDmtpStatus.LengthErrorWhenRead.GetDescription(), default, fileSection);
                                    }
                                default:
                                    return new FileSectionResult(ResultCode.Exception, waitFile.Message, default, fileSection);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return new FileSectionResult(ResultCode.Overtime, default, fileSection);
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new FileSectionResult(ResultCode.Canceled, default, fileSection);
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new FileSectionResult(ResultCode.Fail, default, fileSection);
                        }
                }
            }
            finally
            {
                fileSection.Status = FileSectionStatus.Transfered;
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private Result PrivatePushFileResourceInfo(string targetId, string savePath, FileResourceLocator fileResourceLocator, Metadata metadata = default, int timeout = 5000, CancellationToken token = default)
        {
            var fileResourceInfo = fileResourceLocator.FileResourceInfo;

            var waitFileResource = new WaitFileResource()
            {
                Path = savePath,
                TargetId = targetId,
                Route = targetId.HasValue(),
                SourceId = this.DmtpActor.Id,
                FileInfo = fileResourceInfo.FileInfo,
                Metadata = metadata,
                FileSectionSize = fileResourceInfo.FileSectionSize,
                ResourceHandle = fileResourceInfo.ResourceHandle,
                ContinuationIndex = fileResourceInfo.GetContinuationIndex()
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(waitFileResource);

            var byteBlock = new ByteBlock();
            try
            {
                waitFileResource.Package(byteBlock);
                this.DmtpActor.Send(this.m_pushFileResourceInfo_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFileResource)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        fileResourceLocator.FileResourceInfo.ResetResourceHandle(waitFile.ResourceHandle);
                                        return Result.Success;
                                    }
                                case TouchSocketDmtpStatus.RemoteRefuse:
                                    {
                                        return new Result(ResultCode.Error, waitFile.Status.ToStatus().GetDescription());
                                    }
                                case TouchSocketDmtpStatus.ClientNotFind:
                                    {
                                        return new Result(ResultCode.Error, waitFile.Status.ToStatus().GetDescription(targetId));
                                    }
                                default:
                                    return new Result(ResultCode.Error, waitFile.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return Result.Overtime;
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return Result.Canceled;
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return Result.UnknownFail;
                        }
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private Result PrivatePushFileSection(string targetId, FileResourceLocator fileResourceLocator, FileSection fileSection, int timeout = 5000, CancellationToken token = default)
        {
            fileSection.Status = FileSectionStatus.Transfering;
            using var fileSectionResult = fileResourceLocator.ReadFileSection(fileSection);
            if (!fileSectionResult.IsSuccess())
            {
                fileSection.Status = FileSectionStatus.Fail;
                return new Result(fileSectionResult);
            }
            using var waitFileSection = new WaitFileSection()
            {
                SourceId = this.DmtpActor.Id,
                TargetId = targetId,
                Route = targetId.HasValue(),
                FileSection = fileSection,
                Value = fileSectionResult.Value
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(waitFileSection);

            var byteBlock = new ByteBlock(fileSectionResult.Value.Len + 1024);
            try
            {
                waitFileSection.Package(byteBlock);
                this.DmtpActor.Send(this.m_pushFileSection_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitFileSection)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        fileSection.Status = FileSectionStatus.Finished;
                                        return Result.Success;
                                    }
                                case TouchSocketDmtpStatus.ResourceHandleNotFind:
                                    {
                                        fileSection.Status = FileSectionStatus.Fail;
                                        return new Result(ResultCode.Error, TouchSocketDmtpStatus.ResourceHandleNotFind.GetDescription(waitFile.FileSection.ResourceHandle));
                                    }
                                default:
                                    fileSection.Status = FileSectionStatus.Fail;
                                    return new Result(ResultCode.Error, waitFile.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            fileSection.Status = FileSectionStatus.Fail;
                            return Result.Overtime;
                        }
                    case WaitDataStatus.Canceled:
                        {
                            fileSection.Status = FileSectionStatus.Fail;
                            return Result.Canceled;
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            fileSection.Status = FileSectionStatus.Fail;
                            return Result.UnknownFail;
                        }
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        #endregion Private

        #endregion 传输

        #region Request

        /// <summary>
        /// 请求完成
        /// </summary>
        /// <param name="o"></param>
        private async Task RequestFinishedFileResourceInfo(object o)
        {
            //2.未找到
            //3.未完成
            try
            {
                var waitFinishedPackage = (WaitFinishedPackage)o;
                var transferType = TransferType.Pull;
                string resourcePath = default;
                string savePath = default;
                var resultThis = Result.Success;
                FileResourceInfo resourceInfo = default;
                try
                {
                    if (this.FileController.TryGetFileResourceLocator(waitFinishedPackage.ResourceHandle, out var locator))
                    {
                        if (locator.FileAccess == FileAccess.Read)
                        {
                            transferType = TransferType.Pull;
                            if (this.FileController.TryRelaseFileResourceLocator(waitFinishedPackage.ResourceHandle, out _))
                            {
                                waitFinishedPackage.Status = TouchSocketDmtpStatus.Success.ToValue();
                                resourcePath = locator.LocatorPath;
                                resourceInfo = locator.FileResourceInfo;
                            }
                            else
                            {
                                waitFinishedPackage.Status = TouchSocketDmtpStatus.ResourceHandleNotFind.ToValue();
                                resultThis = new Result(ResultCode.Fail, TouchSocketDmtpStatus.ResourceHandleNotFind.GetDescription(waitFinishedPackage.ResourceHandle));
                            }
                        }
                        else
                        {
                            transferType = TransferType.Push;
                            if (this.FileController.TryGetFileResourceLocator(waitFinishedPackage.ResourceHandle, out _))
                            {
                                resourceInfo = locator.FileResourceInfo;
                                savePath = locator.LocatorPath;
                                var sections = locator.GetUnfinishedFileSection();
                                if (sections.Length == 0)
                                {
                                    var result = locator.TryFinished();
                                    if (result.IsSuccess())
                                    {
                                        waitFinishedPackage.Status = TouchSocketDmtpStatus.Success.ToValue();
                                    }
                                    else
                                    {
                                        waitFinishedPackage.Status = TouchSocketDmtpStatus.Exception.ToValue();
                                        waitFinishedPackage.Message = result.Message;
                                        resultThis = result;
                                    }
                                }
                                else
                                {
                                    waitFinishedPackage.Status = TouchSocketDmtpStatus.HasUnFinished.ToValue();

                                    resultThis = new Result(ResultCode.Fail, TouchSocketDmtpStatus.HasUnFinished.GetDescription(sections.Length));
                                }
                            }
                            else
                            {
                                waitFinishedPackage.Status = TouchSocketDmtpStatus.ResourceHandleNotFind.ToValue();
                                resultThis = new Result(ResultCode.Fail, TouchSocketDmtpStatus.ResourceHandleNotFind.GetDescription(waitFinishedPackage.ResourceHandle));
                            }
                        }
                    }
                    else
                    {
                        waitFinishedPackage.Status = TouchSocketDmtpStatus.ResourceHandleNotFind.ToValue();
                        resultThis = new Result(ResultCode.Fail, TouchSocketDmtpStatus.ResourceHandleNotFind.GetDescription(waitFinishedPackage.ResourceHandle));
                    }
                }
                catch (Exception ex)
                {
                    waitFinishedPackage.Status = TouchSocketDmtpStatus.Exception.ToValue();
                    waitFinishedPackage.Message = ex.Message;
                }

                using (var byteBlock = new ByteBlock())
                {
                    waitFinishedPackage.SwitchId();
                    waitFinishedPackage.Package(byteBlock);
                    this.DmtpActor.Send(this.m_finishedFileResourceInfo_Response, byteBlock);
                }

                var args = new FileTransferedEventArgs(transferType, waitFinishedPackage?.Metadata, resourceInfo?.FileInfo, waitFinishedPackage.Code == ResultCode.Canceled ? Result.Canceled : resultThis)
                {
                    ResourcePath = resourcePath,
                    SavePath = savePath
                };
                await this.OnFileTransfered.Invoke(this.DmtpActor, args);
            }
            catch (Exception ex)
            {
                this.DmtpActor.Logger.Exception(ex);
            }
        }

        /// <summary>
        /// 请求拉取文件信息
        /// </summary>
        /// <param name="o"></param>
        private async Task RequestPullFileResourceInfo(object o)
        {
            //2.不响应
            //3.不允许
            //4.文件不存在
            //5.other
            try
            {
                var waitFileResource = (WaitFileResource)o;

                try
                {
                    var args = new FileTransferingEventArgs(TransferType.Pull, waitFileResource.Metadata, default)
                    {
                        ResourcePath = waitFileResource.Path
                    };

                    await this.OnFileTransfering.Invoke(this.DmtpActor, args);

                    var fullPath = this.GetFullPath(args.ResourcePath);

                    if (args.IsPermitOperation)
                    {
                        if (File.Exists(fullPath))
                        {
                            var locator = this.FileController.LoadFileResourceLocatorForRead(fullPath, waitFileResource.FileSectionSize);
                            waitFileResource.ResourceHandle = locator.FileResourceInfo.ResourceHandle;
                            waitFileResource.FileInfo = locator.FileResourceInfo.FileInfo;
                            waitFileResource.Status = TouchSocketDmtpStatus.Success.ToValue();
                        }
                        else
                        {
                            waitFileResource.Path = fullPath;
                            waitFileResource.Status = TouchSocketDmtpStatus.FileNotExists.ToValue();
                        }
                    }
                    else
                    {
                        waitFileResource.Status = TouchSocketDmtpStatus.RemoteRefuse.ToValue();
                        waitFileResource.Message = args.Message;
                    }
                }
                catch (Exception ex)
                {
                    waitFileResource.Message = ex.Message;
                    waitFileResource.Status = TouchSocketDmtpStatus.Exception.ToValue();
                }
                using (var byteBlock = new ByteBlock())
                {
                    waitFileResource.SwitchId();
                    waitFileResource.Package(byteBlock);
                    this.DmtpActor.Send(this.m_pullFileResourceInfo_Response, byteBlock);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 请求拉取文件块
        /// </summary>
        /// <param name="o"></param>
        private void RequestPullFileSection(object o)
        {
            try
            {
                //2.没找到
                //3.读取文件长度不一致
                var waitFileSection = (WaitFileSection)o;
                var length = waitFileSection.FileSection.Length;
                try
                {
                    var bufferByteBlock = new ByteBlock(length);
                    if (this.FileController.TryGetFileResourceLocator(waitFileSection.FileSection.ResourceHandle,
                   out var locator))
                    {
                        var r = locator.ReadBytes(waitFileSection.FileSection.Offset, bufferByteBlock.Buffer, 0, length);
                        if (r == length)
                        {
                            waitFileSection.Status = TouchSocketDmtpStatus.Success.ToValue();
                            bufferByteBlock.SetLength(r);
                            waitFileSection.Value = bufferByteBlock;
                        }
                        else
                        {
                            waitFileSection.Status = TouchSocketDmtpStatus.LengthErrorWhenRead.ToValue();
                        }
                    }
                    else
                    {
                        waitFileSection.Status = TouchSocketDmtpStatus.ResourceHandleNotFind.ToValue();
                    }

                    using (var byteBlock = new ByteBlock(length + 1024))
                    {
                        waitFileSection.SwitchId();
                        waitFileSection.Package(byteBlock);
                        this.DmtpActor.Send(this.m_pullFileSection_Response, byteBlock);
                    }
                }
                finally
                {
                    waitFileSection.Dispose();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 请求推送文件信息
        /// </summary>
        /// <param name="o"></param>
        private async Task RequestPushFileResourceInfo(object o)
        {
            //2.不响应
            //3.不支持
            //4.其他
            try
            {
                var waitFileResource = (WaitFileResource)o;
                try
                {
                    var args = new FileTransferingEventArgs(TransferType.Push, waitFileResource.Metadata, waitFileResource.FileInfo)
                    {
                        SavePath = waitFileResource.Path
                    };

                    await this.OnFileTransfering.Invoke(this.DmtpActor, args);

                    var savePath = this.GetFullPath(args.SavePath);

                    if (args.IsPermitOperation)
                    {
                        var locator = this.FileController.LoadFileResourceLocatorForWrite(savePath, new FileResourceInfo(waitFileResource.FileInfo, waitFileResource.FileSectionSize));

                        if (waitFileResource.ContinuationIndex > 0)//续传
                        {
                            long size = waitFileResource.ContinuationIndex * waitFileResource.FileSectionSize;
                            if (locator.FileStorage.Length < size)
                            {
                                waitFileResource.Status = TouchSocketDmtpStatus.Exception.ToValue();
                                waitFileResource.Message = "续传文件长度错误";
                            }
                            else
                            {
                                for (var i = 0; i < waitFileResource.ContinuationIndex; i++)
                                {
                                    locator.FileResourceInfo.FileSections[i].Status = FileSectionStatus.Finished;
                                }
                            }
                        }

                        waitFileResource.ResourceHandle = locator.FileResourceInfo.ResourceHandle;
                        waitFileResource.Status = TouchSocketDmtpStatus.Success.ToValue();
                    }
                    else
                    {
                        waitFileResource.Status = TouchSocketDmtpStatus.RemoteRefuse.ToValue();
                        waitFileResource.Message = args.Message;
                    }
                }
                catch (Exception ex)
                {
                    waitFileResource.Status = TouchSocketDmtpStatus.Exception.ToValue();
                    waitFileResource.Message = ex.Message;
                }
                using (var byteBlock = new ByteBlock())
                {
                    waitFileResource.SwitchId();
                    waitFileResource.Package(byteBlock);
                    this.DmtpActor.Send(this.m_pushFileResourceInfo_Response, byteBlock);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 请求推送文件块
        /// </summary>
        /// <param name="o"></param>
        private void RequestPushFileSection(object o)
        {
            //2.没找到
            //3.其他
            try
            {
                var waitFileSection = (WaitFileSection)o;
                try
                {
                    if (this.FileController.TryGetFileResourceLocator(waitFileSection.FileSection.ResourceHandle, out var locator))
                    {
                        var result = Result.UnknownFail;
                        for (var i = 0; i < 10; i++)
                        {
                            result = locator.WriteFileSection(waitFileSection.FileSection, waitFileSection.Value);
                            if (result.IsSuccess())
                            {
                                break;
                            }
                        }
                        if (result.IsSuccess())
                        {
                            waitFileSection.Status = TouchSocketDmtpStatus.Success.ToValue();
                        }
                        else
                        {
                            waitFileSection.Status = TouchSocketDmtpStatus.Exception.ToValue();
                            waitFileSection.Message = result.Message;
                        }
                    }
                    else
                    {
                        waitFileSection.Status = TouchSocketDmtpStatus.ResourceHandleNotFind.ToValue();
                    }
                }
                catch (Exception ex)
                {
                    waitFileSection.Status = TouchSocketDmtpStatus.Exception.ToValue();
                    waitFileSection.Message = ex.Message;
                }

                waitFileSection.Value.Dispose();
                waitFileSection.Value = default;
                using (var byteBlock = new ByteBlock())
                {
                    waitFileSection.SwitchId();
                    waitFileSection.Package(byteBlock);
                    this.DmtpActor.Send(this.m_pushFileSection_Response, byteBlock);
                }
            }
            catch
            {
            }
        }

        #endregion Request

        #region 小文件

        /// <inheritdoc/>
        public PullSmallFileResult PullSmallFile(string targetId, string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            if (this.DmtpActor.AllowRoute && this.TryFindDmtpFileTransferActor(targetId, out var actor))
            {
                return actor.PullSmallFile(path, metadata, timeout, token);
            }
            else
            {
                return this.PrivatePullSmallFile(targetId, path, metadata, timeout, token);
            }
        }

        /// <inheritdoc/>
        public PullSmallFileResult PullSmallFile(string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePullSmallFile(default, path, metadata, timeout, token);
        }

        /// <inheritdoc/>
        public Task<PullSmallFileResult> PullSmallFileAsync(string targetId, string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullSmallFile(targetId, path, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Task<PullSmallFileResult> PullSmallFileAsync(string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PullSmallFile(path, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Result PushSmallFile(string targetId, string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            if (this.DmtpActor.AllowRoute && this.TryFindDmtpFileTransferActor(targetId, out var rpcActor))
            {
                return rpcActor.PushSmallFile(savePath, fileInfo, metadata, timeout, token);
            }
            else
            {
                return this.PrivatePushSmallFile(targetId, savePath, fileInfo, metadata, timeout, token);
            }
        }

        /// <inheritdoc/>
        public Result PushSmallFile(string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return this.PrivatePushSmallFile(default, savePath, fileInfo, metadata, timeout, token);
        }

        /// <inheritdoc/>
        public Task<Result> PushSmallFileAsync(string targetId, string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushSmallFile(targetId, savePath, fileInfo, metadata, timeout, token);
            });
        }

        /// <inheritdoc/>
        public Task<Result> PushSmallFileAsync(string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.PushSmallFile(savePath, fileInfo, metadata, timeout, token);
            });
        }

        private PullSmallFileResult PrivatePullSmallFile(string targetId, string path, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            var waitSmallFilePackage = new WaitSmallFilePackage()
            {
                Path = path,
                SourceId = this.DmtpActor.Id,
                TargetId = targetId,
                Route = targetId.HasValue(),
                Metadata = metadata
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(waitSmallFilePackage);

            var byteBlock = new ByteBlock();
            try
            {
                waitSmallFilePackage.Package(byteBlock);
                this.DmtpActor.Send(this.m_pullSmallFile_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitSmallFilePackage)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        return new PullSmallFileResult(waitFile.Data);
                                    }
                                case TouchSocketDmtpStatus.FileNotExists:
                                    {
                                        return new PullSmallFileResult(ResultCode.Error, TouchSocketDmtpStatus.FileNotExists.GetDescription(waitFile.Path));
                                    }
                                case TouchSocketDmtpStatus.RemoteRefuse:
                                case TouchSocketDmtpStatus.LengthErrorWhenRead:
                                case TouchSocketDmtpStatus.FileLengthTooLong:
                                case TouchSocketDmtpStatus.ClientNotFind:
                                default:
                                    {
                                        return new PullSmallFileResult(ResultCode.Error, TouchSocketDmtpStatus.RemoteRefuse.GetDescription(waitFile.Message));
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return new PullSmallFileResult(ResultCode.Overtime);
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new PullSmallFileResult(ResultCode.Canceled);
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new PullSmallFileResult(ResultCode.Error, TouchSocketDmtpStatus.UnknownError.GetDescription());
                        }
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private Result PrivatePushSmallFile(string targetId, string savePath, FileInfo fileInfo, Metadata metadata = null, int timeout = 5000, CancellationToken token = default)
        {
            if (!File.Exists(fileInfo.FullName))
            {
                return new Result(ResultCode.Error, TouchSocketDmtpStatus.FileNotExists.GetDescription(fileInfo.FullName));
            }
            if (fileInfo.Length > this.MaxSmallFileLength)
            {
                return new Result(ResultCode.Error, TouchSocketDmtpStatus.FileLengthTooLong.GetDescription());
            }
            var waitSmallFilePackage = new WaitSmallFilePackage()
            {
                Path = savePath,
                SourceId = this.DmtpActor.Id,
                Metadata = metadata,
                Route = targetId.HasValue(),
                TargetId = targetId,
                FileInfo = new RemoteFileInfo(fileInfo)
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitData(waitSmallFilePackage);

            var byteBlock = new ByteBlock();
            var buffer = BytePool.Default.Rent((int)fileInfo.Length);
            try
            {
                var r = this.FileController.ReadAllBytes(fileInfo, buffer);
                if (r <= 0)
                {
                    return new Result(ResultCode.Error, TouchSocketDmtpStatus.LengthErrorWhenRead.GetDescription());
                }
                waitSmallFilePackage.Data = buffer;
                waitSmallFilePackage.Len = r;

                waitSmallFilePackage.Package(byteBlock);
                this.DmtpActor.Send(this.m_pushSmallFile_Request, byteBlock);
                waitData.SetCancellationToken(token);

                waitData.Wait(timeout);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var waitFile = (WaitSmallFilePackage)waitData.WaitResult;
                            switch (waitFile.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        return Result.Success;
                                    }
                                case TouchSocketDmtpStatus.RemoteRefuse:
                                    {
                                        return new Result(ResultCode.Error, TouchSocketDmtpStatus.RemoteRefuse.GetDescription(waitFile.Message));
                                    }
                                case TouchSocketDmtpStatus.ClientNotFind:
                                    {
                                        return new Result(ResultCode.Error, TouchSocketDmtpStatus.ClientNotFind.GetDescription());
                                    }
                                default:
                                    return new Result(ResultCode.Exception, waitFile.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return Result.Overtime;
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return Result.Canceled;
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return Result.UnknownFail;
                        }
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
                BytePool.Default.Return(buffer);
            }
        }

        private async Task RequestPullSmallFile(object o)
        {
            //2.不响应
            //3.不允许
            //4.不存在
            //5.读取文件长度异常

            var buffer = BytePool.Default.Rent(this.MaxSmallFileLength);
            try
            {
                var waitSmallFilePackage = (WaitSmallFilePackage)o;
                var resultThis = Result.Success;
                try
                {
                    var args = new FileTransferingEventArgs(TransferType.SmallPull, waitSmallFilePackage.Metadata, default)
                    {
                        ResourcePath = waitSmallFilePackage.Path
                    };

                    await this.OnFileTransfering.Invoke(this.DmtpActor, args);

                    var fullPath = this.GetFullPath(args.ResourcePath);
                    waitSmallFilePackage.Path = fullPath;
                    if (args.IsPermitOperation)
                    {
                        if (File.Exists(fullPath))
                        {
                            var fileInfo = new FileInfo(fullPath);
                            if (fileInfo.Length > this.MaxSmallFileLength)
                            {
                                waitSmallFilePackage.Status = TouchSocketDmtpStatus.FileLengthTooLong.ToValue();
                                resultThis = new Result(ResultCode.Error, TouchSocketDmtpStatus.FileLengthTooLong.GetDescription());
                            }
                            else
                            {
                                var r = this.FileController.ReadAllBytes(fileInfo, buffer);
                                if (r > 0)
                                {
                                    waitSmallFilePackage.Data = buffer;
                                    waitSmallFilePackage.Len = r;
                                    waitSmallFilePackage.FileInfo = new RemoteFileInfo(fileInfo);
                                    waitSmallFilePackage.Status = TouchSocketDmtpStatus.Success.ToValue();
                                }
                                else
                                {
                                    waitSmallFilePackage.Status = TouchSocketDmtpStatus.LengthErrorWhenRead.ToValue();
                                    resultThis = new Result(ResultCode.Error, TouchSocketDmtpStatus.LengthErrorWhenRead.GetDescription());
                                }
                            }
                        }
                        else
                        {
                            waitSmallFilePackage.Status = TouchSocketDmtpStatus.FileNotExists.ToValue();
                            resultThis = new Result(ResultCode.Error, TouchSocketDmtpStatus.FileNotExists.GetDescription(fullPath));
                        }
                    }
                    else
                    {
                        waitSmallFilePackage.Status = TouchSocketDmtpStatus.RemoteRefuse.ToValue();
                        waitSmallFilePackage.Message = args.Message;
                        resultThis = new Result(ResultCode.Error, TouchSocketDmtpStatus.RemoteRefuse.GetDescription(args.Message));
                    }
                }
                catch (Exception ex)
                {
                    waitSmallFilePackage.Status = TouchSocketDmtpStatus.Exception.ToValue();
                    waitSmallFilePackage.Message = ex.Message;
                }

                using (var byteBlock = new ByteBlock())
                {
                    waitSmallFilePackage.SwitchId();
                    waitSmallFilePackage.Package(byteBlock);
                    this.DmtpActor.Send(this.m_pullSmallFile_Response, byteBlock);
                }

                var resultArgs = new FileTransferedEventArgs(
                    TransferType.SmallPull, waitSmallFilePackage.Metadata, waitSmallFilePackage.FileInfo, resultThis)
                {
                    ResourcePath = waitSmallFilePackage.Path
                };
                await this.OnFileTransfered.Invoke(this.DmtpActor, resultArgs);
            }
            catch (Exception ex)
            {
                this.DmtpActor.Logger.Exception(ex);
            }
            finally
            {
                BytePool.Default.Return(buffer);
            }
        }

        private async Task RequestPushSmallFile(object o)
        {
            //2.不响应
            //3.不允许

            try
            {
                var waitSmallFilePackage = (WaitSmallFilePackage)o;
                var resultThis = Result.Success;
                try
                {
                    var args = new FileTransferingEventArgs(TransferType.SmallPush,
                        waitSmallFilePackage.Metadata, waitSmallFilePackage.FileInfo)
                    {
                        SavePath = waitSmallFilePackage.Path
                    };

                    await this.OnFileTransfering.Invoke(this.DmtpActor, args);

                    var fullPath = this.GetFullPath(args.SavePath);
                    waitSmallFilePackage.Path = fullPath;
                    if (args.IsPermitOperation)
                    {
                        this.FileController.WriteAllBytes(fullPath, waitSmallFilePackage.Data, 0, waitSmallFilePackage.Data.Length);
                        waitSmallFilePackage.Status = TouchSocketDmtpStatus.Success.ToValue();
                    }
                    else
                    {
                        waitSmallFilePackage.Status = TouchSocketDmtpStatus.RemoteRefuse.ToValue();
                        waitSmallFilePackage.Message = args.Message;
                        resultThis = new Result(ResultCode.Error, TouchSocketDmtpStatus.RemoteRefuse.GetDescription(args.Message));
                    }
                }
                catch (Exception ex)
                {
                    waitSmallFilePackage.Status = TouchSocketDmtpStatus.Exception.ToValue();
                    waitSmallFilePackage.Message = ex.Message;
                }
                waitSmallFilePackage.FileInfo = default;
                waitSmallFilePackage.Data = default;
                waitSmallFilePackage.SwitchId();
                using (var byteBlock = new ByteBlock())
                {
                    waitSmallFilePackage.Package(byteBlock);
                    this.DmtpActor.Send(this.m_pushSmallFile_Response, byteBlock);
                }

                var resultArgs = new FileTransferedEventArgs(
                    TransferType.SmallPush, waitSmallFilePackage.Metadata, waitSmallFilePackage.FileInfo, resultThis)
                {
                    SavePath = waitSmallFilePackage.Path
                };
                await this.OnFileTransfered.Invoke(this.DmtpActor, resultArgs);
            }
            catch (Exception ex)
            {
                this.DmtpActor.Logger.Exception(ex);
            }
        }

        #endregion 小文件
    }
}
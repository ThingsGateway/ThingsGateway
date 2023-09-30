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

using System.Collections.Concurrent;
using System.Text;

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// 提供Dmtp协议的最基础功能件
    /// </summary>
    public abstract class DmtpActor : DependencyObject, IDmtpActor
    {
        #region 委托

        /// <summary>
        /// 请求关闭
        /// </summary>
        public Action<DmtpActor, string> OnClose { get; set; }

        /// <summary>
        /// 当创建通道时
        /// </summary>
        public Action<DmtpActor, CreateChannelEventArgs> OnCreateChannel { get; set; }

        /// <summary>
        /// 查找其他IDmtpActor
        /// </summary>
        public Func<string, IDmtpActor> OnFindDmtpActor { get; set; }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        public Action<DmtpActor, DmtpVerifyEventArgs> OnHandshaked { get; set; }

        /// <summary>
        /// 握手
        /// </summary>
        public Action<DmtpActor, DmtpVerifyEventArgs> OnHandshaking { get; set; }

        /// <summary>
        /// 重设Id
        /// </summary>
        public Action<DmtpActor, WaitSetId> OnResetId { get; set; }

        /// <summary>
        /// 当需要路由的时候
        /// </summary>
        public Action<DmtpActor, PackageRouterEventArgs> OnRouting { get; set; }

        /// <summary>
        /// 发送数据接口
        /// </summary>
        public Action<DmtpActor, ArraySegment<byte>[]> OutputSend { get; set; }

        #endregion 委托

        #region 属性

        /// <inheritdoc/>
        public bool AllowRoute { get; }

        /// <inheritdoc/>
        public IDmtpActorObject Client { get; set; }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public bool IsHandshaked { get; protected set; }

        /// <inheritdoc/>
        public bool IsReliable { get; }

        /// <inheritdoc/>
        public DateTime LastActiveTime { get; protected set; }

        /// <inheritdoc/>
        public ILog Logger { get; set; }

        /// <inheritdoc/>
        public object SyncRoot { get; } = new object();

        /// <inheritdoc/>
        public WaitHandlePool<IWaitResult> WaitHandlePool { get; protected set; }

        #endregion 属性

        /// <summary>
        /// 创建一个Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute">是否允许路由</param>
        /// <param name="isReliable">是不是基于可靠协议运行的</param>
        public DmtpActor(bool allowRoute, bool isReliable)
        {
            this.WaitHandlePool = new WaitHandlePool<IWaitResult>();
            this.AllowRoute = allowRoute;
            this.LastActiveTime = DateTime.Now;
            this.IsReliable = isReliable;
        }

        /// <summary>
        /// 创建一个可靠协议的Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute"></param>
        public DmtpActor(bool allowRoute) : this(allowRoute, true)
        {
        }

        /// <inheritdoc/>
        public virtual void Close(bool sendClose, string message)
        {
            try
            {
                if (this.IsHandshaked)
                {
                    try
                    {
                        if (sendClose)
                        {
                            this.SendString(0, message);
                        }
                    }
                    catch
                    {
                    }
                    this.IsHandshaked = false;
                    this.WaitHandlePool.CancelAll();
                    this.OnClose?.Invoke(this, message);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 建立对点
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TokenVerifyException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public virtual void Handshake(string verifyToken, string id, int timeout, Metadata metadata, CancellationToken cancellationToken)
        {
            if (this.IsHandshaked)
            {
                return;
            }

            var args = new DmtpVerifyEventArgs()
            {
                Token = verifyToken,
                Id = id,
                Metadata = metadata
            };

            this.OnHandshaking?.Invoke(this, args);

            var waitVerify = new WaitVerify()
            {
                Token = args.Token,
                Id = args.Id,
                Metadata = args.Metadata
            };

            var waitData = this.WaitHandlePool.GetWaitData(waitVerify);
            waitData.SetCancellationToken(cancellationToken);

            try
            {
                this.SendJsonObject(P1_Handshake_Request, waitVerify);
                switch (waitData.Wait(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var verifyResult = (WaitVerify)waitData.WaitResult;
                            if (verifyResult.Status == 1)
                            {
                                this.Id = verifyResult.Id;
                                this.IsHandshaked = true;
                                this.PrivateHandshaked(new DmtpVerifyEventArgs()
                                {
                                    Id = verifyResult.Id,
                                    Metadata = verifyResult.Metadata,
                                    Token = verifyResult.Token,
                                });
                                verifyResult.Handle = true;
                                break;
                            }
                            else
                            {
                                verifyResult.Handle = true;
                                this.Close(false, verifyResult.Message);
                                throw new TokenVerifyException(verifyResult.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        this.Close(false, TouchSocketDmtpStatus.Overtime.GetDescription());
                        throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        this.Close(false, null);
                        return;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <summary>
        /// 建立对点
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TokenVerifyException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public virtual async Task HandshakeAsync(string verifyToken, string id, int timeout, Metadata metadata, CancellationToken cancellationToken)
        {
            if (this.IsHandshaked)
            {
                return;
            }
            var args = new DmtpVerifyEventArgs()
            {
                Token = verifyToken,
                Id = id,
                Metadata = metadata
            };

            this.OnHandshaking?.Invoke(this, args);

            var waitVerify = new WaitVerify()
            {
                Token = args.Token,
                Id = args.Id,
                Metadata = args.Metadata
            };
            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitVerify);
            waitData.SetCancellationToken(cancellationToken);

            try
            {
                this.SendJsonObject(P1_Handshake_Request, waitVerify);
                switch (await waitData.WaitAsync(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var verifyResult = (WaitVerify)waitData.WaitResult;
                            if (verifyResult.Status == 1)
                            {
                                this.Id = verifyResult.Id;
                                this.IsHandshaked = true;
                                this.PrivateHandshaked(new DmtpVerifyEventArgs()
                                {
                                    Id = verifyResult.Id,
                                    Metadata = verifyResult.Metadata,
                                    Token = verifyResult.Token,
                                });
                                verifyResult.Handle = true;
                                break;
                            }
                            else
                            {
                                verifyResult.Handle = true;
                                this.Close(false, verifyResult.Message);
                                throw new TokenVerifyException(verifyResult.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        this.Close(false, TouchSocketDmtpStatus.Overtime.GetDescription());
                        throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        this.Close(false, null);
                        return;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

        #region const

        /// <summary>
        /// Close
        /// </summary>
        public const ushort P0_Close = 0;

        /// <summary>
        /// Handshake_Request
        /// </summary>
        public const ushort P1_Handshake_Request = 1;

        /// <summary>
        /// Handshake_Response
        /// </summary>
        public const ushort P2_Handshake_Response = 2;

        /// <summary>
        /// ResetId_Request
        /// </summary>
        public const ushort P3_ResetId_Request = 3;

        /// <summary>
        /// ResetId_Response
        /// </summary>
        public const ushort P4_ResetId_Response = 4;

        /// <summary>
        /// Ping_Request
        /// </summary>
        public const ushort P5_Ping_Request = 5;

        /// <summary>
        /// Ping_Response
        /// </summary>
        public const ushort P6_Ping_Response = 6;

        /// <summary>
        /// CreateChannel_Request
        /// </summary>
        public const ushort P7_CreateChannel_Request = 7;

        /// <summary>
        /// CreateChannel_Response
        /// </summary>
        public const ushort P8_CreateChannel_Response = 8;

        /// <summary>
        /// ChannelPackage
        /// </summary>
        public const ushort P9_ChannelPackage = 9;

        #endregion const

        /// <summary>
        /// 处理接收数据.
        /// <para>
        /// <list type="table">
        /// <item>0：Close</item>
        /// <item>1：Handshake_Request</item>
        /// <item>2：Handshake_Response</item>
        /// <item>3：ResetId_Request</item>
        /// <item>4：ResetId_Response</item>
        /// <item>5：Ping_Request</item>
        /// <item>6：Ping_Response</item>
        /// <item>7：CreateChannel_Request</item>
        /// <item>8：CreateChannel_Response</item>
        /// <item>9：ChannelPackage</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual bool InputReceivedData(DmtpMessage message)
        {
            this.LastActiveTime = DateTime.Now;
            var byteBlock = message.BodyByteBlock;
            switch (message.ProtocolFlags)
            {
                case P0_Close:
                    {
                        this.Close(false, message.GetBodyString());
                        return true;
                    }
                case P1_Handshake_Request:
                    {
                        try
                        {
                            var waitVerify = this.ResolveJsonObject<WaitVerify>(message.GetBodyString());

                            var args = new DmtpVerifyEventArgs()
                            {
                                Token = waitVerify.Token,
                                Metadata = waitVerify.Metadata,
                                Id = waitVerify.Id,
                            };
                            this.OnHandshaking?.Invoke(this, args);

                            if (args.Id.HasValue())
                            {
                                this.OnResetId?.Invoke(this, new WaitSetId(this.Id, args.Id));
                                this.Id = args.Id;
                            }

                            if (args.IsPermitOperation)
                            {
                                waitVerify.Id = this.Id;
                                waitVerify.Status = 1;
                                this.SendJsonObject(P2_Handshake_Response, waitVerify);
                                this.IsHandshaked = true;
                                args.Message = "Success";

                                Task.Factory.StartNew(this.PrivateHandshaked, args);
                            }
                            else//不允许连接
                            {
                                waitVerify.Status = 2;
                                waitVerify.Message = args.Message;
                                this.SendJsonObject(P2_Handshake_Response, waitVerify);
                                this.Close(false, args.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                            this.Close(false, ex.Message);
                        }
                        return true;
                    }
                case P2_Handshake_Response:
                    {
                        try
                        {
                            var waitVerify = this.ResolveJsonObject<WaitVerify>(message.GetBodyString());
                            this.WaitHandlePool.SetRun(waitVerify);
                            SpinWait.SpinUntil(() =>
                            {
                                return waitVerify.Handle;
                            }, 5000);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }

                        return true;
                    }
                case P3_ResetId_Request:
                    {
                        try
                        {
                            var waitSetId = this.ResolveJsonObject<WaitSetId>(message.GetBodyString());
                            try
                            {
                                this.OnResetId?.Invoke(this, waitSetId);
                                this.Id = waitSetId.NewId;
                                waitSetId.Status = 1;
                            }
                            catch (Exception ex)
                            {
                                waitSetId.Status = 2;
                                waitSetId.Message = ex.Message;
                            }
                            this.SendJsonObject(P4_ResetId_Response, waitSetId);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P4_ResetId_Response:
                    {
                        try
                        {
                            this.WaitHandlePool.SetRun(this.ResolveJsonObject<WaitSetId>(message.GetBodyString()));
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P5_Ping_Request://心跳
                    {
                        try
                        {
                            var waitPing = this.ResolveJsonObject<WaitPing>(message.GetBodyString());

                            if (this.AllowRoute && waitPing.Route)
                            {
                                if (this.TryRoute(RouteType.Ping, waitPing))
                                {
                                    if (this.TryFindDmtpActor(waitPing.TargetId, out var actor))
                                    {
                                        actor.Send(P5_Ping_Request, byteBlock);
                                        return true;
                                    }
                                    else
                                    {
                                        waitPing.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                                    }
                                }
                                else
                                {
                                    waitPing.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                                }
                            }
                            else
                            {
                                waitPing.Status = TouchSocketDmtpStatus.Success.ToValue();
                            }
                            waitPing.SwitchId();
                            this.SendJsonObject(P6_Ping_Response, waitPing);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P6_Ping_Response://心跳
                    {
                        try
                        {
                            var waitPing = this.ResolveJsonObject<WaitPing>(message.GetBodyString());

                            if (this.AllowRoute && waitPing.Route)
                            {
                                if (this.TryFindDmtpActor(waitPing.TargetId, out var actor))
                                {
                                    actor.Send(P6_Ping_Response, byteBlock);
                                }
                            }
                            else
                            {
                                this.WaitHandlePool.SetRun(waitPing);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P7_CreateChannel_Request:
                    {
                        try
                        {
                            var waitCreateChannel = new WaitCreateChannelPackage();
                            waitCreateChannel.UnpackageRouter(byteBlock);
                            if (this.AllowRoute && waitCreateChannel.Route)
                            {
                                if (this.TryRoute(RouteType.CreateChannel, waitCreateChannel))
                                {
                                    if (this.TryFindDmtpActor(waitCreateChannel.TargetId, out var actor))
                                    {
                                        actor.Send(P7_CreateChannel_Request, byteBlock);
                                        return true;
                                    }
                                    else
                                    {
                                        waitCreateChannel.UnpackageBody(byteBlock);
                                        waitCreateChannel.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                                    }
                                }
                                else
                                {
                                    waitCreateChannel.UnpackageBody(byteBlock);
                                    waitCreateChannel.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                                }
                            }
                            else
                            {
                                waitCreateChannel.UnpackageBody(byteBlock);

                                while (true)
                                {
                                    if (this.RequestCreateChannel(waitCreateChannel.ChannelId, waitCreateChannel.Route ? waitCreateChannel.SourceId : waitCreateChannel.TargetId, waitCreateChannel.Metadata))
                                    {
                                        waitCreateChannel.Status = TouchSocketDmtpStatus.Success.ToValue();
                                        break;
                                    }
                                    else
                                    {
                                        waitCreateChannel.Status = TouchSocketDmtpStatus.ChannelExisted.ToValue();
                                    }

                                    if (!waitCreateChannel.Random)
                                    {
                                        break;
                                    }
                                    waitCreateChannel.ChannelId = new object().GetHashCode();
                                }
                            }
                            waitCreateChannel.SwitchId();
                            byteBlock.Reset();
                            waitCreateChannel.Package(byteBlock);
                            this.Send(P8_CreateChannel_Response, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P8_CreateChannel_Response:
                    {
                        try
                        {
                            var waitCreateChannel = new WaitCreateChannelPackage();
                            waitCreateChannel.UnpackageRouter(byteBlock);
                            if (this.AllowRoute && waitCreateChannel.Route)
                            {
                                if (this.TryFindDmtpActor(waitCreateChannel.TargetId, out var actor))
                                {
                                    actor.Send(P8_CreateChannel_Response, byteBlock);
                                    return true;
                                }
                            }
                            else
                            {
                                waitCreateChannel.UnpackageBody(byteBlock);
                                this.WaitHandlePool.SetRun(waitCreateChannel);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P9_ChannelPackage:
                    {
                        try
                        {
                            var channelPackage = new ChannelPackage();
                            channelPackage.UnpackageRouter(byteBlock);
                            if (this.AllowRoute && channelPackage.Route)
                            {
                                if (this.TryFindDmtpActor(channelPackage.TargetId, out var actor))
                                {
                                    actor.Send(P9_ChannelPackage, byteBlock);
                                }
                                else
                                {
                                    channelPackage.UnpackageBody(byteBlock);
                                    channelPackage.Message = TouchSocketDmtpStatus.ClientNotFind.GetDescription(channelPackage.TargetId);
                                    channelPackage.SwitchId();
                                    channelPackage.RunNow = true;
                                    channelPackage.DataType = ChannelDataType.DisposeOrder;
                                    byteBlock.Reset();
                                    channelPackage.Package(byteBlock);
                                    this.Send(P9_ChannelPackage, byteBlock);
                                }
                            }
                            else
                            {
                                channelPackage.UnpackageBody(byteBlock);
                                this.QueueChannelPackage(channelPackage);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                default:
                    {
                        if (message.ProtocolFlags < 20)
                        {
                            return true;
                        }
                        return false;
                    }
            }
        }

        /// <inheritdoc/>
        public virtual bool Ping(int timeout = 5000)
        {
            return this.PrivatePing(default, timeout);
        }

        /// <inheritdoc/>
        public virtual bool Ping(string targetId, int timeout = 5000)
        {
            if (this.AllowRoute && this.TryFindDmtpActor(targetId, out var actor))
            {
                return actor.Ping(timeout);
            }
            return this.PrivatePing(targetId, timeout);
        }

        /// <inheritdoc/>
        public virtual Task<bool> PingAsync(int timeout = 5000)
        {
            return this.PrivatePingAsync(default, timeout);
        }

        /// <inheritdoc/>
        public virtual Task<bool> PingAsync(string targetId, int timeout = 5000)
        {
            if (this.AllowRoute && this.TryFindDmtpActor(targetId, out var actor))
            {
                return actor.PingAsync(timeout);
            }
            else
            {
                return this.PrivatePingAsync(targetId, timeout);
            }
        }

        /// <inheritdoc/>
        public virtual void ResetId(string id)
        {
            var waitSetId = new WaitSetId(this.Id, id);

            var waitData = this.WaitHandlePool.GetWaitData(waitSetId);

            this.SendJsonObject(P3_ResetId_Request, waitSetId);

            switch (waitData.Wait(5000))
            {
                case WaitDataStatus.SetRunning:
                    {
                        if (waitData.WaitResult.Status == 1)
                        {
                            this.OnResetId?.Invoke(this, new WaitSetId(this.Id, id));
                            this.Id = id;
                        }
                        else
                        {
                            throw new Exception(waitData.WaitResult.Message);
                        }

                        break;
                    }
                case WaitDataStatus.Overtime:
                    throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                case WaitDataStatus.Canceled:
                    break;

                case WaitDataStatus.Disposed:
                default:
                    throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
            }
        }

        /// <inheritdoc/>
        public virtual async Task ResetIdAsync(string id)
        {
            var waitSetId = new WaitSetId(this.Id, id);

            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitSetId);

            this.SendJsonObject(P3_ResetId_Request, waitSetId);

            switch (await waitData.WaitAsync(5000))
            {
                case WaitDataStatus.SetRunning:
                    {
                        if (waitData.WaitResult.Status == 1)
                        {
                            this.OnResetId?.Invoke(this, new WaitSetId(this.Id, id));
                            this.Id = id;
                        }
                        else
                        {
                            throw new Exception(waitData.WaitResult.Message);
                        }
                        break;
                    }
                case WaitDataStatus.Overtime:
                    throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                case WaitDataStatus.Canceled:
                    break;

                case WaitDataStatus.Disposed:
                default:
                    throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
            }
        }

        /// <inheritdoc/>
        public virtual void SendFastObject<T>(ushort protocol, in T obj)
        {
            using (var byteBlock = new ByteBlock())
            {
                byteBlock.WriteObject(obj, SerializationType.FastBinary);
                this.Send(protocol, byteBlock);
            }
        }

        private void SendJsonObject<T>(ushort protocol, in T obj)
        {
#if NET6_0_OR_GREATER
            var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj, typeof(T), TouchSokcetDmtpSourceGenerationContext.Default);
#else
            var bytes = SerializeConvert.JsonSerializeToBytes(obj);
#endif

            this.Send(protocol, bytes, 0, bytes.Length);
        }

        private T ResolveJsonObject<T>(string json)
        {
#if NET6_0_OR_GREATER

            return (T)System.Text.Json.JsonSerializer.Deserialize(json, typeof(T), TouchSokcetDmtpSourceGenerationContext.Default);
#else
            return SerializeConvert.FromJsonString<T>(json);
#endif
        }

        /// <inheritdoc/>
        public virtual void SendPackage(ushort protocol, in IPackage package)
        {
            using (var byteBlock = new ByteBlock())
            {
                package.Package(byteBlock);
                this.Send(protocol, byteBlock);
            }
        }

        /// <inheritdoc/>
        public virtual void SendString(ushort protocol, string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            this.Send(protocol, data, 0, data.Length);
        }

        /// <inheritdoc/>
        public virtual Task SendStringAsync(ushort protocol, string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            return this.SendAsync(protocol, data, 0, data.Length);
        }

        /// <inheritdoc/>
        public virtual bool TryFindDmtpActor(string targetId, out DmtpActor actor)
        {
            if (targetId == this.Id)
            {
                actor = this;
                return true;
            }
            if (this.OnFindDmtpActor?.Invoke(targetId) is DmtpActor newActor)
            {
                actor = newActor;
                return true;
            }

            actor = default;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool TryRoute(RouteType routerType, RouterPackage routerPackage)
        {
            try
            {
                var args = new PackageRouterEventArgs(routerType, routerPackage);
                this.OnRouting?.Invoke(this, args);
                return args.IsPermitOperation;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual bool TryRoute(RouteType routerType, WaitRouterPackage routerPackage)
        {
            try
            {
                var args = new PackageRouterEventArgs(routerType, routerPackage);
                this.OnRouting?.Invoke(this, args);
                routerPackage.Message = args.Message;
                return args.IsPermitOperation;
            }
            catch
            {
                return false;
            }
        }

        private void PrivateHandshaked(object obj)
        {
            this.OnHandshaked?.Invoke(this, (DmtpVerifyEventArgs)obj);
        }

        private bool PrivatePing(string targetId, int timeout)
        {
            var waitPing = new WaitPing
            {
                TargetId = targetId,
                SourceId = Id,
                Route = targetId.HasValue()
            };
            var waitData = this.WaitHandlePool.GetWaitData(waitPing);
            try
            {
                this.SendJsonObject(P5_Ping_Request, waitPing);
                switch (waitData.Wait(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            switch (waitData.WaitResult.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success: return true;
                                default:
                                    return false;
                            }
                        }
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Overtime:
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

        private async Task<bool> PrivatePingAsync(string targetId, int timeout)
        {
            var waitPing = new WaitPing
            {
                TargetId = targetId,
                SourceId = Id,
                Route = targetId.HasValue()
            };
            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitPing);
            try
            {
                this.SendJsonObject(P5_Ping_Request, waitPing);
                switch (await waitData.WaitAsync(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            switch (waitData.WaitResult.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success: return true;
                                default:
                                    return false;
                            }
                        }
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Overtime:
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

        #region 重写

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.OnClose = null;
            this.OnRouting = null;
            this.OnFindDmtpActor = null;
            this.OnHandshaked = null;
            this.OnHandshaking = null;
            this.OnResetId = null;

            this.OutputSend = null;
            this.WaitHandlePool.SafeDispose();
            this.Close(false, nameof(Dispose));
            base.Dispose(disposing);
        }

        #endregion 重写

        #region 协议同步发送

        /// <inheritdoc/>
        public virtual void Send(ushort protocol, byte[] buffer, int offset, int length)
        {
            var transferBytes = new ArraySegment<byte>[]
           {
            new ArraySegment<byte>(TouchSocketBitConverter.BigEndian.GetBytes(protocol)),
            new ArraySegment<byte>(TouchSocketBitConverter.BigEndian.GetBytes(length)),
            new ArraySegment<byte>(buffer,offset,length)
           };
            this.OutputSend?.Invoke(this, transferBytes);
            this.LastActiveTime = DateTime.Now;
        }

        /// <inheritdoc/>
        public virtual void Send(ushort protocol, ByteBlock byteBlock)
        {
            this.Send(protocol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 协议同步发送

        #region 协议异步发送

        /// <inheritdoc/>
        public virtual Task SendAsync(ushort protocol, byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.Send(protocol, buffer, offset, length);
            });
        }

        #endregion 协议异步发送

        #region IDmtpChannel

        private readonly ConcurrentDictionary<int, InternalChannel> m_userChannels = new ConcurrentDictionary<int, InternalChannel>();

        /// <inheritdoc/>
        public virtual bool ChannelExisted(int id)
        {
            return this.m_userChannels.ContainsKey(id);
        }

        /// <inheritdoc/>
        public virtual IDmtpChannel CreateChannel(Metadata metadata = default)
        {
            return this.PrivateCreateChannel(default, true, 0, metadata);
        }

        /// <inheritdoc/>
        public virtual IDmtpChannel CreateChannel(int id, Metadata metadata = default)
        {
            return this.PrivateCreateChannel(default, false, id, metadata);
        }

        /// <inheritdoc/>
        public virtual IDmtpChannel CreateChannel(string targetId, int id, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }
            if (this.AllowRoute && this.TryFindDmtpActor(targetId, out var actor))
            {
                return actor.CreateChannel(id, metadata);
            }
            else
            {
                return this.PrivateCreateChannel(targetId, false, id, metadata);
            }
        }

        /// <inheritdoc/>
        public virtual IDmtpChannel CreateChannel(string targetId, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (this.AllowRoute && this.TryFindDmtpActor(targetId, out var actor))
            {
                return actor.CreateChannel(metadata);
            }
            else
            {
                return this.PrivateCreateChannel(targetId, true, 0, metadata);
            }
        }

        /// <inheritdoc/>
        public virtual Task<IDmtpChannel> CreateChannelAsync(Metadata metadata = default)
        {
            return this.PrivateCreateChannelAsync(default, true, 0, metadata);
        }

        /// <inheritdoc/>
        public virtual Task<IDmtpChannel> CreateChannelAsync(int id, Metadata metadata = default)
        {
            return this.PrivateCreateChannelAsync(default, false, id, metadata);
        }

        /// <inheritdoc/>
        public virtual Task<IDmtpChannel> CreateChannelAsync(string targetId, int id, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }
            if (this.AllowRoute && this.TryFindDmtpActor(targetId, out var actor))
            {
                return actor.CreateChannelAsync(id, metadata);
            }
            else
            {
                return this.PrivateCreateChannelAsync(targetId, false, id, metadata);
            }
        }

        /// <inheritdoc/>
        public virtual Task<IDmtpChannel> CreateChannelAsync(string targetId, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (this.AllowRoute && this.TryFindDmtpActor(targetId, out var actor))
            {
                return actor.CreateChannelAsync(metadata);
            }
            else
            {
                return this.PrivateCreateChannelAsync(targetId, true, 0, metadata);
            }
        }

        /// <inheritdoc/>
        public virtual bool TrySubscribeChannel(int id, out IDmtpChannel channel)
        {
            if (this.m_userChannels.TryGetValue(id, out var channelOut))
            {
                if (channelOut.Using)
                {
                    channel = null;
                    return false;
                }
                channelOut.SetUsing();
                channel = channelOut;
                return true;
            }
            channel = null;
            return false;
        }

        internal bool RemoveChannel(int id)
        {
            return this.m_userChannels.TryRemove(id, out _);
        }

        internal void SendChannelPackage(ChannelPackage channelPackage)
        {
            using (var byteBlock = new ByteBlock(channelPackage.GetLen()))
            {
                channelPackage.Package(byteBlock);
                this.Send(P9_ChannelPackage, byteBlock);
            }
        }

        private IDmtpChannel PrivateCreateChannel(string targetId, bool random, int id, Metadata metadata)
        {
            if (random)
            {
                id = new object().GetHashCode();
            }
            else
            {
                if (this.ChannelExisted(id))
                {
                    throw new Exception(TouchSocketDmtpStatus.ChannelExisted.GetDescription(id));
                }
            }

            var byteBlock = new ByteBlock();
            var waitCreateChannel = new WaitCreateChannelPackage()
            {
                Random = random,
                ChannelId = id,
                SourceId = this.Id,
                TargetId = targetId,
                Metadata = metadata,
                Route = targetId.HasValue()
            };
            var waitData = this.WaitHandlePool.GetWaitData(waitCreateChannel);

            try
            {
                waitCreateChannel.Package(byteBlock);
                this.Send(P7_CreateChannel_Request, byteBlock);
                switch (waitData.Wait(10 * 1000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var result = (WaitCreateChannelPackage)waitData.WaitResult;
                            switch (result.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        var channel = new InternalChannel(this, targetId, result.Metadata);
                                        channel.SetId(result.ChannelId);
                                        channel.SetUsing();
                                        return this.m_userChannels.TryAdd(result.ChannelId, channel)
                                            ? (IDmtpChannel)channel
                                            : throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                                    }
                                case TouchSocketDmtpStatus.ClientNotFind:
                                    {
                                        throw new Exception(TouchSocketDmtpStatus.ClientNotFind.GetDescription(targetId));
                                    }
                                case TouchSocketDmtpStatus.RoutingNotAllowed:
                                default:
                                    {
                                        throw new Exception(result.Status.ToStatus().GetDescription(result.Message));
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                        }
                    default:
                        {
                            throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                        }
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private async Task<IDmtpChannel> PrivateCreateChannelAsync(string targetId, bool random, int id, Metadata metadata)
        {
            if (random)
            {
                id = new object().GetHashCode();
            }
            else
            {
                if (this.ChannelExisted(id))
                {
                    throw new Exception(TouchSocketDmtpStatus.ChannelExisted.GetDescription(id));
                }
            }

            var byteBlock = new ByteBlock();
            var waitCreateChannel = new WaitCreateChannelPackage()
            {
                Random = random,
                ChannelId = id,
                SourceId = this.Id,
                TargetId = targetId,
                Metadata = metadata,
                Route = targetId.HasValue()
            };
            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitCreateChannel);

            try
            {
                waitCreateChannel.Package(byteBlock);
                this.Send(P7_CreateChannel_Request, byteBlock);
                switch (await waitData.WaitAsync(10 * 1000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var result = (WaitCreateChannelPackage)waitData.WaitResult;
                            switch (result.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        var channel = new InternalChannel(this, targetId, result.Metadata);
                                        channel.SetId(result.ChannelId);
                                        channel.SetUsing();
                                        return this.m_userChannels.TryAdd(result.ChannelId, channel)
                                            ? (IDmtpChannel)channel
                                            : throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                                    }
                                case TouchSocketDmtpStatus.ClientNotFind:
                                    {
                                        throw new Exception(TouchSocketDmtpStatus.ClientNotFind.GetDescription(targetId));
                                    }
                                case TouchSocketDmtpStatus.RoutingNotAllowed:
                                default:
                                    {
                                        throw new Exception(result.Status.ToStatus().GetDescription(result.Message));
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                        }
                    default:
                        {
                            throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                        }
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private bool QueueChannelPackage(ChannelPackage channelPackage)
        {
            if (this.m_userChannels.TryGetValue(channelPackage.ChannelId, out var channel))
            {
                channel.ReceivedData(channelPackage);
                return true;
            }

            return false;
        }

        private bool RequestCreateChannel(int id, string targetId, Metadata metadata)
        {
            lock (this.SyncRoot)
            {
                var channel = new InternalChannel(this, targetId, metadata);
                channel.SetId(id);
                if (this.m_userChannels.TryAdd(id, channel))
                {
                    Task.Factory.StartNew(this.ThisRequestCreateChannel, new CreateChannelEventArgs(id, metadata));
                    return true;
                }
                else
                {
                    channel.SafeDispose();
                    return false;
                }
            }
        }

        private void ThisRequestCreateChannel(object state)
        {
            try
            {
                this.OnCreateChannel?.Invoke(this, (CreateChannelEventArgs)state);
            }
            catch
            {
            }
        }

        #endregion IDmtpChannel
    }
}
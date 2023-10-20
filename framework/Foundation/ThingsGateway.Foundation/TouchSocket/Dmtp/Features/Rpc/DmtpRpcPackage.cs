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
using ThingsGateway.Foundation.Resources;

namespace ThingsGateway.Foundation.Dmtp.Rpc
{
    /// <summary>
    /// Rpc传输类
    /// </summary>
    public sealed class DmtpRpcPackage : WaitRouterPackage
    {
        /// <inheritdoc/>
        protected override bool IncludedRouter => true;

        /// <summary>
        /// 反馈类型
        /// </summary>
        public FeedbackType Feedback { get; private set; }

        /// <summary>
        /// 参数是否包含引用类型
        /// </summary>
        public bool IsByRef { get; internal set; }

        /// <summary>
        /// 函数名
        /// </summary>
        public string MethodName { get; internal set; }

        /// <summary>
        /// 参数数据
        /// </summary>
        public List<byte[]> ParametersBytes { get; internal set; }

        /// <summary>
        /// 返回参数数据
        /// </summary>
        public byte[] ReturnParameterBytes { get; internal set; }

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializationType SerializationType { get; private set; }

        /// <inheritdoc/>
        public override void PackageBody(in ByteBlock byteBlock)
        {
            base.PackageBody(byteBlock);
            byteBlock.Write((byte)this.SerializationType);
            byteBlock.Write((byte)this.Feedback);
            byteBlock.Write(this.IsByRef);
            byteBlock.Write(this.MethodName);
            byteBlock.WriteBytesPackage(this.ReturnParameterBytes);

            if (this.ParametersBytes != null && this.ParametersBytes.Count > 0)
            {
                byteBlock.Write((byte)this.ParametersBytes.Count);
                foreach (var item in this.ParametersBytes)
                {
                    byteBlock.WriteBytesPackage(item);
                }
            }
            else
            {
                byteBlock.Write((byte)0);
            }
        }

        /// <inheritdoc/>
        public override void UnpackageBody(in ByteBlock byteBlock)
        {
            base.UnpackageBody(byteBlock);
            this.SerializationType = (SerializationType)byteBlock.ReadByte();
            this.Feedback = (FeedbackType)byteBlock.ReadByte();
            this.IsByRef = byteBlock.ReadBoolean();
            this.MethodName = byteBlock.ReadString();
            this.ReturnParameterBytes = byteBlock.ReadBytesPackage();

            var countPar = (byte)byteBlock.ReadByte();
            this.ParametersBytes = new List<byte[]>();
            for (var i = 0; i < countPar; i++)
            {
                this.ParametersBytes.Add(byteBlock.ReadBytesPackage());
            }
        }

        internal void LoadInvokeOption(IInvokeOption option)
        {
            if (option is DmtpInvokeOption dmtpInvokeOption)
            {
                this.Feedback = dmtpInvokeOption.FeedbackType;
                this.SerializationType = dmtpInvokeOption.SerializationType;
            }
            else if (option is InvokeOption invokeOption)
            {
                this.Feedback = invokeOption.FeedbackType;
            }
        }

        internal void ThrowStatus()
        {
            if (this.Status == 1)
            {
                return;
            }
            switch (this.Status.ToStatus())
            {
                case TouchSocketDmtpStatus.ClientNotFind:
                    {
                        throw new ClientNotFindException(TouchSocketDmtpStatus.ClientNotFind.GetDescription(this.TargetId));
                    }
                case TouchSocketDmtpStatus.UnknownError:
                case TouchSocketDmtpStatus.RpcMethodNotFind:
                case TouchSocketDmtpStatus.RpcMethodDisable:
                case TouchSocketDmtpStatus.RemoteRefuse:
                case TouchSocketDmtpStatus.RpcInvokeException:
                case TouchSocketDmtpStatus.RoutingNotAllowed:
                case TouchSocketDmtpStatus.Exception:
                default:
                    {
                        throw new RpcException(this.Status.ToStatus().GetDescription(this.Message));
                    }
            }
        }
    }
}
// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.IO.Ports;

namespace ThingsGateway.Foundation
{
    public abstract class ChannelOptionsBase : IValidatableObject
    {
        /// <summary>
        /// 通道类型
        /// </summary>
        public virtual ChannelTypeEnum ChannelType { get; set; }

        #region 以太网

        /// <summary>
        /// 远程ip
        /// </summary>
        [UriValidation]
        public virtual string RemoteUrl { get; set; } = "127.0.0.1:502";

        /// <summary>
        /// 本地绑定ip，分号分隔，例如：192.168.1.1:502;192.168.1.2:502，表示绑定192.168.1.1:502和192.168.1.2:502
        /// </summary>
        [UriValidation]
        public virtual string BindUrl { get; set; }

        #endregion

        #region 串口

        /// <summary>
        /// COM
        /// </summary>
        public virtual string PortName { get; set; } = "COM1";

        /// <summary>
        /// 波特率
        /// </summary>
        public virtual int BaudRate { get; set; } = 9600;

        /// <summary>
        /// 数据位
        /// </summary>
        public virtual int DataBits { get; set; } = 8;

        /// <summary>
        /// 校验位
        /// </summary>
        public virtual Parity Parity { get; set; } = System.IO.Ports.Parity.None;

        /// <summary>
        /// 停止位
        /// </summary>
        public virtual StopBits StopBits { get; set; } = System.IO.Ports.StopBits.One;

        /// <summary>
        /// DtrEnable
        /// </summary>
        public virtual bool DtrEnable { get; set; } = true;

        /// <summary>
        /// RtsEnable
        /// </summary>
        public virtual bool RtsEnable { get; set; } = true;

        /// <inheritdoc/>
        [MinValue(1)]
        public virtual int MaxConcurrentCount { get; set; } = 1;

        /// <inheritdoc/>
        [MinValue(100)]
        public virtual int CacheTimeout { get; set; } = 500;
        /// <inheritdoc/>
        [MinValue(100)]
        public virtual ushort ConnectTimeout { get; set; } = 3000;

        #endregion

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (ChannelType == ChannelTypeEnum.TcpClient)
            {
                if (string.IsNullOrEmpty(RemoteUrl))
                {
                    yield return new ValidationResult(DefaultResource.Localizer["RemoteUrlNotNull"], new[] { nameof(RemoteUrl) });
                }
            }
            else if (ChannelType == ChannelTypeEnum.TcpService)
            {
                if (string.IsNullOrEmpty(BindUrl))
                {
                    yield return new ValidationResult(DefaultResource.Localizer["BindUrlNotNull"], new[] { nameof(BindUrl) });
                }
            }
            else if (ChannelType == ChannelTypeEnum.UdpSession)
            {
                if (string.IsNullOrEmpty(BindUrl) && string.IsNullOrEmpty(RemoteUrl))
                {
                    yield return new ValidationResult(DefaultResource.Localizer["BindUrlOrRemoteUrlNotNull"], new[] { nameof(BindUrl), nameof(RemoteUrl) });
                }
            }
            else if (ChannelType == ChannelTypeEnum.SerialPort)
            {
                if (string.IsNullOrEmpty(PortName))
                {
                    yield return new ValidationResult(DefaultResource.Localizer["PortNameNotNull"], new[] { nameof(PortName) });
                }
            }

        }


        public override string ToString()
        {
            switch (ChannelType)
            {
                case ChannelTypeEnum.TcpClient:
                    return RemoteUrl;
                case ChannelTypeEnum.TcpService:
                    return BindUrl;
                case ChannelTypeEnum.SerialPort:
                    return PortName;
                case ChannelTypeEnum.UdpSession:
                    return RemoteUrl;
                case ChannelTypeEnum.Other:
                    return string.Empty;
            }
            return string.Empty;
        }
    }
}
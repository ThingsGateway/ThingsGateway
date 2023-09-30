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
using System.Text;

namespace ThingsGateway.Foundation.Http.WebSockets
{
    /// <summary>
    /// WebSocket数据帧
    /// </summary>
    public class WSDataFrame : DisposableObject, IRequestInfo
    {
        private int m_payloadLength;

        /// <summary>
        /// 是否为最后数据帧。
        /// </summary>
        public bool FIN { get; set; }

        /// <summary>
        /// 计算掩码
        /// </summary>
        public bool Mask { get; set; }

        /// <summary>
        /// 掩码值
        /// </summary>
        public byte[] MaskingKey { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public WSDataType Opcode { get; set; }

        /// <summary>
        /// 有效数据
        /// </summary>
        public ByteBlock PayloadData { get; set; }

        /// <summary>
        /// 有效载荷数据长度
        /// </summary>
        public int PayloadLength
        {
            get
            {
                if (this.m_payloadLength != 0)
                {
                    return this.m_payloadLength;
                }
                return this.PayloadData != null ? this.PayloadData.Len : 0;
            }

            set
            {
                this.m_payloadLength = value;
            }
        }

        /// <summary>
        /// 标识RSV-1。
        /// </summary>
        public bool RSV1 { get; set; }

        /// <summary>
        /// 标识RSV-2。
        /// </summary>
        public bool RSV2 { get; set; }

        /// <summary>
        /// 标识RSV-3。
        /// </summary>
        public bool RSV3 { get; set; }

        /// <summary>
        /// 构建数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public bool Build(ByteBlock byteBlock)
        {
            if (this.PayloadData == null)
            {
                return WSTools.Build(byteBlock, this, new byte[0], 0, 0);
            }
            else
            {
                return WSTools.Build(byteBlock, this, this.PayloadData.Buffer, 0, this.PayloadLength);
            }
        }

        /// <summary>
        /// TotalSize
        /// </summary>
        /// <returns></returns>
        public int GetTotalSize()
        {
            return this.PayloadLength + 100;
        }

        /// <summary>
        /// 设置Mask。
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public void SetMaskString(string mask)
        {
            var masks = Encoding.ASCII.GetBytes(mask);
            if (masks.Length != 4)
            {
                throw new OverlengthException("Mask只能为ASCII，且只能为四位。");
            }
            this.MaskingKey = masks;
            this.Mask = true;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.PayloadData?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
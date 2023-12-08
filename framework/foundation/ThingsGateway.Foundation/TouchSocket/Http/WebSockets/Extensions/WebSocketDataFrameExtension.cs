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
    /// WSDataFrame辅助扩展类
    /// </summary>
    public static class WebSocketDataFrameExtension
    {
        /// <summary>
        /// 追加二进制流
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static WSDataFrame AppendBinary(this WSDataFrame dataFrame, byte[] buffer, int offset, int length)
        {
            dataFrame.PayloadData ??= new ByteBlock(length);
            dataFrame.PayloadData.Write(buffer, offset, length);
            return dataFrame;
        }

        /// <summary>
        /// 追加文本
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static WSDataFrame AppendText(this WSDataFrame dataFrame, string text, Encoding encoding = default)
        {
            var data = (encoding == default ? Encoding.UTF8 : encoding).GetBytes(text);
            dataFrame.PayloadData ??= new ByteBlock(data.Length);
            dataFrame.PayloadData.Write(data);
            return dataFrame;
        }

        /// <summary>
        /// 构建请求数据（含Make）
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static bool BuildRequest(this WSDataFrame dataFrame, ByteBlock byteBlock)
        {
            dataFrame.Mask = true;
            if (dataFrame.MaskingKey == null)
            {
                dataFrame.SetMaskString("RRQM");
            }
            return dataFrame.Build(byteBlock);
        }

        /// <summary>
        /// 构建请求数据（含Make）
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <returns></returns>
        public static byte[] BuildRequestToBytes(this WSDataFrame dataFrame)
        {
            dataFrame.Mask = true;
            if (dataFrame.MaskingKey == null)
            {
                dataFrame.SetMaskString("RRQM");
            }
            using (var byteBlock = new ByteBlock())
            {
                dataFrame.Build(byteBlock);
                var data = byteBlock.ToArray();
                return data;
            }
        }

        /// <summary>
        /// 构建响应数据（无Make）
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static bool BuildResponse(this WSDataFrame dataFrame, ByteBlock byteBlock)
        {
            return dataFrame.Build(byteBlock);
        }

        /// <summary>
        /// 构建响应数据（无Make）
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <returns></returns>
        public static byte[] BuildResponseToBytes(this WSDataFrame dataFrame)
        {
            using (var byteBlock = new ByteBlock())
            {
                dataFrame.Build(byteBlock);
                var data = byteBlock.ToArray();
                return data;
            }
        }

        /// <summary>
        /// 当<see cref="WSDataType.Text"/>时，转换为Text消息。
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ToText(this WSDataFrame dataFrame, Encoding encoding = default)
        {
            return (encoding == default ? Encoding.UTF8 : encoding).GetString(dataFrame.PayloadData.Buffer, 0, dataFrame.PayloadLength);
        }
    }
}
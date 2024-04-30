
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------



using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 具有直接发送功能
    /// </summary>
    public interface IDefaultSender
    {
        /// <summary>
        /// 绕过适配器，直接发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">数据长度</param>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        void DefaultSend(byte[] buffer, int offset, int length);

    }
}

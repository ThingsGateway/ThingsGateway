﻿#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 采集返回消息
    /// </summary>
    public interface IMessage : IOperResult, IRequestInfo
    {
        /// <summary>
        /// 实体数据长度
        /// </summary>
        int BodyLength { get; set; }

        /// <summary>
        /// 解析后的字节数据
        /// </summary>
        byte[] Content { get; set; }

        /// <summary>
        /// 消息头字节
        /// </summary>
        byte[] HeadBytes { get; }

        /// <summary>
        /// 消息头的指令长度
        /// </summary>
        int HeadBytesLength { get; }


        /// <summary>
        /// 接收的字节信息
        /// </summary>
        byte[] ReceivedBytes { get; set; }

        /// <summary>
        /// 发送的字节信息
        /// </summary>
        byte[] SendBytes { get; set; }

        /// <summary>
        /// 检查头子节的合法性,并赋值<see cref="BodyLength"/><br />
        /// </summary>
        /// <param name="head">接收的头子节</param>
        /// <returns>是否成功的结果</returns>
        bool CheckHeadBytes(byte[] head);
    }
}
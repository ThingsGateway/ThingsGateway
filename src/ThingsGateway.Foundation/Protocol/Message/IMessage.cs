
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation;

/// <summary>
/// 采集返回消息
/// </summary>
public interface IResultMessage : IOperResult, IRequestInfo, IDisposableObject
{
    /// <summary>
    /// 实体数据长度,不固定是返回0
    /// </summary>
    int BodyLength { get; set; }

    /// <summary>
    /// 解析后的字节数据
    /// </summary>
    public byte[] Content { get; set; }

    /// <summary>
    /// 消息头的指令长度,不固定时返回0
    /// </summary>
    int HeadBytesLength { get; }

    /// <summary>
    /// 接收的字节信息
    /// </summary>
    ByteBlock ReceivedByteBlock { get; set; }

    /// <summary>
    /// 发送的字节信息，对于非并发主从协议，可能需要从中获取校验字段，其他情况下可以为空
    /// </summary>
    ByteBlock? SendByteBlock { get; set; }

    /// <summary>
    /// 等待标识，对于并发协议，必须从协议中例如固定头部获取标识字段
    /// </summary>
    long Sign { get; set; }

    /// <summary>
    /// 检查头子节的合法性,并赋值<see cref="BodyLength"/><br />
    /// </summary>
    /// <param name="heads">接收的头子节</param>
    /// <returns>是否成功的结果</returns>
    bool CheckHeadBytes(ByteBlock? headByteBlock);
}

/// <summary>
/// 发送消息
/// </summary>
public interface ISendMessage : IRequestInfo
{
    /// <summary>
    /// 发送的字节信息
    /// </summary>
    ByteBlock SendByteBlock { get; set; }
    /// <summary>
    /// 等待标识，对于并发协议，必须获取标识字段后写入协议报文
    /// </summary>
    long Sign { get; set; }
}

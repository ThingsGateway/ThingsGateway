//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// 采集返回消息
/// </summary>
public interface IResultMessage : IOperResult, IRequestInfo
{
    /// <summary>
    /// 数据体长度
    /// </summary>
    int BodyLength { get; set; }

    /// <summary>
    /// 解析的字节信息
    /// </summary>
    byte[] Content { get; set; }

    /// <summary>
    /// 消息头的指令长度,不固定时返回0
    /// </summary>
    int HeaderLength { get; }

    /// <summary>
    /// 等待标识，对于并发协议，必须从协议中例如固定头部获取标识字段
    /// </summary>
    int Sign { get; set; }

    /// <summary>
    /// 当收到数据，由框架封送有效载荷数据。
    /// 此时流位置为<see cref="HeaderLength"/>
    /// <para>但是如果是因为数据错误，则需要修改<see cref="ByteBlock.Position"/>到正确位置，如果都不正确，则设置<see cref="ByteBlock.Position"/>等于<see cref="ByteBlock.Length"/></para>
    /// <para>然后返回<see cref="FilterResult.GoOn"/></para>
    /// </summary>
    /// <returns>是否成功有效</returns>
    FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock;

    /// <summary>
    /// 检查头子节的合法性,并赋值<see cref="BodyLength"/><br />
    /// <para>如果返回false，意味着放弃本次解析的所有数据，包括已经解析完成的Header</para>
    /// </summary>
    /// <returns>是否成功的结果</returns>
    bool CheckHead<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock;

    /// <summary>
    /// 发送前的信息处理，例如存储某些特征信息：站号/功能码等等用于验证后续的返回信息是否合法
    /// </summary>
    /// <param name="sendMessage"></param>
    /// <returns></returns>
    void SendInfo(ISendMessage sendMessage);
}

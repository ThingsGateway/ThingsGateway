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

namespace ThingsGateway.Foundation;

/// <summary>
/// 读写接口
/// </summary>
public interface IReadWrite
{
    /// <summary>
    /// 日志
    /// </summary>
    ILog Logger { get; }

    /// <summary>
    /// 异步批量读取字节数组信息，需要指定地址和长度
    /// </summary>
    Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token = default);

    /// <summary>
    /// 发送字节组，返回结果
    /// </summary>
    OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default);

    /// <summary>
    /// 异步发送字节组，返回结果
    /// </summary>
    Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default);

    /// <summary>
    /// 异步写入原始的byte数组数据到指定的地址，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default);

    /// <summary>
    /// 异步写入bool数组数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken token = default);

    /// <summary>
    /// 异步写入bool数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, bool value, CancellationToken token = default);

    /// <summary>
    /// 异步写入short数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, short value, CancellationToken token = default);

    /// <summary>
    /// 异步写入ushort数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, ushort value, CancellationToken token = default);

    /// <summary>
    /// 异步写入int数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, int value, CancellationToken token = default);

    /// <summary>
    /// 异步写入uint数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, uint value, CancellationToken token = default);

    /// <summary>
    /// 异步写入long数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, long value, CancellationToken token = default);

    /// <summary>
    /// 异步写入ulong数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, ulong value, CancellationToken token = default);

    /// <summary>
    /// 异步写入float数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, float value, CancellationToken token = default);

    /// <summary>
    /// 异步写入double数据，返回结果
    /// </summary>
    Task<OperResult> WriteAsync(string address, double value, CancellationToken token = default);

    /// <summary>
    /// 异步写入字符串信息
    /// </summary>
    Task<OperResult> WriteAsync(string address, string value, bool isBcd, CancellationToken token = default);

    /// <summary>
    /// 异步写入字符串信息
    /// </summary>
    Task<OperResult> WriteAsync(string address, string value, Encoding encoding, CancellationToken token = default);
}
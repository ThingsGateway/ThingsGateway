//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

namespace ThingsGateway.Foundation;

/// <summary>
/// 协议接口
/// </summary>
public interface IProtocol : IDisposable
{
    #region 属性

    /// <summary>
    /// 组包缓存时间
    /// </summary>
    int CacheTimeout { get; set; }

    /// <summary>
    /// 通道
    /// </summary>
    IChannel Channel { get; }

    /// <summary>
    /// 连接超时时间
    /// </summary>
    ushort ConnectTimeout { get; set; }

    /// <summary>
    /// 数据解析规则
    /// </summary>
    DataFormatEnum DataFormat { get; set; }

    /// <summary>
    /// 是否需要并发锁，默认为true，对于工业主从协议，通常是必须的
    /// </summary>
    bool IsSingleThread { get; }

    /// <summary>
    /// 日志
    /// </summary>
    ILog? Logger { get; }

    /// <inheritdoc/>
    bool OnLine { get; }

    /// <summary>
    /// 一个寄存器所占的字节长度
    /// </summary>
    int RegisterByteLength { get; }

    /// <summary>
    /// 发送前延时
    /// </summary>
    int SendDelayTime { get; set; }

    /// <summary>
    /// 数据解析规则
    /// </summary>
    IThingsGatewayBitConverter ThingsGatewayBitConverter { get; }

    /// <summary>
    /// 读写超时时间
    /// </summary>
    int Timeout { get; set; }

    #endregion 属性

    #region 适配器

    /// <summary>
    /// 获取新的适配器实例
    /// </summary>
    DataHandlingAdapter GetDataAdapter();

    #endregion 适配器

    #region 变量地址解析

    /// <summary>
    /// 寄存器地址的详细说明
    /// </summary>
    /// <returns></returns>
    string GetAddressDescription();
    /// <summary>
    /// 获取变量地址对应的bit偏移，默认0
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    int GetBitOffsetDefault(string address);
    /// <summary>
    /// 获取变量地址对应的bit偏移
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    int? GetBitOffset(string address);

    /// <summary>
    /// 获取数据类型对应的寄存器长度
    /// </summary>
    /// <param name="address">寄存器地址</param>
    /// <param name="length">读取数量</param>
    /// <param name="typeLength">读取数据类型对应的字节长度</param>
    /// <param name="isBool">是否按布尔解析</param>
    /// <returns></returns>
    int GetLength(string address, int length, int typeLength, bool isBool = false);

    /// <summary>
    /// 连读寄存器打包
    /// </summary>
    List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime) where T : IVariableSource, new();

    #endregion 变量地址解析

    #region 动态类型读写

    /// <summary>
    /// 通过数据类型，获取对应的类型值
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<IOperResult<Array>> ReadAsync(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据数据类型，写入类型值
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult> WriteAsync(string address, JToken value, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    #endregion 动态类型读写

    #region 读取

    /// <summary>
    /// 批量读取字节数组信息，需要指定地址和长度
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取寄存器数量，对于不同PLC，对应的字节数量可能不一样</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取布尔量数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<bool[]>> ReadBooleanAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Double数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<double[]>> ReadDoubleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<short[]>> ReadInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<int[]>> ReadInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<long[]>> ReadInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Single数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<float[]>> ReadSingleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取String
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<string[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<ushort[]>> ReadUInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<uint[]>> ReadUInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<OperResult<ulong[]>> ReadUInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    #endregion 读取

    #region 写入

    /// <summary>
    /// 写入原始的byte数组数据到指定的地址，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入bool数组数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入bool数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, bool value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入byte数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, byte value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入short数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入ushort数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入int数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入uint数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入long数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入ulong数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入float数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入double数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入string数据，返回结果
    /// </summary>
    ValueTask<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    #endregion 写入

    #region 写入数组

    /// <summary>
    /// 写入String数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, string[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Double数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, double[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Single数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, float[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, int[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, long[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, short[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, uint[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, ulong[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    ValueTask<OperResult> WriteAsync(string address, ushort[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    #endregion 写入数组

    /// <summary>
    /// 布尔量解析时是否需要按字反转
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    bool BitReverse(string address);

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    Task CloseAsync(string msg = null);

    /// <summary>
    /// 配置IPluginManager
    /// </summary>
    Action<IPluginManager> ConfigurePlugins();

    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取通道
    /// </summary>
    /// <param name="socketId"></param>
    /// <returns></returns>
    ValueTask<OperResult<IClientChannel>> GetChannelAsync(string socketId);

    /// <summary>
    /// 发送，会经过适配器，可传入<see cref="IClientChannel"/>，如果为空，则默认通道必须为<see cref="IClientChannel"/>类型
    /// </summary>
    /// <param name="sendMessage">发送字节数组</param>
    /// <param name="token">取消令箭</param>
    /// <param name="channel">通道</param>
    /// <returns>返回消息体</returns>
    ValueTask<OperResult> SendAsync(ISendMessage sendMessage, IClientChannel channel = default, CancellationToken token = default);

    /// <summary>
    /// 发送，会经过适配器，可传入socketId，如果为空，则默认通道必须为<see cref="IClientChannel"/>类型
    /// </summary>
    /// <param name="socketId">通道</param>
    /// <param name="sendMessage">发送字节数组</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>返回消息体</returns>
    ValueTask<OperResult> SendAsync(ISendMessage sendMessage, string socketId, CancellationToken cancellationToken);

    /// <summary>
    /// 发送并等待返回，会经过适配器，可传入<see cref="IClientChannel"/>，如果为空，则默认通道必须为<see cref="IClientChannel"/>类型
    /// </summary>
    /// <param name="command">发送字节数组</param>
    /// <param name="waitData">waitData</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <param name="channel">通道</param>
    /// <returns>返回消息体</returns>
    ValueTask<OperResult<byte[]>> SendThenReturnAsync(ISendMessage command, IClientChannel channel = default, WaitDataAsync<MessageBase> waitData = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送并等待返回，会经过适配器，可传入socketId，如果为空，则默认通道必须为<see cref="IClientChannel"/>类型
    /// </summary>
    /// <param name="socketId">通道</param>
    /// <param name="sendMessage">发送字节数组</param>
    /// <param name="waitData">waitData</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>返回消息体</returns>
    ValueTask<OperResult<byte[]>> SendThenReturnAsync(ISendMessage sendMessage, string socketId, WaitDataAsync<MessageBase> waitData = default, CancellationToken cancellationToken = default);
}

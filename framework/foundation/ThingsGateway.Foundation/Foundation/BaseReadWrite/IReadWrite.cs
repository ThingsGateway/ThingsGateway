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

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// 设备读写接口
/// </summary>
public interface IReadWrite : IDisposable
{
    #region 属性

    /// <summary>
    /// 日志
    /// </summary>
    ILog Logger { get; }

    /// <summary>
    /// 对象断开连接/释放时，是否操作链路对象
    /// </summary>
    bool CascadeDisposal { get; set; }

    /// <summary>
    /// 通道类型
    /// </summary>
    ChannelEnum ChannelEnum { get; }

    /// <summary>
    /// 多字节数据解析规则
    /// </summary>
    DataFormat? DataFormat { get; set; }

    /// <summary>
    /// 数据解析规则
    /// </summary>
    IThingsGatewayBitConverter ThingsGatewayBitConverter { get; }

    /// <summary>
    /// 读写超时时间(ms)
    /// </summary>
    int TimeOut { get; set; }

    /// <summary>
    /// 一个寄存器所占的字节长度
    /// </summary>
    int RegisterByteLength { get; }

    /// <summary>
    /// 组包缓存时间(ms)
    /// </summary>
    int CacheTimeout { get; set; }

    /// <summary>
    /// 发送延时(ms)
    /// </summary>
    int FrameTime { get; set; }

    #endregion

    #region 连接，设置

    /// <summary>
    /// 获取链路连接状态
    /// </summary>
    bool IsConnected();

    /// <summary>
    /// 获取变量地址对应的bit偏移
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    int GetBitOffset(string address);

    /// <summary>
    /// 寄存器地址的详细说明
    /// </summary>
    /// <returns></returns>
    string GetAddressDescription();

    /// <summary>
    /// 连读变量打包
    /// </summary>
    List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack, int defaultIntervalTime) where T : IDeviceVariableSourceRead<IDeviceVariableRunTime>, new() where T2 : IDeviceVariableRunTime, new();

    /// <summary>
    /// 布尔量是否需要按字反转
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    bool BitReverse(string address);

    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="cancellationToken">取消令箭</param>
    void Connect(CancellationToken cancellationToken);

    /// <summary>
    /// 异步连接
    /// </summary>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 断开连接
    /// </summary>
    void Disconnect();

    /// <summary>
    /// 设置适配器
    /// </summary>
    /// <param name="socketClient">客户端，对应Server有效</param>
    void SetDataAdapter(ISocketClient socketClient = default);

    /// <summary>
    /// 获取数据类型对应的寄存器长度
    /// </summary>
    /// <param name="address">寄存器地址</param>
    /// <param name="length">读取数量</param>
    /// <param name="typeLength">读取数据类型对应的字节长度</param>
    /// <param name="isBool">isBool</param>
    /// <returns></returns>
    int GetLength(string address, int length, int typeLength, bool isBool = false);

    /// <summary>
    /// 发送
    /// </summary>
    /// <param name="command">发送字节数组</param>
    /// <param name="id">客户端id，TcpServer中生效</param>
    /// <returns></returns>
    void Send(byte[] command, string id = default);

    /// <summary>
    /// 发送并返回结果
    /// </summary>
    /// <typeparam name="T">返回的类型，需继承<see cref="IRequestInfo"/>与<see cref="OperResult{T}"/></typeparam>
    /// <param name="command">字节数组</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <param name="senderClient">传入改参数时，将新建等待线程</param>
    /// <returns></returns>
    T SendThenReturn<T>(byte[] command, CancellationToken cancellationToken, ISenderClient senderClient = default) where T : OperResult<byte[]>;

    /// <summary>
    /// 异步发送并返回结果
    /// </summary>
    /// <typeparam name="T">返回的类型，需继承<see cref="IRequestInfo"/>与<see cref="OperResult{T}"/></typeparam>
    /// <param name="command">字节数组</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <param name="senderClient">传入改参数时，将新建等待线程</param>
    /// <returns></returns>
    Task<T> SendThenReturnAsync<T>(byte[] command, CancellationToken cancellationToken, ISenderClient senderClient = default) where T : OperResult<byte[]>;

    /// <summary>
    /// 发送获取数据
    /// </summary>
    /// <param name="item">发送的字节数组</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <param name="senderClient">传入改参数时，将新建等待线程</param>
    /// <returns></returns>
    Task<ResponsedData> GetResponsedDataAsync(byte[] item, int timeout, CancellationToken cancellationToken, ISenderClient senderClient = default);

    /// <summary>
    /// 异步发送获取数据
    /// </summary>
    /// <param name="item">发送的字节数组</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <param name="senderClient">传入改参数时，将新建等待线程</param>
    /// <returns></returns>
    ResponsedData GetResponsedData(byte[] item, int timeout, CancellationToken cancellationToken, ISenderClient senderClient = default);

    #endregion

    #region 读取

    /// <summary>
    /// 批量读取字节数组信息，需要指定地址和长度
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取寄存器数量，对于不同PLC，对应的字节数量可能不一样</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="ReadAsync(string, int, CancellationToken)"/>
    /// </summary>
    OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过数据类型，获取对应的类型值
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<object> Read(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Read(string, int, DataTypeEnum, CancellationToken)"/>
    Task<IOperResult<object>> ReadAsync(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取布尔量数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<bool[]> ReadBoolean(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadBoolean(string, int, CancellationToken)"/>
    Task<OperResult<bool[]>> ReadBooleanAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Double数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<double[]> ReadDouble(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadDouble(string, int, CancellationToken)"/>
    Task<OperResult<double[]>> ReadDoubleAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<short[]> ReadInt16(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadInt16(string, int, CancellationToken)"/>
    Task<OperResult<short[]>> ReadInt16Async(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<int[]> ReadInt32(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadInt32(string, int, CancellationToken)"/>
    Task<OperResult<int[]>> ReadInt32Async(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<long[]> ReadInt64(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadInt64(string, int, CancellationToken)"/>
    Task<OperResult<long[]>> ReadInt64Async(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Single数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<float[]> ReadSingle(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadSingle(string, int, CancellationToken)"/>
    Task<OperResult<float[]>> ReadSingleAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取String
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<string> ReadString(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadString(string, int, CancellationToken)"/>
    Task<OperResult<string>> ReadStringAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<ushort[]> ReadUInt16(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadUInt16(string, int, CancellationToken)"/>
    Task<OperResult<ushort[]>> ReadUInt16Async(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<uint[]> ReadUInt32(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadUInt32(string, int, CancellationToken)"/>
    Task<OperResult<uint[]>> ReadUInt32Async(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult<ulong[]> ReadUInt64(string address, int length, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadUInt64(string, int, CancellationToken)"/>
    Task<OperResult<ulong[]>> ReadUInt64Async(string address, int length, CancellationToken cancellationToken = default);

    #endregion

    #region 写入数组

    /// <summary>
    /// 写入Double数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    OperResult Write(string address, double[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Single数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    OperResult Write(string address, float[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    OperResult Write(string address, int[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    OperResult Write(string address, long[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    OperResult Write(string address, short[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    OperResult Write(string address, uint[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    OperResult Write(string address, ulong[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    OperResult Write(string address, ushort[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, double[], CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, double[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, float[], CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, float[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, int[], CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, int[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, long[], CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, long[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, short[], CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, short[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, uint[], CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, uint[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, ulong[], CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, ulong[] value, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, ushort[], CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, ushort[] value, CancellationToken cancellationToken = default);

    #endregion

    #region 写入

    /// <summary>
    /// 根据数据类型，写入类型值
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    OperResult Write(string address, string value, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, string, DataTypeEnum, CancellationToken)"/>
    Task<OperResult> WriteAsync(string address, string value, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, byte[], CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, bool[], CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, bool, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, bool value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, byte, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, byte value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, short, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, short value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, ushort, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, ushort value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, int, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, int value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, uint, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, uint value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, long, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, long value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, ulong, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, ulong value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, float, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, float value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, double, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, double value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, string, CancellationToken)"/>
    /// </summary>
    Task<OperResult> WriteAsync(string address, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入原始的byte数组数据到指定的地址，返回结果
    /// </summary>
    OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入bool数组数据，返回结果
    /// </summary>
    OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入bool数据，返回结果
    /// </summary>
    OperResult Write(string address, bool value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入byte数据，返回结果
    /// </summary>
    OperResult Write(string address, byte value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入short数据，返回结果
    /// </summary>
    OperResult Write(string address, short value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入ushort数据，返回结果
    /// </summary>
    OperResult Write(string address, ushort value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入int数据，返回结果
    /// </summary>
    OperResult Write(string address, int value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入uint数据，返回结果
    /// </summary>
    OperResult Write(string address, uint value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入long数据，返回结果
    /// </summary>
    OperResult Write(string address, long value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入ulong数据，返回结果
    /// </summary>
    OperResult Write(string address, ulong value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入float数据，返回结果
    /// </summary>
    OperResult Write(string address, float value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入double数据，返回结果
    /// </summary>
    OperResult Write(string address, double value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入字符串信息
    /// </summary>
    OperResult Write(string address, string value, CancellationToken cancellationToken = default);

    #endregion
}
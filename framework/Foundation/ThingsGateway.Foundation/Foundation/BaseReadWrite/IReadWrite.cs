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


    #endregion

    #region 异步写入

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

    #endregion

    #region 同步写入

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

    #region 属性
    /// <summary>
    /// 日志
    /// </summary>
    ILog Logger { get; }

    /// <summary>
    /// 对象断开连接/释放时，是否同样操作链路对象
    /// </summary>
    bool CascadeDisposal { get; set; }

    /// <summary>
    /// 获取变量地址对应的bit偏移
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    int GetBitOffset(string address);


    /// <summary>
    /// 多字节数据解析规则
    /// </summary>
    DataFormat DataFormat { get; set; }

    /// <summary>
    /// 数据解析规则
    /// </summary>
    IThingsGatewayBitConverter ThingsGatewayBitConverter { get; }

    /// <summary>
    /// 读写超时时间
    /// </summary>
    ushort TimeOut { get; set; }

    /// <summary>
    /// 一个寄存器所占的字节长度
    /// </summary>
    ushort RegisterByteLength { get; }

    /// <summary>
    /// 组包缓存时间/ms
    /// </summary>
    int CacheTimeout { get; set; }
    /// <summary>
    /// 帧前时间ms
    /// </summary>
    int FrameTime { get; set; }

    /// <summary>
    /// 寄存器地址的详细说明
    /// </summary>
    /// <returns></returns>
    string GetAddressDescription();

    #endregion

    /// <inheritdoc/>
    void Connect(CancellationToken cancellationToken);
    /// <inheritdoc/>
    Task ConnectAsync(CancellationToken cancellationToken);
    /// <inheritdoc/>
    void Disconnect();
    /// <inheritdoc/>
    IOperResult<object> Read(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<IOperResult<object>> ReadAsync(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<bool[]> ReadBoolean(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<bool[]>> ReadBooleanAsync(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<double[]> ReadDouble(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<double[]>> ReadDoubleAsync(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<short[]> ReadInt16(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<short[]>> ReadInt16Async(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<int[]> ReadInt32(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<int[]>> ReadInt32Async(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<long[]> ReadInt64(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<long[]>> ReadInt64Async(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<float[]> ReadSingle(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<float[]>> ReadSingleAsync(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<string> ReadString(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<string>> ReadStringAsync(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<ushort[]> ReadUInt16(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<ushort[]>> ReadUInt16Async(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<uint[]> ReadUInt32(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<uint[]>> ReadUInt32Async(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult<ulong[]> ReadUInt64(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult<ulong[]>> ReadUInt64Async(string address, int length, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    void SetDataAdapter(object socketClient = null);
    /// <inheritdoc/>
    OperResult Write(string address, double[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult Write(string address, float[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult Write(string address, int[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult Write(string address, long[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult Write(string address, short[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult Write(string address, string value, DataTypeEnum dataType, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult Write(string address, uint[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult Write(string address, ulong[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    OperResult Write(string address, ushort[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, double[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, float[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, int[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, long[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, short[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, string value, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, uint[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, ulong[] value, CancellationToken cancellationToken = default);
    /// <inheritdoc/>
    Task<OperResult> WriteAsync(string address, ushort[] value, CancellationToken cancellationToken = default);
    /// <summary>
    /// 连读变量打包
    /// </summary>
    List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack) where T : IDeviceVariableSourceRead<IDeviceVariableRunTime>, new() where T2 : IDeviceVariableRunTime, new();
    /// <summary>
    /// 布尔量是否需要按字反转
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    bool IsBitReverse(string address);
}

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
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
    /// 日志
    /// </summary>
    ILog? Logger { get; }

    /// <summary>
    /// 通道
    /// </summary>
    IChannel Channel { get; }

    /// <summary>
    /// 默认多字节数据解析规则
    /// </summary>
    DataFormatEnum? DataFormat { get; set; }

    /// <summary>
    /// 数据解析规则
    /// </summary>
    IThingsGatewayBitConverter ThingsGatewayBitConverter { get; }

    /// <summary>
    /// 读写超时时间
    /// </summary>
    int Timeout { get; set; }

    /// <summary>
    /// 一个寄存器所占的字节长度
    /// </summary>
    int RegisterByteLength { get; }

    /// <summary>
    /// 组包缓存时间
    /// </summary>
    int CacheTimeout { get; set; }

    /// <summary>
    /// 发送前延时
    /// </summary>
    int SendDelayTime { get; set; }

    /// <inheritdoc/>
    bool OnLine { get; }

    /// <summary>
    /// 是否需要并发锁，默认为true，对于工业主从协议，通常是必须的
    /// </summary>
    bool IsSingleThread { get; }

    #endregion 属性

    #region 适配器

    /// <summary>
    /// 获取新的适配器实例
    /// </summary>
    DataHandlingAdapter GetDataAdapter();

    #endregion 适配器

    #region 变量地址解析

    /// <summary>
    /// 获取变量地址对应的bit偏移
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    int GetBitOffset(string address);

    /// <summary>
    /// 布尔量解析时是否需要按字反转
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <returns></returns>
    bool BitReverse(string address);

    /// <summary>
    /// 寄存器地址的详细说明
    /// </summary>
    /// <returns></returns>
    string GetAddressDescription();

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
    IOperResult<Array> Read(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Read(string, int, DataTypeEnum,   CancellationToken)"/>
    ValueTask<IOperResult<Array>> ReadAsync(string address, int length, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据数据类型，写入类型值
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult Write(string address, JToken value, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, JToken, DataTypeEnum,  CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, JToken value, DataTypeEnum dataType, CancellationToken cancellationToken = default);

    #endregion 动态类型读写

    #region 读取

    /// <summary>
    /// 批量读取字节数组信息，需要指定地址和长度
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取寄存器数量，对于不同PLC，对应的字节数量可能不一样</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    ValueTask<IOperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="ReadAsync(string, int,  CancellationToken)"/>
    /// </summary>
    IOperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取布尔量数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<bool[]> ReadBoolean(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadBoolean(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<bool[]>> ReadBooleanAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Double数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<double[]> ReadDouble(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadDouble(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<double[]>> ReadDoubleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<short[]> ReadInt16(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadInt16(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<short[]>> ReadInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<int[]> ReadInt32(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadInt32(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<int[]>> ReadInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Int64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<long[]> ReadInt64(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadInt64(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<long[]>> ReadInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取Single数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<float[]> ReadSingle(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadSingle(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<float[]>> ReadSingleAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取String
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<string[]> ReadString(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadString(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<string[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<ushort[]> ReadUInt16(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadUInt16(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<ushort[]>> ReadUInt16Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<uint[]> ReadUInt32(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadUInt32(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<uint[]>> ReadUInt32Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="length">读取长度</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    IOperResult<ulong[]> ReadUInt64(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="ReadUInt64(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult<ulong[]>> ReadUInt64Async(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    #endregion 读取

    #region 写入

    /// <summary>
    /// 写入原始的byte数组数据到指定的地址，返回结果
    /// </summary>
    IOperResult Write(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入bool数组数据，返回结果
    /// </summary>
    IOperResult Write(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入bool数据，返回结果
    /// </summary>
    IOperResult Write(string address, bool value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入byte数据，返回结果
    /// </summary>
    IOperResult Write(string address, byte value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入short数据，返回结果
    /// </summary>
    IOperResult Write(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入ushort数据，返回结果
    /// </summary>
    IOperResult Write(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入int数据，返回结果
    /// </summary>
    IOperResult Write(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入uint数据，返回结果
    /// </summary>
    IOperResult Write(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入long数据，返回结果
    /// </summary>
    IOperResult Write(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入ulong数据，返回结果
    /// </summary>
    IOperResult Write(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入float数据，返回结果
    /// </summary>
    IOperResult Write(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入double数据，返回结果
    /// </summary>
    IOperResult Write(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入字符串信息
    /// </summary>
    IOperResult Write(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, byte[], CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, bool[], CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, bool, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, bool value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, byte, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, byte value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, short, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, ushort, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, int, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, uint, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, long, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, ulong, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, float, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, double, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// <inheritdoc cref="Write(string, string, IThingsGatewayBitConverter, CancellationToken)"/>
    /// </summary>
    ValueTask<IOperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

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
    IOperResult Write(string address, string[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Double数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    IOperResult Write(string address, double[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Single数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    IOperResult Write(string address, float[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    IOperResult Write(string address, int[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    IOperResult Write(string address, long[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入Int16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    IOperResult Write(string address, short[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt32数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    IOperResult Write(string address, uint[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt64数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    IOperResult Write(string address, ulong[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入UInt16数组
    /// </summary>
    /// <param name="address">变量地址</param>
    /// <param name="value">值</param>
    /// <param name="bitConverter">转换规则</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns>写入结果</returns>
    IOperResult Write(string address, ushort[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, string[], IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, string[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, double[], IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, double[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, float[], IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, float[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, int[], IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, int[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, long[], IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, long[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, short[], IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, short[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, uint[], IThingsGatewayBitConverter,CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, uint[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, ulong[], IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, ulong[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="Write(string, ushort[], IThingsGatewayBitConverter, CancellationToken)"/>
    ValueTask<IOperResult> WriteAsync(string address, ushort[] value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default);

    #endregion 写入数组

    /// <summary>
    /// 发送并等待返回，会经过适配器，可传入<see cref="IClientChannel"/>，如果为空，则默认通道必须为<see cref="IClientChannel"/>类型
    /// </summary>
    /// <param name="command">发送字节数组</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <param name="channel">通道</param>
    /// <returns>返回消息体</returns>
    ValueTask<IOperResult<byte[]>> SendThenReturnAsync(ISendMessage command, CancellationToken cancellationToken, IClientChannel channel = null);

    /// <inheritdoc cref="SendThenReturnAsync(ISendMessage, CancellationToken, IClientChannel)"/>
    IOperResult<byte[]> SendThenReturn(ISendMessage command, CancellationToken cancellationToken, IClientChannel channel = null);

    /// <summary>
    /// 发送数据，不经过适配器
    /// </summary>
    /// <param name="command"></param>
    /// <param name="channel"></param>
    void DefaultSend(byte[] command, int offset, int length, IClientChannel channel = null, CancellationToken token = default);

    /// <summary>
    /// 配置IPluginManager
    /// </summary>
    Action<IPluginManager> ConfigurePlugins();
    ValueTask SendAsync(byte[] command, int offset, int length, IClientChannel channel = null, CancellationToken cancellationToken = default);
    ValueTask DefaultSendAsync(byte[] command, int offset, int length, IClientChannel channel = null, CancellationToken token = default);
}

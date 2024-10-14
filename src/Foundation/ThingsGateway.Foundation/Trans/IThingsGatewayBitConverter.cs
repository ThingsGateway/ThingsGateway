//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;

#endif

using Newtonsoft.Json;

using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// 类型转换
/// </summary>
public interface IThingsGatewayBitConverter
{
    /// <summary>
    /// 指定大小端。
    /// </summary>
    EndianType EndianType { get; }

    /// <summary>
    /// 当前的字符串编码类型
    /// </summary>
#if NET6_0_OR_GREATER
    [System.Text.Json.Serialization.JsonConverter(typeof(EncodingConverter))]
    [JsonProperty(ItemConverterType = typeof(NewtonsoftEncodingConverter))]
#else

    [JsonProperty(ItemConverterType = typeof(NewtonsoftEncodingConverter))]
#endif
    Encoding Encoding { get; set; }

    /// <summary>
    /// 当前的Bcd编码类型
    /// </summary>
    BcdFormatEnum? BcdFormat { get; set; }

    /// <summary>
    /// 字符串长度
    /// </summary>
    int? StringLength { get; set; }

    /// <summary>
    /// 数组长度，只在连读时生效
    /// </summary>
    int? ArrayLength { get; set; }

    /// <summary>
    /// 获取或设置在解析字符串的时候是否将字节按照字单位反转
    /// </summary>
    bool IsStringReverseByteWord { get; set; }
    /// <summary>
    /// 获取或设置在解析字符串的时候是否变长字符串
    /// </summary>
    bool IsVariableStringLength { get; set; }

    /// <summary>
    /// 4字节解析规则
    /// </summary>
    DataFormatEnum DataFormat { get; set; }

    #region GetBytes

    /// <summary>
    /// 转换为指定端16字节
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    byte[] GetBytes(decimal value);

    /// <summary>
    /// 转换为指定端2字节
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    byte[] GetBytes(char value);

    /// <summary>
    /// bool变量转化缓存数据，一般来说单bool只能转化为0x01 或是 0x00<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(bool value);

    /// <summary>
    /// 将bool数组变量转化缓存数据，如果数组长度不满足8的倍数，则自动补0操作。<br />
    /// </summary>
    /// <param name="values">等待转化的数组</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(bool[] values);

    /// <summary>
    /// short变量转化缓存数据，一个short数据可以转为2个字节的byte数组<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(short value);

    /// <inheritdoc/>
    byte[] GetBytes(short[] value);

    /// <inheritdoc/>
    byte[] GetBytes(ushort[] value);

    /// <inheritdoc/>
    byte[] GetBytes(int[] value);

    /// <inheritdoc/>
    byte[] GetBytes(uint[] value);

    /// <inheritdoc/>
    byte[] GetBytes(long[] value);

    /// <inheritdoc/>
    byte[] GetBytes(ulong[] value);

    /// <inheritdoc/>
    byte[] GetBytes(float[] value);

    /// <inheritdoc/>
    byte[] GetBytes(double[] value);

    /// <summary>
    /// ushort变量转化缓存数据，一个ushort数据可以转为2个字节的Byte数组<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(ushort value);

    /// <summary>
    /// int变量转化缓存数据，一个int数据可以转为4个字节的byte数组<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(int value);

    /// <summary>
    /// uint变量转化缓存数据，一个uint数据可以转为4个字节的byte数组<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(uint value);

    /// <summary>
    /// long变量转化缓存数据，一个long数据可以转为8个字节的byte数组<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(long value);

    /// <summary>
    /// ulong变量转化缓存数据，一个ulong数据可以转为8个字节的byte数组<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(ulong value);

    /// <summary>
    /// float变量转化缓存数据，一个float数据可以转为4个字节的byte数组<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(float value);

    /// <summary>
    /// double变量转化缓存数据，一个double数据可以转为8个字节的byte数组<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(double value);

    /// <summary>
    /// 使用指定的编码字符串转化缓存数据<br />
    /// </summary>
    /// <param name="value">等待转化的数据</param>
    /// <returns>buffer数据</returns>
    byte[] GetBytes(string value);

    #endregion GetBytes

    #region ToValue

    /// <summary>
    /// 从缓存中提取出bool结果，需要传入想要提取的位索引，注意：是从0开始的位索引，10则表示 buffer[1] 的第二位。<br />
    /// </summary>
    /// <param name="buffer">等待提取的缓存数据</param>
    /// <param name="offset">位的索引，注意：是从0开始的位索引，10则表示 buffer[1] 的第二位。</param>
    /// <param name="isReverse">是否需要按字反转</param>
    bool ToBoolean(byte[] buffer, int offset, bool isReverse);

    /// <inheritdoc/>
    bool[] ToBoolean(byte[] buffer, int offset, int len, bool isReverse);

    /// <inheritdoc/>
    byte ToByte(byte[] buffer, int offset);

    /// <inheritdoc/>
    byte[] ToByte(byte[] buffer, int offset, int length);

    /// <summary>
    /// 从缓存中提取double结果，需要指定起始的字节索引，按照字节为单位，一个double占用八个字节<br />
    /// </summary>
    /// <param name="buffer">缓存对象</param>
    /// <param name="offset">索引位置</param>
    /// <returns>double对象</returns>
    double ToDouble(byte[] buffer, int offset);

    /// <inheritdoc/>
    double[] ToDouble(byte[] buffer, int offset, int len);

    /// <summary>
    /// 从缓存中提取short结果，需要指定起始的字节索引，按照字节为单位，一个short占用两个字节<br />
    /// </summary>
    /// <param name="buffer">缓存数据</param>
    /// <param name="offset">索引位置</param>
    /// <returns>short对象</returns>
    short ToInt16(byte[] buffer, int offset);

    /// <inheritdoc/>
    short[] ToInt16(byte[] buffer, int offset, int len);

    /// <summary>
    /// 从缓存中提取int结果，需要指定起始的字节索引，按照字节为单位，一个int占用四个字节<br />
    /// </summary>
    /// <param name="buffer">缓存数据</param>
    /// <param name="offset">索引位置</param>
    /// <returns>int对象</returns>
    int ToInt32(byte[] buffer, int offset);

    /// <inheritdoc/>
    int[] ToInt32(byte[] buffer, int offset, int len);

    /// <summary>
    /// 从缓存中提取long结果，需要指定起始的字节索引，按照字节为单位，一个long占用八个字节<br />
    /// </summary>
    /// <param name="buffer">缓存数据</param>
    /// <param name="offset">索引位置</param>
    /// <returns>long对象</returns>
    long ToInt64(byte[] buffer, int offset);

    /// <inheritdoc/>
    long[] ToInt64(byte[] buffer, int offset, int len);

    /// <summary>
    /// 从缓存中提取float结果，需要指定起始的字节索引，按照字节为单位，一个float占用四个字节<b />
    /// </summary>
    /// <param name="buffer">缓存对象</param>
    /// <param name="offset">索引位置</param>
    /// <returns>float对象</returns>
    float ToSingle(byte[] buffer, int offset);

    /// <inheritdoc/>
    float[] ToSingle(byte[] buffer, int offset, int len);

    /// <summary>
    /// 从缓存中的部分字节数组转化为string结果，使用指定的编码，指定起始的字节索引，字节长度信息。<br />
    /// </summary>
    /// <param name="buffer">缓存对象</param>
    /// <param name="offset">索引位置</param>
    /// <param name="length">byte数组长度</param>
    /// <returns>string对象</returns>
    string ToString(byte[] buffer, int offset, int length);

    /// <summary>
    /// 从缓存中提取ushort结果，需要指定起始的字节索引，按照字节为单位，一个ushort占用两个字节<br />
    /// </summary>
    /// <param name="buffer">缓存数据</param>
    /// <param name="offset">索引位置</param>
    /// <returns>ushort对象</returns>
    ushort ToUInt16(byte[] buffer, int offset);

    /// <inheritdoc/>
    ushort[] ToUInt16(byte[] buffer, int offset, int len);

    /// <summary>
    /// 从缓存中提取uint结果，需要指定起始的字节索引，按照字节为单位，一个uint占用四个字节<br />
    /// </summary>
    /// <param name="buffer">缓存数据</param>
    /// <param name="offset">索引位置</param>
    /// <returns>uint对象</returns>
    uint ToUInt32(byte[] buffer, int offset);

    /// <inheritdoc/>
    uint[] ToUInt32(byte[] buffer, int offset, int len);

    /// <summary>
    /// 从缓存中提取ulong结果，需要指定起始的字节索引，按照字节为单位，一个ulong占用八个字节<b />
    /// </summary>
    /// <param name="buffer">缓存数据</param>
    /// <param name="offset">索引位置</param>
    /// <returns>ulong对象</returns>
    ulong ToUInt64(byte[] buffer, int offset);

    /// <inheritdoc/>
    ulong[] ToUInt64(byte[] buffer, int offset, int len);

    /// <summary>
    ///  转换为指定端模式的<see cref="decimal"/>数据。
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    decimal ToDecimal(byte[] buffer, int offset);

    /// <summary>
    ///  转换为指定端模式的Char数据。
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    char ToChar(byte[] buffer, int offset);

    /// <summary>
    /// 从缓存中提取decimal结果，需要指定起始的字节索引，按照字节为单位，一个decimal占用16个字节<b />
    /// </summary>
    /// <param name="buffer">缓存数据</param>
    /// <param name="offset">索引位置</param>
    /// <param name="length">length</param>
    /// <returns>decimal对象</returns>
    decimal[] ToDecimal(byte[] buffer, int offset, int length);

    /// <summary>
    /// 获取指定的数据格式
    /// </summary>
    /// <param name="dataFormat"></param>
    /// <returns></returns>
    IThingsGatewayBitConverter GetByDataFormat(DataFormatEnum dataFormat);

    #endregion ToValue
}

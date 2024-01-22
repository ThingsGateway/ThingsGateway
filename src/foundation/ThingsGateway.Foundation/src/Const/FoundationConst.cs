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
/// 常量
/// </summary>
public class FoundationConst
{
    /// <summary>
    /// Dtu-{0}-已连接
    /// </summary>
    public const string DtuConnected = "Dtu-{0}-Connected.";

    /// <summary>
    /// 客户端未连接，或寄存器设置错误，必须设置ID={DTU注册包}
    /// </summary>
    public const string DtuNoConnectedWaining = "The client is not connected or the register is set incorrectly. id={Dtu registration package} must be set.";

    /// <summary>
    /// 接收
    /// </summary>
    public const string Receive = "Receive";

    /// <summary>
    /// 发送
    /// </summary>
    public const string Send = "Send";

    /// <summary>
    /// 主动断开
    /// </summary>
    public const string ProactivelyDisconnect = "{0} Proactively disconnect.";

    /// <summary>
    /// 必须继承{0}才能使用这个适配器
    /// </summary>
    public const string AdapterTypeError = "Must inherit from {0} to use this adapter.";

    /// <summary>
    /// 当前适配器不支持对象发送
    /// </summary>
    public const string CannotSendIRequestInfo = "The current adapter does not support object sending.";

    /// <summary>
    /// 不允许自由调用{0}进行赋值。
    /// </summary>
    public const string CannotSet = "Free calls to {0} for assignment are not allowed.";

    /// <summary>
    /// 该适配器不支持拼接发送
    /// </summary>
    public const string CannotSplicingSend = "This adapter does not support splicing sending.";

    /// <summary>
    /// 此适配器已被其他终端使用，请重新创建对象。
    /// </summary>
    public const string CannotUseAdapterAgain = "This adapter is already used by another terminal, please recreate the object.";

    /// <summary>
    /// 配置文件不能为空
    /// </summary>
    public const string ConfigNotNull = "Configuration file cannot be empty.";

    /// <summary>
    /// 连接成功
    /// </summary>
    public const string Connected = "Connection succeeded.";

    /// <summary>
    /// 连接成功
    /// </summary>
    public const string ConnectTimeout = "Connect timeout.";

    /// <summary>
    /// 正在连接
    /// </summary>
    public const string Connecting = "Connecting.";

    /// <summary>
    /// 数据长度错误：{0}
    /// </summary>
    public const string DataLengthError = "Data length error {0}.";

    /// <summary>
    /// {0}数据类型未实现
    /// </summary>
    public const string DataTypeNotSupported = "{0} data type is not implemented";

    /// <summary>
    /// 默认寄存器说明，中文
    /// </summary>
    public const string DefaultAddressDes =
        """
        ————————————————————
        4字节数据转换格式：data=ABCD;可选ABCD=>Big-Endian;BADC=>;Big-Endian Byte Swap;CDAB=>Little-Endian Byte Swap;DCBA=>Little-Endian。
        字符串长度：len=1。
        数组长度：arraylen=1。只在打包连读时生效
        Bcd格式：bcd=C8421，可选C8421;C5421;C2421;C3;Gray。
        字符格式：encoding=UTF-8，可选UTF-8;ASCII;Default;Unicode等。
        ————————————————————
        """;

    /// <summary>
    /// 已断开连接
    /// </summary>
    public const string Disconnected = "Disconnected.";

    /// <summary>
    /// 正在断开连接
    /// </summary>
    public const string Disconnecting = "Disconnecting.";

    /// <summary>
    /// 错误信息
    /// </summary>
    public const string ErrorMessage = "Error message";

    /// <summary>
    /// 在事件{0}中发生错误
    /// </summary>
    public const string EventError = "An error occurred in event {0}.";

    /// <summary>
    /// 异常堆栈
    /// </summary>
    public const string Exception = "Exception stack";

    /// <summary>
    /// 数据长度不足，原始数据：{0}
    /// </summary>
    public const string LengthShortError = "Insufficient data length, original data: {0}.";

    /// <summary>
    /// 接收数据正确，但主机并没有主动请求数据。
    /// </summary>
    public const string NotActiveQueryError = "The data is received correctly, but the host does not actively request the data.";

    /// <summary>
    /// 接收出现错误：{0}，错误代码：{1}
    /// </summary>
    public const string ProcessReceiveError = "Reception error: {0}, error code: {1}.";

    /// <summary>
    /// 在处理数据时发生错误
    /// </summary>
    public const string ReceiveError = "An error occurred while processing the data.";

    /// <summary>
    /// 远程终端已关闭
    /// </summary>
    public const string RemoteClose = "The remote terminal is closed.";

    /// <summary>
    /// 新的SerialPort必须在连接状态
    /// </summary>
    public const string SerialPortNotClient = "The new SerialPort must be in the connected state.";

    /// <summary>
    /// 已启动
    /// </summary>
    public const string ServiceStarted = "Started.";

    /// <summary>
    /// 已停止
    /// </summary>
    public const string ServiceStoped = "Stopped.";

    /// <summary>
    /// 字符串读写必须在寄存器地址中指定长度，例如 len=10;
    /// </summary>
    public const string StringAddressError = "String data type must have a length specified in the register address, e.g. len = 10;";

    /// <summary>
    /// 转换失败-原始字节数组： {0}，长度：({1})
    /// </summary>
    public const string TransBytesError = "Conversion failed - raw byte array: {0}, length: ({1})";

    /// <summary>
    /// 未知错误
    /// </summary>
    public const string UnknowError = "Unknow error.";
}
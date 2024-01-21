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

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// 常量
/// </summary>
public class ModbusConst
{
    /// <summary>
    /// 地址说明
    /// </summary>
    public const string AddressDes =
"""
线圈寄存器使用从 00001 开始的地址编号。
离散输入寄存器使用从 10001 开始的地址编号。
输入寄存器使用从 30001 开始的地址编号。
保持寄存器使用从 40001 开始的地址编号。
举例：40001=>保持寄存器第一个寄存器
额外格式
设备站号 ，比如40001;s=2; ，代表设备地址为2的保持寄存器第一个寄存器
写入功能码 ，比如40001;w=16; ，代表保持寄存器第一个寄存器，写入值时采用0x10功能码
"""
;

    /// <summary>
    /// Dtu-{0}-已连接
    /// </summary>
    public const string DtuConnected = "Dtu-{0}-Connected.";

    /// <summary>
    /// 客户端未连接，或寄存器设置错误，必须设置ID={DTU注册包}
    /// </summary>
    public const string DtuNoConnectedWaining = "The client is not connected or the register is set incorrectly. id={Dtu registration package} must be set.";

    /// <summary>
    /// 功能码错误
    /// </summary>
    public const string FunctionError = "Function code error";

    /// <summary>
    /// 功能码不一致，请求功能码{0}，返回功能码{1}。
    /// </summary>
    public const string FunctionNotSame = "The function codes are inconsistent. Function code {0} is requested and function code {1} is returned.";

    /// <summary>
    /// 不支持的功能码
    /// </summary>
    public const string ModbusError1 = "Unsupported function code.";

    /// <summary>
    /// 网关路径不可用
    /// </summary>
    public const string ModbusError10 = "Gateway path is not available.";

    /// <summary>
    /// 网关目标设备响应失败
    /// </summary>
    public const string ModbusError11 = "Gateway target device failed to respond.";

    /// <summary>
    /// 读取寄存器越界
    /// </summary>
    public const string ModbusError2 = "Reading register out of bounds.";

    /// <summary>
    /// 读取长度超限
    /// </summary>
    public const string ModbusError3 = "Read length exceeded.";

    /// <summary>
    /// 设备故障
    /// </summary>
    public const string ModbusError4 = "Equipment failure.";

    /// <summary>
    /// 设备已确认，但未执行
    /// </summary>
    public const string ModbusError5 = "Device confirmed but not executed.";

    /// <summary>
    /// 设备忙
    /// </summary>
    public const string ModbusError6 = "Device busy.";

    /// <summary>
    /// 存储奇偶性错误
    /// </summary>
    public const string ModbusError8 = "Storage parity error.";

    /// <summary>
    /// 站号不一致，请求站号{0}，返回站号{1}。
    /// </summary>
    public const string StationNotSame = "The station number is inconsistent. The station number {0} is requested and the station number {1} is returned.";

    /// <summary>
    /// {0} 不能超过 {1}
    /// </summary>
    public const string ValueOverlimit = "{0} can't be more than {1}";
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusRequest
{
    #region Request

    /// <summary>
    /// 数据
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; set; }

    /// <summary>
    /// 功能码
    /// </summary>
    public byte FunctionCode { get; set; }

    public bool IsBitFunction => FunctionCode == 1 || FunctionCode == 2;

    /// <summary>
    /// 可能表示读取字节数组长度，也可能为请求寄存器数量
    /// </summary>
    public ushort Length { get; set; } = 1;

    /// <summary>
    /// 起始位置
    /// </summary>
    public ushort StartAddress { get; set; }

    /// <summary>
    /// 站号
    /// </summary>
    public byte Station { get; set; }

    #endregion Request
}

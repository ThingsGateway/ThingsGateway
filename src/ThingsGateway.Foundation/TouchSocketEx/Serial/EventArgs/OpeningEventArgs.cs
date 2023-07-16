#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.IO.Ports;

namespace ThingsGateway.Foundation.Serial;

/// <summary>
/// 串口连接事件。
/// </summary>
public class OpeningEventArgs : OperationEventArgs
{
    private readonly SerialPort serialPort;

    /// <summary>
    /// 构造函数
    /// </summary>
    public OpeningEventArgs(SerialPort serialPort)
    {
        this.serialPort = serialPort;
    }

    /// <summary>
    /// 新初始化的通信器
    /// </summary>
    public SerialPort SerialPort => serialPort;
}
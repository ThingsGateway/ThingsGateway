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
/// 串口连接接口
/// </summary>
public interface ISerialClientBase : IClient, ISender, IDefaultSender, IPluginObject, IRequsetInfoSender
{
    /// <summary>
    /// 缓存池大小
    /// </summary>
    int BufferLength { get; }

    /// <summary>
    /// 是否允许自由调用<see cref="SetDataHandlingAdapter"/>进行赋值。
    /// </summary>
    bool CanSetDataHandlingAdapter { get; }

    /// <summary>
    /// 串口描述
    /// </summary>
    SerialProperty SerialProperty { get; }
    /// <summary>
    /// 配置
    /// </summary>
    TouchSocketConfig Config { get; }

    /// <summary>
    /// 数据处理适配器
    /// </summary>
    SerialDataHandlingAdapter SerialDataHandlingAdapter { get; }

    /// <summary>
    /// 断开连接
    /// </summary>
    CloseEventHandler<ISerialClientBase> Closed { get; set; }

    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// <para>
    /// 当主动调用Close断开时，可通过<see cref="TouchSocketEventArgs.IsPermitOperation"/>终止断开行为。
    /// </para>
    /// </summary>
    CloseEventHandler<ISerialClientBase> Closing { get; set; }
    /// <summary>
    /// 表示是否为客户端。
    /// </summary>
    bool IsClient { get; }
    /// <summary>
    /// 主通信器
    /// </summary>
    SerialPort MainSerialPort { get; }

    /// <summary>
    /// 接收模式
    /// </summary>
    public ReceiveType ReceiveType { get; }

    /// <summary>
    /// 中断终端
    /// </summary>
    void Close();

    /// <summary>
    /// 中断终端，传递中断消息
    /// </summary>
    /// <param name="msg"></param>
    void Close(string msg);

    /// <summary>
    /// 设置数据处理适配器
    /// </summary>
    /// <param name="adapter"></param>
    void SetDataHandlingAdapter(SerialDataHandlingAdapter adapter);
}
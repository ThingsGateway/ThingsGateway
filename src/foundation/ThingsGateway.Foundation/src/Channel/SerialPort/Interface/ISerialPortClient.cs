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

using System.IO.Ports;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 串口客户端接口。
    /// </summary>
    public interface ISerialPortClient : IClient, ISender, IDefaultSender, IPluginObject, IRequsetInfoSender, ISetupConfigObject, IOnlineClient, IAdapterObject, IConnectObject, ICloseObject
    {
        /// <summary>
        /// 成功打开串口
        /// </summary>
        ConnectedEventHandler<ISerialPortClient> Connected { get; set; }

        /// <summary>
        /// 准备连接串口的时候
        /// </summary>
        SerialConnectingEventHandler<ISerialPortClient> Connecting { get; set; }

        /// <summary>
        /// 断开连接
        /// </summary>
        DisconnectEventHandler<ISerialPortClient> Disconnected { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// </para>
        /// </summary>
        DisconnectEventHandler<ISerialPortClient> Disconnecting { get; set; }

        /// <summary>
        /// 主通信器
        /// </summary>
        SerialPort MainSerialPort { get; }
    }
}
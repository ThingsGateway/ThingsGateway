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

namespace ThingsGateway.Foundation.Serial;

/// <summary>
/// <inheritdoc cref="ISerialSessionBase"/>
/// </summary>
public interface ISerialSession : ISerialSessionBase, IClientSender, IPluginObject
{
    /// <summary>
    /// 成功打开串口
    /// </summary>
    ConnectedEventHandler<ISerialSession> Connected { get; set; }

    /// <summary>
    /// 准备连接串口的时候
    /// </summary>
    SerialConnectingEventHandler<ISerialSession> Connecting { get; set; }

    /// <summary>
    /// 连接串口
    /// </summary>
    /// <exception cref="Exception"></exception>
    ISerialSession Connect();

    /// <summary>
    /// 配置服务器
    /// </summary>
    /// <param name="config"></param>
    /// <exception cref="Exception"></exception>
    ISerialSession Setup(TouchSocketConfig config);

}

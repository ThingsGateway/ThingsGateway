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

namespace ThingsGateway.Foundation.Serial;

/// <inheritdoc cref="ISerialClientBase"/>
public interface ISerialClient : ISerialClientBase, IClientSender, IPluginObject
{
    /// <summary>
    /// 成功连接
    /// </summary>
    MessageEventHandler<ISerialClient> Opened { get; set; }

    /// <summary>
    /// 准备连接的时候
    /// </summary>
    OpeningEventHandler<ISerialClient> Opening { get; set; }


    /// <summary>
    /// 连接串口
    /// </summary>
    /// <exception cref="Exception"></exception>
    ISerialClient Connect();

    /// <summary>
    /// 异步连接串口
    /// </summary>
    /// <exception cref="Exception"></exception>
    Task<ISerialClient> ConnectAsync();

    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="config"></param>
    /// <exception cref="Exception"></exception>
    ISerialClient Setup(TouchSocketConfig config);

}
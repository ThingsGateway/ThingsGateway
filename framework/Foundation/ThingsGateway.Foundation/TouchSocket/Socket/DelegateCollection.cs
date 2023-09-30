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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Net;

/// <summary>
/// 显示信息
/// </summary>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void MessageEventHandler<TClient>(TClient client, MsgPermitEventArgs e);

/// <summary>
/// 普通通知
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void TouchSocketEventHandler<TClient>(TClient client, PluginEventArgs e);

/// <summary>
/// Id修改通知
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void IdChangedEventHandler<TClient>(TClient client, IdChangedEventArgs e);

/// <summary>
/// Connecting
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void ConnectingEventHandler<TClient>(TClient client, ConnectingEventArgs e);

/// <summary>
/// Connected
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void ConnectedEventHandler<TClient>(TClient client, ConnectedEventArgs e);

/// <summary>
/// 客户端断开连接
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void DisconnectEventHandler<TClient>(TClient client, DisconnectEventArgs e);

/// <summary>
/// 接收数据
/// </summary>
/// <param name="client"></param>
/// <param name="byteBlock"></param>
/// <param name="requestInfo"></param>
public delegate void ReceivedEventHandler<TClient>(TClient client, ByteBlock byteBlock, IRequestInfo requestInfo);

/// <summary>
/// UDP接收
/// </summary>
/// <param name="endpoint"></param>
/// <param name="byteBlock"></param>
/// <param name="requestInfo"></param>
public delegate void UdpReceivedEventHandler(EndPoint endpoint, ByteBlock byteBlock, IRequestInfo requestInfo);
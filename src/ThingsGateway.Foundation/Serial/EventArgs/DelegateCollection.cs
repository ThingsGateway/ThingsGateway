using ThingsGateway.Foundation.Serial;


/// <summary>
/// Opening
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void OpeningEventHandler<TClient>(TClient client, OpeningEventArgs e);

/// <summary>
/// 串口断开
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void CloseEventHandler<TClient>(TClient client, CloseEventArgs e);

/// <summary>
/// 正在打开事件
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void OperationEventHandler<TClient>(TClient client, OperationEventArgs e);


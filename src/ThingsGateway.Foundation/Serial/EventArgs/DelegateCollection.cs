using ThingsGateway.Foundation.Serial;


/// <summary>
/// Opening
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void OpeningEventHandler<TClient>(TClient client, OpeningEventArgs e);

/// <summary>
/// ���ڶϿ�
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void CloseEventHandler<TClient>(TClient client, CloseEventArgs e);

/// <summary>
/// ���ڴ��¼�
/// </summary>
/// <typeparam name="TClient"></typeparam>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate void OperationEventHandler<TClient>(TClient client, OperationEventArgs e);


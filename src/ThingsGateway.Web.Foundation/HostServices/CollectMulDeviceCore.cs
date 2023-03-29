namespace ThingsGateway.Web.Foundation;
/// <summary>
/// <inheritdoc/>
/// <br></br>
///  未完成
/// </summary>
public class CollectMulDeviceCore : CollectDeviceCore
{
    /// <inheritdoc/>
    public CollectMulDeviceCore(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {

    }



    /// <inheritdoc/>
    public override void StartThread()
    {
        DeviceTask?.Start();
    }
    /// <inheritdoc/>
    public override void StopThread()
    {
    }




}


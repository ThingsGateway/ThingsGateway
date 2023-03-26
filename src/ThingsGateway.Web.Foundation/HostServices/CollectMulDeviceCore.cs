namespace ThingsGateway.Web.Foundation;


public class CollectMulDeviceCore : CollectDeviceCore
{
    public CollectMulDeviceCore(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {

    }



    public override void StartThread()
    {
        DeviceTask?.Start();
    }
    public override void StopThread()
    {
    }




}


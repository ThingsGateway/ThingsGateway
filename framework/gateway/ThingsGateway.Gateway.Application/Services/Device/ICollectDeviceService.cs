
namespace ThingsGateway.Gateway.Application
{
    public interface ICollectDeviceService : IDeviceService<CollectDevice>
    {
        Task CopyDevAndVarAsync(IEnumerable<CollectDevice> input);
        Task EditsAsync(List<CollectDevice> input);
        Task<IEnumerable<DeviceRunTime>> GetDeviceRuntimeAsync(long devId = 0);
    }
}
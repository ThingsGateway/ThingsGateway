
namespace ThingsGateway.Gateway.Application
{
    public interface IUploadDeviceService : IDeviceService<Device>
    {
        Task EditsAsync(List<Device> input);
        IEnumerable<DeviceRunTime> GetDeviceRuntime(long devId = 0);
    }
}
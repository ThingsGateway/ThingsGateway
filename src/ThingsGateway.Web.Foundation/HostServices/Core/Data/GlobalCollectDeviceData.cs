using System.Linq;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 采集设备值与状态全局提供
/// </summary>
public class GlobalCollectDeviceData : ISingleton
{
    public GlobalCollectDeviceData()
    {

    }
    /// <summary>
    /// 全局设备对象
    /// </summary>
    public ConcurrentList<CollectDeviceRunTime> CollectDevices { get; set; } = new();
    /// <summary>
    /// 全局设备变量对象
    /// </summary>
    public List<CollectVariableRunTime> CollectVariables
    {
        get
        {
            return CollectDevices?.SelectMany(it => it.DeviceVariableRunTimes).ToList();
        }
    }

}

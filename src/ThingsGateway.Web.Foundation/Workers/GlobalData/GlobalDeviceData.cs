#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Linq;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 采集设备值与状态全局提供
/// </summary>
public class GlobalDeviceData : ISingleton
{

    /// <summary>
    /// 全局设备对象
    /// </summary>
    public ConcurrentList<CollectDeviceRunTime> CollectDevices { get; set; } = new();
    /// <summary>
    /// 全局设备变量对象
    /// </summary>
    public List<DeviceVariableRunTime> AllVariables
    {
        get
        {
            return CollectDevices?.SelectMany(it => it.DeviceVariableRunTimes).Concat(MemoryVariables).ToList();
        }
    }
    /// <summary>
    /// 全局设备变量对象
    /// </summary>
    public ConcurrentList<DeviceVariableRunTime> MemoryVariables { get; internal set; } = new();

}

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

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 采集设备值与状态全局提供
/// </summary>
public class GlobalData : ISingleton
{
    /// <summary>
    /// 设备对象
    /// </summary>
    public ConcurrentList<CollectDeviceRunTime> CollectDevices { get; set; } = new();

    /// <summary>
    /// 设备对象
    /// </summary>
    public ConcurrentList<DeviceRunTime> BusinessDevices { get; set; } = new();

    /// <summary>
    /// 全部变量对象
    /// </summary>
    public IEnumerable<VariableRunTime> AllVariables => CollectDevices?.SelectMany(it => it.VariableRunTimes);
}
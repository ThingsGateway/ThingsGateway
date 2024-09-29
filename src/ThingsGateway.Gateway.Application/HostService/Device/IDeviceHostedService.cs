// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

public interface IDeviceHostedService
{
    /// <summary>
    /// 驱动列表
    /// </summary>
    IEnumerable<DriverBase> DriverBases { get; }

    /// <summary>
    /// 更新设备线程,切换为冗余通道
    /// </summary>
    /// <param name="deviceId">设备id</param>
    /// <returns></returns>
    Task DeviceRedundantThreadAsync(long deviceId);



    /// <summary>
    /// 获取驱动调试UI
    /// </summary>
    /// <param name="pluginName">驱动名称</param>
    /// <returns></returns>
    Type GetDebugUI(string pluginName);


    /// <summary>
    /// 获取驱动方法信息
    /// </summary>
    /// <param name="deviceId">设备id</param>
    /// <returns></returns>
    List<DriverMethodInfo> GetDriverMethodInfo(long deviceId);

    /// <summary>
    /// 获取驱动UI
    /// </summary>
    /// <param name="pluginName">驱动名称</param>
    /// <returns></returns>
    Type GetDriverUI(string pluginName);

    /// <summary>
    /// 暂停控制
    /// </summary>
    /// <param name="deviceId">设备id</param>
    /// <param name="isStart">是否继续</param>
    void PauseThread(long deviceId, bool isStart);

    /// <summary>
    /// 更新设备线程
    /// </summary>
    /// <param name="deviceId">设备Id</param>
    /// <param name="isChanged">是否重新获取组态数据</param>
    /// <param name="deleteCache">删除设备数据缓存</param>
    /// <returns></returns>
    Task RestartChannelThreadAsync(long deviceId, bool isChanged, bool deleteCache = false);
}

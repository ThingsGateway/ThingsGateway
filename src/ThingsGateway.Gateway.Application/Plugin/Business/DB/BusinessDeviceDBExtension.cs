//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public class BusinessDeviceDBExtension
{
    /// <summary>
    /// 按条件获取DB插件中的全部历史数据(不分页)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static async Task<OperResult<List<IDBHistoryValue>>> GetDBHistoryValuesAsync(string businessDeviceName, DBPageInput input)
    {
        var businessDevice = WorkerUtil.GetWoker<BusinessDeviceWorker>().DriverBases.Where(a => a is IDBHistoryService b).Where(a => a.DeviceName == businessDeviceName).Select(a => (IDBHistoryService)a).FirstOrDefault();
        if (businessDevice == null)
        {
            return new("业务设备不存在");
        }
        var data = await businessDevice.GetDBHistoryValuesAsync(input);
        return OperResult.CreateSuccessResult(data);
    }

    /// <summary>
    /// 按条件获取DB插件中的全部历史数据(不分页)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static async Task<OperResult<SqlSugarPagedList<IDBHistoryValue>>> GetDBHistoryValuePagesAsync(string businessDeviceName, DBPageInput input)
    {
        var businessDevice = WorkerUtil.GetWoker<BusinessDeviceWorker>().DriverBases.Where(a => a is IDBHistoryService b).Where(a => a.DeviceName == businessDeviceName).Select(a => (IDBHistoryService)a).FirstOrDefault();
        if (businessDevice == null)
        {
            return new("业务设备不存在");
        }
        var data = await businessDevice.GetDBHistoryValuePagesAsync(input);
        return OperResult.CreateSuccessResult(data);
    }
}
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

using Furion;

using Mapster;

using Microsoft.Extensions.Hosting;

using SqlSugar;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

using ThingsGateway.Foundation.Extension.Generic;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.SQLDB;

/// <summary>
/// MqttClient
/// </summary>
public partial class SQLDB : UpLoadDatabaseWithCacheT<DeviceData, SQLHistoryValue>
{
    private const string devType = "dev";
    private const string varType = "var";
    private readonly SQLDBProperty _driverPropertys = new();
    private readonly SQLDBVariableProperty _variablePropertys = new();
    private readonly EasyLock easyLock = new();

    private TimerTick _exRealTimerTick;
    private volatile bool success = true;

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<SQLHistoryValue> dev)
    {
        var data = dev.ChunkBetter(_driverPropertys.CacheItemCount);
        foreach (var item in data)
        {
            var cacheItem = new CacheItem()
            {
                Id = YitIdHelper.NextId(),
                Type = varType,
                Value = item.ToJsonString(),
            };
            cacheItems.Add(cacheItem);
        }
    }

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<DeviceData> dev)
    {
    }

    private async Task<OperResult> InserableAsync(SqlSugarClient db, List<SQLHistoryValue> dbInserts, CancellationToken cancellationToken)
    {
        try
        {
            var result = await db.Fastest<SQLHistoryValue>().PageSize(50000).SplitTable().BulkCopyAsync(dbInserts);
            //var result = await db.Insertable(dbInserts).SplitTable().ExecuteCommandAsync();
            if (result > 0)
            {
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
                LogMessage.Trace($"{FoundationConst.LogMessageHeader}主题：{nameof(SQLHistoryValue)}");
            }
            return OperResult.CreateSuccessResult();
        }
        catch (Exception ex)
        {
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999);
            return new(ex);
        }
    }
}
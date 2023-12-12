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

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.SQLHisAlarm;

/// <summary>
/// MqttClient
/// </summary>
public partial class SQLHisAlarm : UpLoadDatabaseWithCache
{
    private const string devType = "dev";
    private const string varType = "var";
    private readonly SQLHisAlarmProperty _driverPropertys = new();

    //private readonly SQLHisAlarmVariableProperty _variablePropertys = new();
    private readonly EasyLock easyLock = new();

    private ConcurrentQueue<HistoryAlarm> _alarmVariables = new();
    private volatile bool success = true;

    private void AddCache(List<CacheItem> cacheItems, IEnumerable<HistoryAlarm> dev)
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

    private void AddCache(IEnumerable<IEnumerable<HistoryAlarm>> devData, List<CacheItem> cacheItems)
    {
        try
        {
            foreach (var dev in devData)
            {
                AddCache(cacheItems, dev);
            }
        }
        catch (Exception ex)
        {
            LogMessage.LogWarning(ex, "缓存失败");
        }
    }

    /// <summary>
    /// 添加变量队列，超限后会入缓存
    /// </summary>
    /// <param name="variableData"></param>
    private void AddVariableQueue(HistoryAlarm variableData)
    {
        //检测队列长度，超限存入缓存数据库
        if (_alarmVariables.Count > _uploadPropertyWithCache.QueueMaxCount)
        {
            List<HistoryAlarm> list = null;
            lock (_alarmVariables)
            {
                if (_alarmVariables.Count > _uploadPropertyWithCache.QueueMaxCount)
                {
                    list = _alarmVariables.ToListWithDequeue();
                }
            }
            if (list != null)
            {
                var devData = list.ChunkBetter(_uploadPropertyWithCache.SplitSize);
                var cacheItems = new List<CacheItem>();
                AddCache(devData, cacheItems);

                if (cacheItems.Count > 0)
                    CacheDb.Cache.Insert(cacheItems);
            }
        }

        _alarmVariables.Enqueue(variableData);
    }

    private async Task<OperResult> InserableAsync(SqlSugarClient db, List<HistoryAlarm> dbInserts, CancellationToken cancellationToken)
    {
        try
        {
            //.SplitTable()
            var result = await db.Fastest<HistoryAlarm>().PageSize(50000).BulkCopyAsync(dbInserts);
            //var result = await db.Insertable(dbInserts).SplitTable().ExecuteCommandAsync();
            if (result > 0)
            {
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
                LogMessage.Trace($"{FoundationConst.LogMessageHeader}主题：{nameof(HistoryAlarm)}");
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
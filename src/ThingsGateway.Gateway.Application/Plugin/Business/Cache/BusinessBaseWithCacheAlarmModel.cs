//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，实现实体VarModel,AlarmModel缓存
/// </summary>
public abstract class BusinessBaseWithCacheAlarmModel<VarModel, DevModel, AlarmModel> : BusinessBaseWithCacheDevModel<VarModel, DevModel>
{
    protected ConcurrentQueue<CacheDBItem<AlarmModel>> _memoryAlarmModelQueue = new();

    private volatile bool LocalDBCacheAlarmModelInited;

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<AlarmModel>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                var dir = CacheDBUtil.GetFilePath(CurrentDevice.Id.ToString());
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_{typeof(AlarmModel).FullName}_{nameof(AlarmModel)}");
                var fullName = dir.CombinePathWithOs($"{fileStart}{CacheDBUtil.EX}");

                lock (fullName)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    using var cache = LocalDBCacheAlarmModel();
                    cache.DBProvider.Fastest<CacheDBItem<AlarmModel>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheAlarmModel();
                    lock (cache.CacheDBOption.FileFullName)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<AlarmModel>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueAlarmModel(CacheDBItem<AlarmModel> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<AlarmModel>> list = null;
                lock (_memoryAlarmModelQueue)
                {
                    if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryAlarmModelQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }
        if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryAlarmModelQueue)
            {
                if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    _memoryAlarmModelQueue.Clear();
                    _memoryAlarmModelQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryAlarmModelQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual CacheDB LocalDBCacheAlarmModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<AlarmModel>), CurrentDevice.Id.ToString(), $"{CurrentDevice.PluginName}_{typeof(AlarmModel).Name}");

        if (!LocalDBCacheAlarmModelInited)
        {
            cacheDb.InitDb();
            LocalDBCacheAlarmModelInited = true;
        }
        return cacheDb;
    }

    protected override async Task Update(CancellationToken cancellationToken)
    {
        await UpdateVarModelMemory(cancellationToken).ConfigureAwait(false);
        await UpdateDevModelMemory(cancellationToken).ConfigureAwait(false);
        await UpdateAlarmModelMemory(cancellationToken).ConfigureAwait(false);
        await UpdateVarModelCache(cancellationToken).ConfigureAwait(false);
        await UpdateDevModelCache(cancellationToken).ConfigureAwait(false);
        await UpdateAlarmModelCache(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateAlarmModel(IEnumerable<CacheDBItem<AlarmModel>> item, CancellationToken cancellationToken);

    protected async Task UpdateAlarmModelCache(CancellationToken cancellationToken)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            #region //成功上传时，补上传缓存数据

            if (IsConnected())
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        using var cache = LocalDBCacheAlarmModel();

                        //循环获取，固定读最大行数量，执行完成需删除行
                        var varList = await cache.DBProvider.Queryable<CacheDBItem<AlarmModel>>().Take(_businessPropertyWithCache.SplitSize).ToListAsync().ConfigureAwait(false);
                        if (varList.Any())
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    var result = await UpdateAlarmModel(varList, cancellationToken).ConfigureAwait(false);
                                    if (result.IsSuccess)
                                    {
                                        //删除缓存
                                        await cache.DBProvider.Deleteable<CacheDBItem<AlarmModel>>(varList).ExecuteCommandAsync().ConfigureAwait(false);
                                    }
                                    else
                                        break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (success)
                                    LogMessage?.LogWarning(ex);
                                success = false;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }

    protected async Task UpdateAlarmModelMemory(CancellationToken cancellationToken)
    {
        #region //上传设备内存队列中的数据

        try
        {
            var list = _memoryAlarmModelQueue.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_businessPropertyWithCache.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await UpdateAlarmModel(item, cancellationToken).ConfigureAwait(false);
                            if (!result.IsSuccess)
                            {
                                AddCache(item.ToList());
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (success)
                            LogMessage?.LogWarning(ex);
                        success = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }

        #endregion //上传设备内存队列中的数据
    }
}

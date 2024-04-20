
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，实现实体VarModel,DevModel缓存
/// </summary>
public abstract class BusinessBaseWithCacheDevModel<VarModel, DevModel> : BusinessBaseWithCacheVarModel<VarModel>
{
    protected ConcurrentQueue<CacheDBItem<DevModel>> _memoryDevModelQueue = new();

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<DevModel>> data)
    {
        if (data?.Count > 0)
        {
            using var cache = LocalDBCacheDevModel();
            cache.DBProvider.Fastest<CacheDBItem<DevModel>>().PageSize(50000).BulkCopy(data);
        }
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueDevModel(CacheDBItem<DevModel> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<DevModel>> list = null;
                lock (_memoryDevModelQueue)
                {
                    if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryDevModelQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }

        _memoryDevModelQueue.Enqueue(data);
    }

    private volatile bool LocalDBCacheDevModelInited;
    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual CacheDB LocalDBCacheDevModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<DevModel>), CurrentDevice.Id.ToString(), $"{CurrentDevice.PluginName}_{typeof(DevModel).FullName}_{nameof(DevModel)}");
        if (!LocalDBCacheDevModelInited)
        {
            cacheDb.InitDb();
            LocalDBCacheDevModelInited = true;
        }
        return cacheDb;
    }

    protected override async Task Update(CancellationToken cancellationToken)
    {
        await UpdateVarModelMemory(cancellationToken);
        await UpdateDevModelMemory(cancellationToken);
        await UpdateVarModelCache(cancellationToken);
        await UpdateDevModelCache(cancellationToken);
    }

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<OperResult> UpdateDevModel(IEnumerable<CacheDBItem<DevModel>> item, CancellationToken cancellationToken);

    protected async Task UpdateDevModelCache(CancellationToken cancellationToken)
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
                        using var cache = LocalDBCacheDevModel();

                        //循环获取
                        var varList = await cache.DBProvider.Queryable<CacheDBItem<DevModel>>().Take(_businessPropertyWithCache.SplitSize).ToListAsync();
                        if (varList.Any())
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    OperResult result = await UpdateDevModel(varList, cancellationToken);
                                    if (result.IsSuccess)
                                    {
                                        //删除缓存
                                        await cache.DBProvider.Deleteable<CacheDBItem<DevModel>>(varList).ExecuteCommandAsync();
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
                    LogMessage?.LogWarning(ex);
                    success = false;
                }
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }

    protected async Task UpdateDevModelMemory(CancellationToken cancellationToken)
    {
        #region //上传设备内存队列中的数据

        try
        {
            var list = _memoryDevModelQueue.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_businessPropertyWithCache.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            OperResult result = await UpdateDevModel(item, cancellationToken);
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
                        success = false;
                        LogMessage?.LogWarning(ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            success = false;
            LogMessage?.LogWarning(ex);
        }

        #endregion //上传设备内存队列中的数据
    }
}

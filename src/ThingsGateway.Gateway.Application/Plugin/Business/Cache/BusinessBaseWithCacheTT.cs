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

using System.Collections.Concurrent;

using ThingsGateway.Cache;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，实现实体T,T2缓存
/// </summary>
public abstract class BusinessBaseWithCacheTT<T, T2> : BusinessBaseWithCacheT<T>
{
    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual LiteDBCache<LiteDBDefalutCacheItem<T2>> LiteDBCacheT2 => LiteDBCacheUtil.GetDB<LiteDBDefalutCacheItem<T2>>(CurrentDevice.Id.ToString(), $"{CurrentDevice.PluginName}{typeof(T2).FullName}_{nameof(T2)}");

    protected ConcurrentQueue<LiteDBDefalutCacheItem<T2>> _memoryT2Queue = new();

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<LiteDBDefalutCacheItem<T2>> data)
    {
        if (data?.Count > 0)
            LiteDBCacheT2.AddRange(data);
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueT2(LiteDBDefalutCacheItem<T2> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryT2Queue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<LiteDBDefalutCacheItem<T2>> list = null;
                lock (_memoryT2Queue)
                {
                    if (_memoryT2Queue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryT2Queue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }

        _memoryT2Queue.Enqueue(data);
    }

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<OperResult> UpdateT2(IEnumerable<LiteDBDefalutCacheItem<T2>> item, CancellationToken cancellationToken);

    protected override async Task Update(CancellationToken cancellationToken)
    {
        await UpdateTMemory(cancellationToken);
        await UpdateT2Memory(cancellationToken);
        await UpdateTCache(cancellationToken);
        await UpdateT2Cache(cancellationToken);
    }

    protected async Task UpdateT2Memory(CancellationToken cancellationToken)
    {
        #region //上传设备内存队列中的数据

        try
        {
            var list = _memoryT2Queue.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_businessPropertyWithCache.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            OperResult result = await UpdateT2(item, cancellationToken);
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

    protected async Task UpdateT2Cache(CancellationToken cancellationToken)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            #region //成功上传时，补上传缓存数据

            if (success)
            {
                List<long> successIds = new();

                var successCount = 0;
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        //循环获取
                        var varList = LiteDBCacheT2.GetPage(successCount, _businessPropertyWithCache.SplitSize).ToList(); //按最大列表数量分页
                        if (varList?.Count != 0)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    OperResult result = await UpdateT2(varList, cancellationToken);
                                    if (result.IsSuccess)
                                    {
                                        successIds.AddRange(varList.Select(a => a.Id));
                                        successCount += _businessPropertyWithCache.SplitSize;
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
                if (successIds.Count > 0)
                    LiteDBCacheT2.DeleteMany(a => successIds.Contains(a.Id));
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }
}
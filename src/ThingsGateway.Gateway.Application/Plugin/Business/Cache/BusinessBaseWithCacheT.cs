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

using ThingsGateway.Cache;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件，实现实体T缓存
/// </summary>
public abstract class BusinessBaseWithCacheT<T> : BusinessBase
{
    protected volatile bool success;
    protected ConcurrentQueue<LiteDBDefalutCacheItem<T>> _memoryTQueue = new();
    protected override BusinessPropertyBase _businessPropertyBase => _businessPropertyWithCache;

    /// <summary>
    /// <inheritdoc cref="DriverPropertys"/>
    /// </summary>
    protected abstract BusinessPropertyWithCache _businessPropertyWithCache { get; }

    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual LiteDBCache<LiteDBDefalutCacheItem<T>> LiteDBCacheT => LiteDBCacheUtil.GetDB<LiteDBDefalutCacheItem<T>>(CurrentDevice.Id.ToString(), $"{CurrentDevice.PluginName}{typeof(T).FullName}_{nameof(T)}");

    protected override IProtocol? Protocol => null;

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<OperResult> UpdateT(IEnumerable<LiteDBDefalutCacheItem<T>> item, CancellationToken cancellationToken);

    protected virtual async Task Update(CancellationToken cancellationToken)
    {
        await UpdateTMemory(cancellationToken);
        await UpdateTCache(cancellationToken);
    }

    protected async Task UpdateTMemory(CancellationToken cancellationToken)
    {
        #region //上传变量内存队列中的数据

        try
        {
            var list = _memoryTQueue.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_businessPropertyWithCache.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            OperResult result = await UpdateT(item, cancellationToken);
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

        #endregion //上传变量内存队列中的数据
    }

    protected async Task UpdateTCache(CancellationToken cancellationToken)
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
                        var varList = LiteDBCacheT.GetPage(successCount, _businessPropertyWithCache.SplitSize).ToList(); //按最大列表数量分页
                        if (varList?.Count != 0)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    OperResult result = await UpdateT(varList, cancellationToken);
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
                    success = false;
                    LogMessage?.LogWarning(ex);
                }
                if (successIds.Count > 0)
                    LiteDBCacheT.DeleteMany(a => successIds.Contains(a.Id));
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<LiteDBDefalutCacheItem<T>> data)
    {
        if (data?.Count > 0)
            LiteDBCacheT.AddRange(data);
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueT(LiteDBDefalutCacheItem<T> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryTQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<LiteDBDefalutCacheItem<T>> list = null;
                lock (_memoryTQueue)
                {
                    if (_memoryTQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryTQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }

        _memoryTQueue.Enqueue(data);
    }
}
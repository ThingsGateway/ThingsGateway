
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
/// 业务插件，实现实体VarModel缓存
/// </summary>
public abstract class BusinessBaseWithCacheVarModel<VarModel> : BusinessBase
{
    protected ConcurrentQueue<CacheDBItem<VarModel>> _memoryVarModelQueue = new();
    protected volatile bool success = true;
    protected override BusinessPropertyBase _businessPropertyBase => _businessPropertyWithCache;

    protected abstract BusinessPropertyWithCache _businessPropertyWithCache { get; }

    protected override IProtocol? Protocol => null;

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<VarModel>> data)
    {
        if (data?.Count > 0)
        {
            using var cache = LiteDBCacheVarModel();
            cache.DBProvider.Fastest<CacheDBItem<VarModel>>().PageSize(50000).BulkCopy(data);
        }
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueVarModel(CacheDBItem<VarModel> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<VarModel>> list = null;
                lock (_memoryVarModelQueue)
                {
                    if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryVarModelQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }

        _memoryVarModelQueue.Enqueue(data);
    }

    /// <summary>
    /// 获取缓存对象，注意using
    /// </summary>
    protected virtual CacheDB LiteDBCacheVarModel()
    {
        return CacheDBUtil.GetCache(typeof(CacheDBItem<VarModel>), CurrentDevice.Id.ToString(), $"{CurrentDevice.PluginName}{typeof(VarModel).FullName}_{nameof(VarModel)}");
    }

    protected virtual async Task Update(CancellationToken cancellationToken)
    {
        await UpdateVarModelMemory(cancellationToken);
        await UpdateVarModelCache(cancellationToken);
    }

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task<OperResult> UpdateVarModel(IEnumerable<CacheDBItem<VarModel>> item, CancellationToken cancellationToken);

    protected async Task UpdateVarModelCache(CancellationToken cancellationToken)
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
                        //循环获取
                        using var cache = LiteDBCacheVarModel();
                        var varList = await cache.DBProvider.Queryable<CacheDBItem<VarModel>>().Take(_businessPropertyWithCache.SplitSize).ToListAsync();
                        if (varList.Any())
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    OperResult result = await UpdateVarModel(varList, cancellationToken);
                                    if (result.IsSuccess)
                                    {
                                        //删除缓存
                                        await cache.DBProvider.Deleteable<CacheDBItem<VarModel>>(varList).ExecuteCommandAsync();
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
            }

            #endregion //成功上传时，补上传缓存数据
        }
    }

    protected async Task UpdateVarModelMemory(CancellationToken cancellationToken)
    {
        #region //上传变量内存队列中的数据

        try
        {
            var list = _memoryVarModelQueue.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var data = list.ChunkBetter(_businessPropertyWithCache.SplitSize);
                foreach (var item in data)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            OperResult result = await UpdateVarModel(item, cancellationToken);
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
}
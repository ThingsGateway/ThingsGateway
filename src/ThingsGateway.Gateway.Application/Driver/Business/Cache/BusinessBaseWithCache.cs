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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务插件
/// </summary>
public abstract class BusinessBaseWithCache : BusinessBase
{

    protected sealed override BusinessPropertyBase _businessPropertyBase => _businessPropertyWithCache;

    protected abstract BusinessPropertyWithCache _businessPropertyWithCache { get; }


    protected override Task DisposeAsync(bool disposing)
    {
        // 清空内存队列
        _memoryPluginEventDataModelQueue.Clear();
        _memoryAlarmModelQueue.Clear();
        _memoryDevModelQueue.Clear();
        _memoryDevModelsQueue.Clear();
        _memoryVarModelQueue.Clear();
        _memoryVarModelsQueue.Clear();
        return base.DisposeAsync(disposing);
    }

    #region 条件

    protected abstract bool PluginEventDataModelEnable { get; set; }
    protected abstract bool AlarmModelEnable { get; set; }
    protected abstract bool DevModelEnable { get; set; }
    protected abstract bool VarModelEnable { get; set; }
    protected internal override Task InitChannelAsync(ChannelObject channelObject, CancellationToken cancellationToken)
    {
        if (AlarmModelEnable)
        {
            DBCacheAlarm = LocalDBCacheAlarmModel();

        }
        if (PluginEventDataModelEnable)
        {
            DBCachePluginEventData = LocalDBCachePluginEventDataModel();
        }


        if (DevModelEnable)
        {
            DBCacheDev = LocalDBCacheDevModel();
            DBCacheDevs = LocalDBCacheDevModels();
        }

        if (VarModelEnable)
        {
            DBCacheVar = LocalDBCacheVarModel();
            DBCacheVars = LocalDBCacheVarModels();
        }

        return base.InitChannelAsync(channelObject, cancellationToken);
    }
    protected override Task ProtectedExecuteAsync(object? state, CancellationToken cancellationToken)
    {
        return Update(cancellationToken);
    }
    protected virtual Task Update(CancellationToken cancellationToken)
    {
        return Update(this, cancellationToken);

        static async PooledTask Update(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            if (@this.VarModelEnable)
            {
                await @this.UpdateVarModelMemory(cancellationToken).ConfigureAwait(false);
                await @this.UpdateVarModelsMemory(cancellationToken).ConfigureAwait(false);
            }

            if (@this.DevModelEnable)
            {
                await @this.UpdateDevModelMemory(cancellationToken).ConfigureAwait(false);
                await @this.UpdateDevModelsMemory(cancellationToken).ConfigureAwait(false);
            }

            if (@this.AlarmModelEnable)
            {
                await @this.UpdateAlarmModelMemory(cancellationToken).ConfigureAwait(false);
            }
            if (@this.PluginEventDataModelEnable)
            {
                await @this.UpdatePluginEventDataModelMemory(cancellationToken).ConfigureAwait(false);
            }
            if (@this.VarModelEnable)
            {
                await @this.UpdateVarModelCache(cancellationToken).ConfigureAwait(false);
                await @this.UpdateVarModelsCache(cancellationToken).ConfigureAwait(false);
            }

            if (@this.DevModelEnable)
            {
                await @this.UpdateDevModelCache(cancellationToken).ConfigureAwait(false);
                await @this.UpdateDevModelsCache(cancellationToken).ConfigureAwait(false);
            }

            if (@this.AlarmModelEnable)
            {
                await @this.UpdateAlarmModelCache(cancellationToken).ConfigureAwait(false);

            }
            if (@this.PluginEventDataModelEnable)
            {
                await @this.UpdatePluginEventDataModelCache(cancellationToken).ConfigureAwait(false);

            }
        }
    }
    #endregion

    #region alarm

    protected ConcurrentQueue<CacheDBItem<AlarmVariable>> _memoryAlarmModelQueue = new();
    protected ConcurrentQueue<CacheDBItem<PluginEventData>> _memoryPluginEventDataModelQueue = new();

    private volatile bool LocalDBCacheAlarmModelInited;
    private CacheDB DBCacheAlarm;
    private volatile bool LocalDBCachePluginEventDataModelInited;
    private CacheDB DBCachePluginEventData;
    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<AlarmVariable>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                LogMessage?.LogInformation($"Add {typeof(AlarmVariable).Name} data to file cache, count {data.Count}");
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name);
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_{typeof(AlarmVariable).FullName}_{nameof(AlarmVariable)}");
                var fullName = PathHelper.CombinePathReplace(dir, $"{fileStart}{CacheDBUtil.EX}");

                lock (cacheLock)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    using var cache = LocalDBCacheAlarmModel();
                    cache.DBProvider.Fastest<CacheDBItem<AlarmVariable>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheAlarmModel();
                    lock (cache.CacheDBOption)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<AlarmVariable>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<PluginEventData>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                LogMessage?.LogInformation($"Add {typeof(PluginEventData).Name} data to file cache, count {data.Count}");
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name);
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_{typeof(PluginEventData).FullName}_{nameof(PluginEventData)}");
                var fullName = PathHelper.CombinePathReplace(dir, $"{fileStart}{CacheDBUtil.EX}");

                lock (cacheLock)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    using var cache = LocalDBCachePluginEventDataModel();
                    cache.DBProvider.Fastest<CacheDBItem<PluginEventData>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCachePluginEventDataModel();
                    lock (cache.CacheDBOption)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<PluginEventData>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueAlarmModel(CacheDBItem<AlarmVariable> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryAlarmModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<AlarmVariable>> list = null;
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
                    LogMessage?.LogWarning($"{typeof(AlarmVariable).Name} Queue exceeds limit, clear old data. If it doesn't work as expected, increase {_businessPropertyWithCache.QueueMaxCount} or Enable cache");
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
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueuePluginDataModel(CacheDBItem<PluginEventData> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryPluginEventDataModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<PluginEventData>> list = null;
                lock (_memoryPluginEventDataModelQueue)
                {
                    if (_memoryPluginEventDataModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryPluginEventDataModelQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }
        if (_memoryPluginEventDataModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryPluginEventDataModelQueue)
            {
                if (_memoryPluginEventDataModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    LogMessage?.LogWarning($"{typeof(PluginEventData).Name} Queue exceeds limit, clear old data. If it doesn't work as expected, increase {_businessPropertyWithCache.QueueMaxCount} or Enable cache");
                    _memoryPluginEventDataModelQueue.Clear();
                    _memoryPluginEventDataModelQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryPluginEventDataModelQueue.Enqueue(data);
        }
    }


    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual CacheDB LocalDBCacheAlarmModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<AlarmVariable>), CurrentDevice.Name, $"{CurrentDevice.PluginName}_{typeof(AlarmVariable).Name}");

        if (!LocalDBCacheAlarmModelInited)
        {
            cacheDb.InitDb();
            LocalDBCacheAlarmModelInited = true;
        }
        return cacheDb;
    }

    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual CacheDB LocalDBCachePluginEventDataModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<PluginEventData>), CurrentDevice.Name, $"{CurrentDevice.PluginName}_{typeof(PluginEventData).Name}");

        if (!LocalDBCachePluginEventDataModelInited)
        {
            cacheDb.InitDb();
            LocalDBCachePluginEventDataModelInited = true;
        }
        return cacheDb;
    }

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateAlarmModel(List<CacheDBItem<AlarmVariable>> item, CancellationToken cancellationToken);


    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdatePluginEventDataModel(List<CacheDBItem<PluginEventData>> item, CancellationToken cancellationToken);


    protected Task UpdateAlarmModelCache(CancellationToken cancellationToken)
    {
        return UpdateAlarmModelCache(this, cancellationToken);

        static async PooledTask UpdateAlarmModelCache(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            if (@this._businessPropertyWithCache.CacheEnable)
            {
                #region //成功上传时，补上传缓存数据

                if (@this.IsConnected())
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            //循环获取，固定读最大行数量，执行完成需删除行
                            var varList = await @this.DBCacheAlarm.DBProvider.Queryable<CacheDBItem<AlarmVariable>>().Take(@this._businessPropertyWithCache.SplitSize).ToListAsync(cancellationToken).ConfigureAwait(false);
                            if (varList.Count != 0)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        var result = await @this.UpdateAlarmModel(varList, cancellationToken).ConfigureAwait(false);
                                        if (result.IsSuccess)
                                        {
                                            //删除缓存
                                            await @this.DBCacheAlarm.DBProvider.Deleteable<CacheDBItem<AlarmVariable>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
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
                                    if (@this.success)
                                        @this.LogMessage?.LogWarning(ex);
                                    @this.success = false;
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
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }


                #endregion //成功上传时，补上传缓存数据
            }
        }
    }
    protected Task UpdatePluginEventDataModelCache(CancellationToken cancellationToken)
    {
        return UpdatePluginEventDataModelCache(this, cancellationToken);

        static async PooledTask UpdatePluginEventDataModelCache(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            if (@this._businessPropertyWithCache.CacheEnable)
            {
                #region //成功上传时，补上传缓存数据

                if (@this.IsConnected())
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            //循环获取，固定读最大行数量，执行完成需删除行
                            var varList = await @this.DBCachePluginEventData.DBProvider.Queryable<CacheDBItem<PluginEventData>>().Take(@this._businessPropertyWithCache.SplitSize).ToListAsync(cancellationToken).ConfigureAwait(false);
                            if (varList.Count != 0)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        var result = await @this.UpdatePluginEventDataModel(varList, cancellationToken).ConfigureAwait(false);
                                        if (result.IsSuccess)
                                        {
                                            //删除缓存
                                            await @this.DBCachePluginEventData.DBProvider.Deleteable<CacheDBItem<PluginEventData>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
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
                                    if (@this.success)
                                        @this.LogMessage?.LogWarning(ex);
                                    @this.success = false;
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
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }

                #endregion //成功上传时，补上传缓存数据
            }
        }
    }
    protected Task UpdateAlarmModelMemory(CancellationToken cancellationToken)
    {
        return UpdateAlarmModelMemory(this, cancellationToken);

        static async PooledTask UpdateAlarmModelMemory(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            #region //上传设备内存队列中的数据

            try
            {
                var list = @this._memoryAlarmModelQueue.ToListWithDequeue().ChunkBetter(@this._businessPropertyWithCache.SplitSize);
                foreach (var item in list)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await @this.UpdateAlarmModel(item, cancellationToken).ConfigureAwait(false);
                            if (!result.IsSuccess)
                            {
                                @this.AddCache(item);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (@this.success)
                    @this.LogMessage?.LogWarning(ex);
                @this.success = false;
            }

            #endregion //上传设备内存队列中的数据
        }
    }
    protected Task UpdatePluginEventDataModelMemory(CancellationToken cancellationToken)
    {
        return UpdatePluginEventDataModelMemory(this, cancellationToken);

        static async PooledTask UpdatePluginEventDataModelMemory(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            #region //上传设备内存队列中的数据


            try
            {
                var list = @this._memoryPluginEventDataModelQueue.ToListWithDequeue().ChunkBetter(@this._businessPropertyWithCache.SplitSize);
                foreach (var item in list)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await @this.UpdatePluginEventDataModel(item, cancellationToken).ConfigureAwait(false);
                            if (!result.IsSuccess)
                            {
                                @this.AddCache(item);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (@this.success)
                    @this.LogMessage?.LogWarning(ex);
                @this.success = false;
            }
            #endregion //上传设备内存队列中的数据
        }
    }

    #endregion

    #region device

    protected ConcurrentQueue<CacheDBItem<DeviceBasicData>> _memoryDevModelQueue = new();
    protected ConcurrentQueue<CacheDBItem<List<DeviceBasicData>>> _memoryDevModelsQueue = new();

    private volatile bool LocalDBCacheDevModelInited;
    private volatile bool LocalDBCacheDevModelsInited;

    private CacheDB DBCacheDev;
    private CacheDB DBCacheDevs;

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<DeviceBasicData>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                LogMessage?.LogInformation($"Add {typeof(DeviceBasicData).Name} data to file cache, count {data.Count}");
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name);
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_{typeof(DeviceBasicData).FullName}_{nameof(DeviceBasicData)}");
                var fullName = PathHelper.CombinePathReplace(dir, $"{fileStart}{CacheDBUtil.EX}");

                lock (cacheLock)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    using var cache = LocalDBCacheDevModel();
                    cache.DBProvider.Fastest<CacheDBItem<DeviceBasicData>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheDevModel();
                    lock (cache.CacheDBOption)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<DeviceBasicData>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }




    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<List<DeviceBasicData>>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name);
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_List_{typeof(DeviceBasicData).Name}");
                var fullName = PathHelper.CombinePathReplace(dir, $"{fileStart}{CacheDBUtil.EX}");

                lock (cacheLock)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    LocalDBCacheDevModelsInited = false;
                    using var cache = LocalDBCacheDevModels();
                    cache.DBProvider.Fastest<CacheDBItem<List<DeviceBasicData>>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheDevModels();
                    lock (cache.CacheDBOption)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<List<DeviceBasicData>>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }


    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueDevModels(CacheDBItem<List<DeviceBasicData>> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryDevModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<List<DeviceBasicData>>> list = null;
                lock (_memoryDevModelsQueue)
                {
                    if (_memoryDevModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryDevModelsQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }
        if (_memoryDevModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryDevModelsQueue)
            {
                if (_memoryDevModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    _memoryDevModelsQueue.Clear();
                    _memoryDevModelsQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryDevModelsQueue.Enqueue(data);
        }
    }
    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateDevModels(List<DeviceBasicData> item, CancellationToken cancellationToken);


    protected Task UpdateDevModelsCache(CancellationToken cancellationToken)
    {
        return UpdateDevModelsCache(this, cancellationToken);

        static async PooledTask UpdateDevModelsCache(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            if (@this._businessPropertyWithCache.CacheEnable)
            {
                #region //成功上传时，补上传缓存数据

                if (@this.IsConnected())
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            //循环获取
                            var varList = await @this.DBCacheDevs.DBProvider.Queryable<CacheDBItem<List<DeviceBasicData>>>().FirstAsync(cancellationToken).ConfigureAwait(false);
                            if (varList?.Value?.Count > 0)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        var result = await @this.UpdateDevModels(varList.Value, cancellationToken).ConfigureAwait(false);
                                        if (result.IsSuccess)
                                        {
                                            //删除缓存
                                            await @this.DBCacheDevs.DBProvider.DeleteableT<CacheDBItem<List<DeviceBasicData>>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
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
                                    if (@this.success)
                                        @this.LogMessage?.LogWarning(ex);
                                    @this.success = false;
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
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }

                #endregion //成功上传时，补上传缓存数据
            }
        }
    }


    protected Task UpdateDevModelsMemory(CancellationToken cancellationToken)
    {
        return UpdateDevModelsMemory(this, cancellationToken);

        static async PooledTask UpdateDevModelsMemory(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            #region //上传变量内存队列中的数据

            try
            {
                var queues = @this._memoryDevModelsQueue.ToListWithDequeue();
                foreach (var cacheDBItem in queues)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    var list = cacheDBItem.Value;
                    var data = list.ChunkBetter(@this._businessPropertyWithCache.SplitSize);
                    foreach (var item in data)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                var result = await @this.UpdateDevModels(item, cancellationToken).ConfigureAwait(false);
                                if (!result.IsSuccess)
                                {
                                    @this.AddCache(new List<CacheDBItem<List<DeviceBasicData>>>() { new CacheDBItem<List<DeviceBasicData>>(item) });
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (@this.success)
                                @this.LogMessage?.LogWarning(ex);
                            @this.success = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (@this.success)
                    @this.LogMessage?.LogWarning(ex);
                @this.success = false;
            }

            #endregion //上传变量内存队列中的数据
        }
    }





    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueDevModel(CacheDBItem<DeviceBasicData> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<DeviceBasicData>> list = null;
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
        if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryDevModelQueue)
            {
                if (_memoryDevModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    LogMessage?.LogWarning($"{typeof(DeviceBasicData).Name} Queue exceeds limit, clear old data. If it doesn't work as expected, increase {_businessPropertyWithCache.QueueMaxCount} or Enable cache");
                    _memoryDevModelQueue.Clear();
                    _memoryDevModelQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryDevModelQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual CacheDB LocalDBCacheDevModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<DeviceBasicData>), CurrentDevice.Name, $"{CurrentDevice.PluginName}_{typeof(DeviceBasicData).Name}");
        if (!LocalDBCacheDevModelInited)
        {
            cacheDb.InitDb();
            LocalDBCacheDevModelInited = true;
        }
        return cacheDb;
    }
    /// <summary>
    /// 获取缓存对象，注意每次获取的对象可能不一样，如顺序操作，需固定引用
    /// </summary>
    protected virtual CacheDB LocalDBCacheDevModels()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<List<DeviceBasicData>>), CurrentDevice.Name, $"{CurrentDevice.PluginName}_List_{typeof(DeviceBasicData).Name}");
        if (!LocalDBCacheDevModelsInited)
        {
            cacheDb.InitDb();
            LocalDBCacheDevModelsInited = true;
        }
        return cacheDb;
    }
    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateDevModel(List<CacheDBItem<DeviceBasicData>> item, CancellationToken cancellationToken);

    protected Task UpdateDevModelCache(CancellationToken cancellationToken)
    {
        return UpdateDevModelCache(this, cancellationToken);

        static async PooledTask UpdateDevModelCache(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            if (@this._businessPropertyWithCache.CacheEnable)
            {
                #region //成功上传时，补上传缓存数据

                if (@this.IsConnected())
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            //循环获取
                            var varList = await @this.DBCacheDev.DBProvider.Queryable<CacheDBItem<DeviceBasicData>>().Take(@this._businessPropertyWithCache.SplitSize).ToListAsync(cancellationToken).ConfigureAwait(false);
                            if (varList.Count != 0)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        var result = await @this.UpdateDevModel(varList, cancellationToken).ConfigureAwait(false);
                                        if (result.IsSuccess)
                                        {
                                            //删除缓存
                                            await @this.DBCacheDev.DBProvider.Deleteable<CacheDBItem<DeviceBasicData>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
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
                                    if (@this.success)
                                        @this.LogMessage?.LogWarning(ex);
                                    @this.success = false;
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
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }

                #endregion //成功上传时，补上传缓存数据
            }
        }
    }

    protected Task UpdateDevModelMemory(CancellationToken cancellationToken)
    {
        return UpdateDevModelMemory(this, cancellationToken);

        static async PooledTask UpdateDevModelMemory(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            #region //上传设备内存队列中的数据

            try
            {
                var list = @this._memoryDevModelQueue.ToListWithDequeue().ChunkBetter(@this._businessPropertyWithCache.SplitSize);
                foreach (var item in list)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await @this.UpdateDevModel(item, cancellationToken).ConfigureAwait(false);
                            if (!result.IsSuccess)
                            {
                                @this.AddCache(item);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (@this.success)
                    @this.LogMessage?.LogWarning(ex);
                @this.success = false;
            }

            #endregion //上传设备内存队列中的数据
        }
    }




    #endregion

    #region variable

    protected ConcurrentQueue<CacheDBItem<VariableBasicData>> _memoryVarModelQueue = new();
    protected ConcurrentQueue<CacheDBItem<List<VariableBasicData>>> _memoryVarModelsQueue = new();
    protected volatile bool success = true;
    private volatile bool LocalDBCacheVarModelInited;
    private volatile bool LocalDBCacheVarModelsInited;
    private CacheDB DBCacheVar;
    private CacheDB DBCacheVars;



    protected object cacheLock = new();

    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<VariableBasicData>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                LogMessage?.LogInformation($"Add {typeof(VariableBasicData).Name} data to file cache, count {data.Count}");
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name);
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_{typeof(VariableBasicData).Name}");
                var fullName = PathHelper.CombinePathReplace(dir, $"{fileStart}{CacheDBUtil.EX}");

                lock (cacheLock)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    LocalDBCacheVarModelInited = false;
                    using var cache = LocalDBCacheVarModel();
                    cache.DBProvider.Fastest<CacheDBItem<VariableBasicData>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheVarModel();
                    lock (cache.CacheDBOption)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<VariableBasicData>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }
    /// <summary>
    /// 入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddCache(List<CacheDBItem<List<VariableBasicData>>> data)
    {
        if (_businessPropertyWithCache.CacheEnable && data?.Count > 0)
        {
            try
            {
                foreach (var item in data)
                {
                    item.Id = CommonUtils.GetSingleId();
                }
                var dir = CacheDBUtil.GetCacheFilePath(CurrentDevice.Name);
                var fileStart = CacheDBUtil.GetFileName($"{CurrentDevice.PluginName}_List_{typeof(VariableBasicData).Name}");
                var fullName = PathHelper.CombinePathReplace(dir, $"{fileStart}{CacheDBUtil.EX}");

                lock (cacheLock)
                {
                    bool s = false;
                    while (!s)
                    {
                        s = CacheDBUtil.DeleteCache(_businessPropertyWithCache.CacheFileMaxLength, fullName);
                    }
                    LocalDBCacheVarModelsInited = false;
                    using var cache = LocalDBCacheVarModels();
                    cache.DBProvider.Fastest<CacheDBItem<List<VariableBasicData>>>().PageSize(50000).BulkCopy(data);
                }
            }
            catch
            {
                try
                {
                    using var cache = LocalDBCacheVarModels();
                    lock (cache.CacheDBOption)
                    {
                        cache.DBProvider.Fastest<CacheDBItem<List<VariableBasicData>>>().PageSize(50000).BulkCopy(data);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "Add cache fail");
                }
            }
        }
    }
    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueVarModel(CacheDBItem<VariableBasicData> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<VariableBasicData>> list = null;
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
        if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryVarModelQueue)
            {
                if (_memoryVarModelQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    LogMessage?.LogWarning($"{typeof(VariableBasicData).Name} Queue exceeds limit, clear old data. If it doesn't work as expected, increase {_businessPropertyWithCache.QueueMaxCount} or Enable cache");
                    _memoryVarModelQueue.Clear();
                    _memoryVarModelQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryVarModelQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 添加队列，超限后会入缓存
    /// </summary>
    /// <param name="data"></param>
    protected virtual void AddQueueVarModels(CacheDBItem<List<VariableBasicData>> data)
    {
        if (_businessPropertyWithCache.CacheEnable)
        {
            //检测队列长度，超限存入缓存数据库
            if (_memoryVarModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
            {
                List<CacheDBItem<List<VariableBasicData>>> list = null;
                lock (_memoryVarModelsQueue)
                {
                    if (_memoryVarModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                    {
                        list = _memoryVarModelsQueue.ToListWithDequeue();
                    }
                }
                AddCache(list);
            }
        }
        if (_memoryVarModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
        {
            lock (_memoryVarModelsQueue)
            {
                if (_memoryVarModelsQueue.Count > _businessPropertyWithCache.QueueMaxCount)
                {
                    _memoryVarModelsQueue.Clear();
                    _memoryVarModelsQueue.Enqueue(data);
                    return;
                }
            }
        }
        else
        {
            _memoryVarModelsQueue.Enqueue(data);
        }
    }

    /// <summary>
    /// 获取缓存对象，注意using
    /// </summary>
    protected virtual CacheDB LocalDBCacheVarModel()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<VariableBasicData>), CurrentDevice.Name, $"{CurrentDevice.PluginName}_{typeof(VariableBasicData).Name}");
        if (!LocalDBCacheVarModelInited)
        {
            cacheDb.InitDb();
            LocalDBCacheVarModelInited = true;
        }
        return cacheDb;
    }

    /// <summary>
    /// 获取缓存对象，注意using
    /// </summary>
    protected virtual CacheDB LocalDBCacheVarModels()
    {
        var cacheDb = CacheDBUtil.GetCache(typeof(CacheDBItem<List<VariableBasicData>>), CurrentDevice.Name, $"{CurrentDevice.PluginName}_List_{typeof(VariableBasicData).Name}");
        if (!LocalDBCacheVarModelsInited)
        {
            cacheDb.InitDb();
            LocalDBCacheVarModelsInited = true;
        }
        return cacheDb;
    }

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateVarModel(List<CacheDBItem<VariableBasicData>> item, CancellationToken cancellationToken);

    /// <summary>
    /// 需实现上传到通道
    /// </summary>
    /// <param name="item"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<OperResult> UpdateVarModels(List<VariableBasicData> item, CancellationToken cancellationToken);

    protected Task UpdateVarModelCache(CancellationToken cancellationToken)
    {
        return UpdateVarModelCache(this, cancellationToken);

        static async PooledTask UpdateVarModelCache(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            if (@this._businessPropertyWithCache.CacheEnable)
            {
                #region //成功上传时，补上传缓存数据

                if (@this.IsConnected())
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            //循环获取

                            var varList = await @this.DBCacheVar.DBProvider.Queryable<CacheDBItem<VariableBasicData>>().Take(@this._businessPropertyWithCache.SplitSize).ToListAsync(cancellationToken).ConfigureAwait(false);
                            if (varList.Count != 0)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        var result = await @this.UpdateVarModel(varList, cancellationToken).ConfigureAwait(false);
                                        if (result.IsSuccess)
                                        {
                                            //删除缓存
                                            await @this.DBCacheVar.DBProvider.Deleteable<CacheDBItem<VariableBasicData>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
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
                                    if (@this.success)
                                        @this.LogMessage?.LogWarning(ex);
                                    @this.success = false;
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
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }

                #endregion //成功上传时，补上传缓存数据
            }
        }
    }

    protected Task UpdateVarModelsCache(CancellationToken cancellationToken)
    {
        return UpdateVarModelsCache(this, cancellationToken);

        static async PooledTask UpdateVarModelsCache(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            if (@this._businessPropertyWithCache.CacheEnable)
            {
                #region //成功上传时，补上传缓存数据

                if (@this.IsConnected())
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            //循环获取
                            var varList = await @this.DBCacheVars.DBProvider.Queryable<CacheDBItem<List<VariableBasicData>>>().FirstAsync(cancellationToken).ConfigureAwait(false);
                            if (varList?.Value?.Count > 0)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        var result = await @this.UpdateVarModels(varList.Value, cancellationToken).ConfigureAwait(false);
                                        if (result.IsSuccess)
                                        {
                                            //删除缓存
                                            await @this.DBCacheVars.DBProvider.DeleteableT<CacheDBItem<List<VariableBasicData>>>(varList).ExecuteCommandAsync(cancellationToken).ConfigureAwait(false);
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
                                    if (@this.success)
                                        @this.LogMessage?.LogWarning(ex);
                                    @this.success = false;
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
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }

                #endregion //成功上传时，补上传缓存数据
            }
        }
    }

    protected Task UpdateVarModelMemory(CancellationToken cancellationToken)
    {
        return UpdateVarModelMemory(this, cancellationToken);

        static async PooledTask UpdateVarModelMemory(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            #region //上传变量内存队列中的数据

            try
            {
                var list = @this._memoryVarModelQueue.ToListWithDequeue().ChunkBetter(@this._businessPropertyWithCache.SplitSize);
                foreach (var item in list)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var result = await @this.UpdateVarModel(item, cancellationToken).ConfigureAwait(false);
                            if (!result.IsSuccess)
                            {
                                @this.AddCache(item);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (@this.success)
                            @this.LogMessage?.LogWarning(ex);
                        @this.success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (@this.success)
                    @this.LogMessage?.LogWarning(ex);
                @this.success = false;
            }

            #endregion //上传变量内存队列中的数据
        }
    }

    protected Task UpdateVarModelsMemory(CancellationToken cancellationToken)
    {
        return UpdateVarModelsMemory(this, cancellationToken);

        static async PooledTask UpdateVarModelsMemory(BusinessBaseWithCache @this, CancellationToken cancellationToken)
        {
            #region //上传变量内存队列中的数据

            try
            {
                var queues = @this._memoryVarModelsQueue.ToListWithDequeue();
                foreach (var cacheDBItem in queues)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    var list = cacheDBItem.Value;
                    var data = list.ChunkBetter(@this._businessPropertyWithCache.SplitSize);
                    foreach (var item in data)
                    {
                        try
                        {

                            if (!cancellationToken.IsCancellationRequested)
                            {
                                var result = await @this.UpdateVarModels(item, cancellationToken).ConfigureAwait(false);
                                if (!result.IsSuccess)
                                {
                                    @this.AddCache(new List<CacheDBItem<List<VariableBasicData>>>() { new CacheDBItem<List<VariableBasicData>>(item) });
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (@this.success)
                                @this.LogMessage?.LogWarning(ex);
                            @this.success = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (@this.success)
                    @this.LogMessage?.LogWarning(ex);
                @this.success = false;
            }

            #endregion //上传变量内存队列中的数据
        }
    }

    #endregion

}

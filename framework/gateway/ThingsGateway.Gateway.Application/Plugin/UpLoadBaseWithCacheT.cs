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

using Mapster;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Gateway.Application;


/// <summary>
/// 上传插件
/// </summary>
public abstract class UpLoadBaseWithCacheT<DeviceT, VariableT> : UpLoadBase
{
    /// <summary>
    /// <inheritdoc/><br></br>
    /// 实现<see cref="_uploadPropertyWithCache"/>
    /// </summary>
    public override DriverPropertyBase DriverPropertys => _uploadPropertyWithCache;

    /// <summary>
    /// mapster配置
    /// </summary>
    protected virtual TypeAdapterConfig _config { get; set; }
    /// <summary>
    /// 是否需要设备上传，默认true
    /// </summary>
    protected virtual bool _device { get; } = true;
    /// <summary>
    /// <inheritdoc cref="DriverPropertys"/>
    /// </summary>
    protected abstract UploadPropertyWithCacheT _uploadPropertyWithCache { get; }

    /// <summary>
    /// 是否需要变量上传，默认true
    /// </summary>
    protected virtual bool _variable { get; } = true;

    /// <summary>
    /// 离线缓存
    /// </summary>
    protected LiteDBCache CacheDb { get; set; }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {

            _globalDeviceData?.AllVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);

            _globalDeviceData?.CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusChange -= DeviceStatusChange;
            });
            CacheDb?.Litedb?.SafeDispose();
            base.Dispose(disposing);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
    }

    public override void Init(DeviceRunTime device)
    {
        base.Init(device);
        if (_uploadPropertyWithCache.IsAllVariable)
        {
            device.DeviceVariableRunTimes = _globalDeviceData.AllVariables;
            CollectDevices = _globalDeviceData.CollectDevices.ToList();
        }
        else
        {
            var variables = _globalDeviceData.AllVariables.Where(a =>
  a.VariablePropertys.ContainsKey(device.Id)).ToList();
            device.DeviceVariableRunTimes = variables;
            CollectDevices = _globalDeviceData.CollectDevices.Where(a => device.DeviceVariableRunTimes.Select(b => b.DeviceId).Contains(a.Id)).ToList();
        }
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device">当前设备</param>
    /// <param name="client">链路，共享链路时生效</param>
    protected override void Init(ISenderClient client = null)
    {
        CacheDb = new LiteDBCache(DeviceId.ToString(), CurrentDevice.PluginName);
        if (!_uploadPropertyWithCache.IsInterval)
        {
            if (_device)
                CollectDevices.ForEach(a => { a.DeviceStatusChange += DeviceStatusChange; });
            if (_variable)
                CurrentDevice.DeviceVariableRunTimes.ForEach(a => { a.VariableValueChange += VariableValueChange; });
        }

        if (_uploadPropertyWithCache.CycleInterval <= 50) _uploadPropertyWithCache.CycleInterval = 50;
        if (_uploadPropertyWithCache.UploadInterval <= 100) _uploadPropertyWithCache.UploadInterval = 100;
        _exVariableTimerTick = new(_uploadPropertyWithCache.UploadInterval);
        _exDeviceTimerTick = new(_uploadPropertyWithCache.UploadInterval);
    }

    /// <inheritdoc/>
    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        _ = Task.Factory.StartNew(IntervalInsert);
        _ = Task.Factory.StartNew(CheckCacheDb);
        return base.ProtectedBeforStartAsync(cancellationToken);
    }
    #region 缓存操作

    /// <summary>
    /// 设备内存队列
    /// </summary>

    protected ConcurrentQueue<DeviceT> _collectDeviceRunTimes = new();
    /// <summary>
    /// 变量内存队列
    /// </summary>

    protected ConcurrentQueue<VariableT> _collectVariableRunTimes = new();

    protected TimerTick _exDeviceTimerTick;

    protected TimerTick _exVariableTimerTick;

    private CancellationToken _token;
    protected abstract void AddCache(List<CacheItem> cacheItems, IEnumerable<VariableT> dev);

    protected abstract void AddCache(List<CacheItem> cacheItems, IEnumerable<DeviceT> dev);

    /// <summary>
    /// 添加设备队列，超限后会入缓存
    /// </summary>
    /// <param name="deviceData"></param>
    private void AddDeviceQueue(DeviceT deviceData)
    {
        //检测队列长度，超限存入缓存数据库
        if (_collectDeviceRunTimes.Count > _uploadPropertyWithCache.QueueMaxCount)
        {
            List<DeviceT> list = null;
            lock (_collectDeviceRunTimes)
            {
                if (_collectDeviceRunTimes.Count > _uploadPropertyWithCache.QueueMaxCount)
                {
                    list = _collectDeviceRunTimes.ToListWithDequeue();
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

        _collectDeviceRunTimes.Enqueue(deviceData);

    }

    /// <summary>
    /// 添加变量队列，超限后会入缓存
    /// </summary>
    /// <param name="variableData"></param>
    private void AddVariableQueue(VariableT variableData)
    {
        //检测队列长度，超限存入缓存数据库
        if (_collectVariableRunTimes.Count > _uploadPropertyWithCache.QueueMaxCount)
        {
            List<VariableT> list = null;
            lock (_collectVariableRunTimes)
            {
                if (_collectVariableRunTimes.Count > _uploadPropertyWithCache.QueueMaxCount)
                {
                    list = _collectVariableRunTimes.ToListWithDequeue();

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

        _collectVariableRunTimes.Enqueue(variableData);

    }

    protected virtual void DeviceStatusChange(DeviceRunTime collectDeviceRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_device)
            if (_uploadPropertyWithCache?.IsInterval != true)
            {
                AddDeviceQueue(collectDeviceRunTime.Adapt<DeviceT>(_config ?? TypeAdapterConfig.GlobalSettings));
            }
    }

    protected virtual void VariableValueChange(DeviceVariableRunTime deviceVariableRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        if (_variable)
            if (_uploadPropertyWithCache?.IsInterval != true)
            {
                AddVariableQueue(deviceVariableRunTime.Adapt<VariableT>(_config ?? TypeAdapterConfig.GlobalSettings));
            }
    }

    private void AddCache(IEnumerable<IEnumerable<VariableT>> devData, List<CacheItem> cacheItems)
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
    private void AddCache(IEnumerable<IEnumerable<DeviceT>> devData, List<CacheItem> cacheItems)
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
    private async Task CheckCacheDb()
    {
        while (!_token.IsCancellationRequested)
        {
            try
            {
                CacheDb.DeleteOldData(_uploadPropertyWithCache.CahceMaxLength);
            }
            catch (Exception ex)
            {
                LogMessage.LogWarning(ex, "删除缓存失败");
            }
            await Delay(30000, _token);
        }
    }
    private async Task IntervalInsert()
    {
        while (!_token.IsCancellationRequested)
        {

            if (CurrentDevice?.KeepRun == false)
            {
                await Delay(_uploadPropertyWithCache.CycleInterval, _token);
                continue;
            }
            //间隔上传
            if (_uploadPropertyWithCache.IsInterval)
            {
                try
                {
                    if (_variable)
                        if (_exVariableTimerTick.IsTickHappen())
                        {
                            //间隔推送全部变量
                            var varList = CurrentDevice.DeviceVariableRunTimes.Adapt<List<VariableT>>(_config ?? TypeAdapterConfig.GlobalSettings);
                            foreach (var variableData in varList)
                            {
                                AddVariableQueue(variableData);
                            }
                        }

                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "添加队列失败");
                }
                try
                {
                    if (_device)
                        if (_exDeviceTimerTick.IsTickHappen())
                        {
                            var devList = CollectDevices.Adapt<List<DeviceT>>(_config ?? TypeAdapterConfig.GlobalSettings);
                            foreach (var devData in devList)
                            {
                                AddDeviceQueue(devData);
                            }
                        }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "添加队列失败");
                }
            }

            await Delay(_uploadPropertyWithCache.CycleInterval, _token);
        }
    }
    #endregion
}



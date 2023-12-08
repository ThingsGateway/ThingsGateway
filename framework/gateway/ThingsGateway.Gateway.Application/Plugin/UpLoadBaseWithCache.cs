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

using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 上传插件
/// </summary>
public abstract class UpLoadBaseWithCache : UpLoadBase
{
    /// <summary>
    /// <inheritdoc/><br></br>
    /// 实现<see cref="_uploadPropertyWithCache"/>
    /// </summary>
    public override DriverPropertyBase DriverPropertys => _uploadPropertyWithCache;

    /// <summary>
    /// <inheritdoc cref="DriverPropertys"/>
    /// </summary>
    protected abstract UploadPropertyWithCache _uploadPropertyWithCache { get; }

    /// <summary>
    /// 离线缓存
    /// </summary>
    protected LiteDBCache CacheDb { get; set; }

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

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            CacheDb?.Litedb?.SafeDispose();
            base.Dispose(disposing);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
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

        if (_uploadPropertyWithCache.CycleInterval <= 100) _uploadPropertyWithCache.CycleInterval = 100;
    }

    /// <inheritdoc/>
    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        _ = Task.Factory.StartNew(CheckCacheDb);
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    #region 缓存操作

    protected CancellationToken _token;

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

    #endregion
}
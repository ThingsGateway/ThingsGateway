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

using LiteDB;

using Mapster;

using Microsoft.Extensions.Logging;

using MQTTnet.Diagnostics;

using SqlSugar;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Sockets;

namespace ThingsGateway.Plugin.TDengineDB;

/// <summary>
/// SQLDB
/// </summary>
public partial class TDengineDB : UpLoadBaseWithCache<DeviceData, TDHistoryValue>
{
    protected override bool _device => false;
    protected override bool _variable => true;
    /// <inheritdoc/>
    public override Type DriverDebugUIType => null;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;
    protected override IReadWrite _readWrite => null;

    protected override UploadPropertyWithCache _uploadPropertyWithCache => _driverPropertys;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(TDengineDB)}";
    }

    protected override void Init(ISenderClient client = null)
    {
        _config = new TypeAdapterConfig();
        _config.ForType<DeviceVariableRunTime, TDHistoryValue>()
            .Map(dest => dest.Value, src => src.Value == null ? string.Empty : src.Value.ToString() ?? string.Empty);
        base.Init(client);
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        var db = GetDb();
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.InitTables(typeof(TDHistoryValue));
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        //获取设备连接状态
        if (IsConnected())
        {
            //更新设备活动时间
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
        }
        else
        {
            //if (!IsUploadBase)
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999);
        }
        CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1);

        var cacheItems = new List<CacheItem>();
        var db = GetDb();
        db.Ado.CancellationToken = cancellationToken;

        try
        {
            var list = _collectVariableRunTimes.ToListWithDequeue();
            if (list?.Count != 0)
            {
                var result = await InserableAsync(db, list, cancellationToken);
                if (success != result.IsSuccess)
                {
                    if (!result.IsSuccess)
                        LogMessage.Warning(result.ToString());
                    success = result.IsSuccess;
                }
                if (!result.IsSuccess)
                {
                    AddCache(cacheItems, list);
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        if (cacheItems.Count > 0)
            CacheDb.Cache.Insert(cacheItems);

        List<long> successIds = new();
        try
        {
            var varList = CacheDb.Cache.Find(a => a.Type == varType, 0, _driverPropertys.CacheSendCount).ToList();//最大100w条
            {
                var item = varList.SelectMany(a => a.Value.FromJsonString<List<TDHistoryValue>>()).ToList();
                if (item.Count != 0)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {

                            var result = await InserableAsync(db, item, cancellationToken);
                            if (success != result.IsSuccess)
                            {
                                if (!result.IsSuccess)
                                    LogMessage.Warning(result.ToString());
                                success = result.IsSuccess;
                            }
                            if (result.IsSuccess)
                                successIds.AddRange(varList.Select(a => a.Id));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        if (successIds.Count > 0)
            CacheDb.Cache.DeleteMany(a => successIds.Contains(a.Id));

        await Delay(_driverPropertys.CycleInterval, cancellationToken);
    }


}

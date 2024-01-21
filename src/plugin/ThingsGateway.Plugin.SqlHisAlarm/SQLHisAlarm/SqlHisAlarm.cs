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

using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.SqlHisAlarm;

/// <summary>
/// SqlHisAlarm
/// </summary>
public partial class SqlHisAlarm : BusinessBaseWithCacheT<HistoryAlarm>
{
    private readonly SqlHisAlarmVariableProperty _variablePropertys = new();
    internal readonly SqlHisAlarmProperty _driverPropertys = new();
    private TypeAdapterConfig _config = new();
    public override Type DriverUIType => typeof(HisAlarmPage);

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCache _businessPropertyWithCache => _driverPropertys;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override bool IsConnected() => success;

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);
        CurrentDevice.VariableRunTimes = GlobalData.AllVariables.Where(a => a.AlarmEnable).ToList();
        CollectDevices = GlobalData.CollectDevices.Where(a => a.VariableRunTimes.Any(a => a.AlarmEnable)).ToList();
        _config.ForType<AlarmVariable, HistoryAlarm>().Map(dest => dest.Id, (src) => YitIdHelper.NextId());
        var alarmWorker = WorkerUtil.GetWoker<AlarmWorker>();
        alarmWorker.OnAlarmChanged += AlarmWorker_OnAlarmChanged;
    }

    protected override Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        db.CodeFirst.InitTables(typeof(HistoryAlarm));
        return base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        await Update(cancellationToken);

        await Delay(CurrentDevice.IntervalTime, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        var alarmWorker = WorkerUtil.GetWoker<AlarmWorker>();
        alarmWorker.OnAlarmChanged -= AlarmWorker_OnAlarmChanged;
        base.Dispose(disposing);
    }
}
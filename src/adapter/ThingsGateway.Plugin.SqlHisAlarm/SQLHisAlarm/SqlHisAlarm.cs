//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Mapster;

using SqlSugar;

using ThingsGateway.Admin.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.SqlHisAlarm;

/// <summary>
/// SqlHisAlarm
/// </summary>
public partial class SqlHisAlarm : BusinessBaseWithCacheVarModel<HistoryAlarm>
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
        CurrentDevice.VariableRunTimes = CurrentDevice.VariableRunTimes.Where(a => a.Value.AlarmEnable).ToDictionary();
        CollectDevices = CollectDevices
                                .Where(a => CurrentDevice.VariableRunTimes.Select(b => b.Value.DeviceId).Contains(a.Value.Id))
                                .ToDictionary();

        _config.ForType<AlarmVariable, HistoryAlarm>().Map(dest => dest.Id, (src) => YitIdHelper.NextId());
        HostedServiceUtil.AlarmHostedService.OnAlarmChanged += AlarmWorker_OnAlarmChanged;
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

        await Delay(cancellationToken);
    }

    internal async Task<QueryData<HistoryAlarm>> QueryData(QueryPageOptions option)
    {
        using var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var ret = new QueryData<HistoryAlarm>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any(),
            IsSearch = option.Searches.Any() || option.CustomerSearches.Any()
        };

        var query = db.GetQuery<HistoryAlarm>(option);

        if (option.IsPage)
        {
            RefAsync<int> totalCount = 0;

            var items = await query
                .ToPageListAsync(option.PageIndex, option.PageItems, totalCount);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else if (option.IsVirtualScroll)
        {
            RefAsync<int> totalCount = 0;

            var items = await query
                .ToPageListAsync(option.StartIndex, option.PageItems, totalCount);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else
        {
            var items = await query
                .ToListAsync();
            ret.TotalCount = items.Count;
            ret.Items = items;
        }
        return ret;
    }

    protected override void Dispose(bool disposing)
    {
        HostedServiceUtil.AlarmHostedService.OnAlarmChanged -= AlarmWorker_OnAlarmChanged;
        base.Dispose(disposing);
    }
}
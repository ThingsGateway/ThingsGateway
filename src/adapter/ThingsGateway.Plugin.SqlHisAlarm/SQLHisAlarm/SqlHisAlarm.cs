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
using ThingsGateway.Core;
using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.SqlHisAlarm;

/// <summary>
/// SqlHisAlarm
/// </summary>
public partial class SqlHisAlarm : BusinessBaseWithCacheVarModel<HistoryAlarm>, IDBHistoryAlarmService
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
        CurrentDevice.VariableRunTimes = CurrentDevice.VariableRunTimes.Where(a => a.Value.AlarmEnable).ToDictionary(a => a.Key, a => a.Value);
        CollectDevices = CollectDevices
                                .Where(a => CurrentDevice.VariableRunTimes.Select(b => b.Value.DeviceId).Contains(a.Value.Id))
                                .ToDictionary(a => a.Key, a => a.Value);

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
        await Update(cancellationToken).ConfigureAwait(false);

        await Delay(cancellationToken).ConfigureAwait(false);
    }

    protected override void Dispose(bool disposing)
    {
        HostedServiceUtil.AlarmHostedService.OnAlarmChanged -= AlarmWorker_OnAlarmChanged;
        base.Dispose(disposing);
    }

    #region 数据查询

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

    internal ISugarQueryable<HistoryAlarm> Query(DBHistoryAlarmPageInput input)
    {
        using var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var query = db.Queryable<HistoryAlarm>().SplitTable()
                             .WhereIF(input.StartTime != null, a => a.EventTime >= input.StartTime)
                           .WhereIF(input.EndTime != null, a => a.EventTime <= input.EndTime)
                           .WhereIF(!string.IsNullOrEmpty(input.VariableName), it => it.Name.Contains(input.VariableName))
                           .WhereIF(input.AlarmType != null, a => a.AlarmType == input.AlarmType)
                           .WhereIF(input.EventType != null, a => a.EventType == input.EventType)
                           ;

        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    public async Task<List<IDBHistoryAlarm>> GetDBHistoryAlarmsAsync(DBHistoryAlarmPageInput input)
    {
        var data = await Query(input).ToListAsync();
        return data.Cast<IDBHistoryAlarm>().ToList();
    }

    public async Task<SqlSugarPagedList<IDBHistoryAlarm>> GetDBHistoryAlarmPagesAsync(DBHistoryAlarmPageInput input)
    {
        var data = await Query(input).ToPagedListAsync<HistoryAlarm, IDBHistoryAlarm>(input.Current, input.Size);//分页
        return data;
    }

    #endregion 数据查询
}

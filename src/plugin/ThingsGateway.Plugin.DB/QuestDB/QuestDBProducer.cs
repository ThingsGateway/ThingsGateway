//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Mapster;

using SqlSugar;

using ThingsGateway.Core;
using ThingsGateway.Foundation;

namespace ThingsGateway.Plugin.QuestDB;

/// <summary>
/// QuestDBProducer
/// </summary>
public partial class QuestDBProducer : BusinessBaseWithCacheIntervalVarModel<QuestDBHistoryValue>, IDBHistoryValueService
{
    internal readonly QuestDBProducerProperty _driverPropertys = new();
    private readonly QuestDBProducerVariableProperty _variablePropertys = new();

    /// <inheritdoc/>
    public override Type DriverUIType => typeof(QuestDBPage);

    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _driverPropertys;

    public async Task<SqlSugarPagedList<IDBHistoryValue>> GetDBHistoryValuePagesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToPagedListAsync<QuestDBHistoryValue, IDBHistoryValue>(input.Current, input.Size).ConfigureAwait(false);//分页
        return data;
    }

    public async Task<List<IDBHistoryValue>> GetDBHistoryValuesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToListAsync().ConfigureAwait(false);
        return data.Cast<IDBHistoryValue>().ToList(); ;
    }

    protected override void Init(IChannel? channel = null)
    {
        _config = new TypeAdapterConfig();
        DateTime utcTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _config.ForType<VariableRunTime, QuestDBHistoryValue>()
            //.Map(dest => dest.Id, src => YitIdHelper.NextId())
            .Map(dest => dest.Id, src => src.Id)//Id更改为变量Id
            .Map(dest => dest.Value, src => src.Value == null ? string.Empty : src.Value.ToString() ?? string.Empty)
            .Map(dest => dest.CollectTime, (src) => src.CollectTime < DateTime.MinValue ? utcTime : src.CollectTime!.Value.ToUniversalTime())//注意sqlsugar插入时无时区，直接utc时间
            .Map(dest => dest.CreateTime, (src) => DateTime.UtcNow)
            ;//注意sqlsugar插入时无时区，直接utc时间

        base.Init(channel);
    }

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(QuestDBProducer)}";
    }

    internal ISugarQueryable<QuestDBHistoryValue> Query(DBHistoryValuePageInput input)
    {
        var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var query = db.Queryable<QuestDBHistoryValue>()
                             .WhereIF(input.StartTime != null, a => a.CreateTime >= input.StartTime)
                           .WhereIF(input.EndTime != null, a => a.CreateTime <= input.EndTime)
                           .WhereIF(!string.IsNullOrEmpty(input.VariableName), it => it.Name.Contains(input.VariableName))
                           .WhereIF(input.VariableNames != null, it => input.VariableNames.Contains(it.Name))
                           ;

        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.Id, OrderByType.Desc);//排序

        return query;
    }

    internal async Task<QueryData<QuestDBHistoryValue>> QueryData(QueryPageOptions option)
    {
        using var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var ret = new QueryData<QuestDBHistoryValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any() || option.CustomerSearches.Any(),
            IsSearch = option.Searches.Any()
        };

        var query = db.GetQuery<QuestDBHistoryValue>(option);

        if (option.IsPage)
        {
            RefAsync<int> totalCount = 0;

            var items = await query.ToPageListAsync(option.PageIndex, option.PageItems, totalCount).ConfigureAwait(false);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else if (option.IsVirtualScroll)
        {
            RefAsync<int> totalCount = 0;

            var items = await query.ToPageListAsync(option.StartIndex, option.PageItems, totalCount).ConfigureAwait(false);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else
        {
            var items = await query.ToListAsync().ConfigureAwait(false);
            ret.TotalCount = items.Count;
            ret.Items = items;
        }
        return ret;
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        await base.ProtectedBeforStartAsync(cancellationToken).ConfigureAwait(false);
        var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.InitTables(typeof(QuestDBHistoryValue));
    }

    protected override async ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        await UpdateVarModelMemory(cancellationToken).ConfigureAwait(false);

        await UpdateVarModelCache(cancellationToken).ConfigureAwait(false);

        await Delay(cancellationToken).ConfigureAwait(false);
    }
}

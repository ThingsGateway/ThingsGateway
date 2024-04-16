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

namespace ThingsGateway.Plugin.TDengineDB;

/// <summary>
/// TDengineDBProducer
/// </summary>
public partial class TDengineDBProducer : BusinessBaseWithCacheIntervalVarModel<TDengineDBHistoryValue>, IDBHistoryValueService
{
    private readonly TDengineDBProducerVariableProperty _variablePropertys = new();
    internal readonly TDengineDBProducerProperty _driverPropertys = new();

    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _driverPropertys;

    /// <inheritdoc/>
    public override Type DriverUIType => typeof(TDengineDBPage);

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(TDengineDBProducer)}";
    }

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        #region 初始化

        _config = new TypeAdapterConfig();
        _config.ForType<VariableRunTime, TDengineDBHistoryValue>()
            .Map(dest => dest.Value, src => src.Value == null ? string.Empty : src.Value.ToString() ?? string.Empty)
            //.Map(dest => dest.Id, src => YitIdHelper.NextId())
            .Map(dest => dest.Id, src => src.Id)//Id更改为变量Id
            ;//注意sqlsugar插入时无时区，直接utc时间

        #endregion 初始化
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.InitTables(typeof(TDengineDBHistoryValue));
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        await UpdateVarModelMemory(cancellationToken);
        await UpdateVarModelCache(cancellationToken);

        await Delay(cancellationToken);
    }

    internal async Task<QueryData<TDengineDBHistoryValue>> QueryData(QueryPageOptions option)
    {
        using var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var ret = new QueryData<TDengineDBHistoryValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any(),
            IsSearch = option.Searches.Any() || option.CustomerSearches.Any()
        };

        var query = db.GetQuery<TDengineDBHistoryValue>(option);

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

    internal ISugarQueryable<TDengineDBHistoryValue> Query(DBHistoryValuePageInput input)
    {
        var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        var query = db.Queryable<TDengineDBHistoryValue>()
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

    public async Task<List<IDBHistoryValue>> GetDBHistoryValuesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToListAsync();
        return data.Cast<IDBHistoryValue>().ToList(); ;
    }

    public async Task<SqlSugarPagedList<IDBHistoryValue>> GetDBHistoryValuePagesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToPagedListAsync<TDengineDBHistoryValue, IDBHistoryValue>(input.Current, input.Size);//分页
        return data;
    }
}
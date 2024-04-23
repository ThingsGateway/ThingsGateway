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

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducer
/// </summary>
public partial class SqlDBProducer : BusinessBaseWithCacheIntervalVarModel<SQLHistoryValue>, IDBHistoryValueService
{
    private readonly SqlDBProducerVariableProperty _variablePropertys = new();
    internal readonly SqlDBProducerProperty _driverPropertys = new();

    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _driverPropertys;

    /// <inheritdoc/>
    public override Type DriverUIType => typeof(SqlDBPage);

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(SqlDBProducer)}";
    }

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        #region 初始化

        _config = new TypeAdapterConfig();
        _config.ForType<VariableRunTime, SQLHistoryValue>()
            //.Map(dest => dest.Id, (src) =>YitIdHelper.NextId())
            .Map(dest => dest.Id, src => src.Id)//Id更改为变量Id
            .Map(dest => dest.CreateTime, (src) => DateTime.Now);

        _exRealTimerTick = new(_driverPropertys.BusinessInterval);

        #endregion 初始化
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        if (_driverPropertys.IsHisDB)
            db.CodeFirst.InitTables(typeof(SQLHistoryValue));
        if (_driverPropertys.IsReadDB)
            db.CodeFirst.As<SQLRealValue>(_driverPropertys.ReadDBTableName).InitTables<SQLRealValue>();
        //该功能索引名要加占位符
        //[SugarIndex("{table}index_codetable1_name",nameof(CodeFirstTable1.Name),OrderByType.Asc)]
        await base.ProtectedBeforStartAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_driverPropertys.IsReadDB)
        {
            if (_exRealTimerTick.IsTickHappen())
            {
                try
                {
                    var varList = CurrentDevice.VariableRunTimes.Values.Adapt<List<SQLRealValue>>();

                    var result = await UpdateAsync(varList, cancellationToken).ConfigureAwait(false);
                    if (result != null && success != result.IsSuccess)
                    {
                        if (!result.IsSuccess)
                            LogMessage.LogWarning(result.ToString());
                        success = result.IsSuccess;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex);
                }
            }
        }

        if (_driverPropertys.IsHisDB)
        {
            await UpdateVarModelMemory(cancellationToken).ConfigureAwait(false);
            await UpdateVarModelCache(cancellationToken).ConfigureAwait(false);
        }
        await Delay(cancellationToken).ConfigureAwait(false);
    }

    internal async Task<QueryData<SQLHistoryValue>> QueryHisData(QueryPageOptions option)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        var ret = new QueryData<SQLHistoryValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any(),
            IsSearch = option.Searches.Any() || option.CustomerSearches.Any()
        };

        var query = db.GetQuery<SQLHistoryValue>(option);

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

    internal async Task<QueryData<SQLRealValue>> QueryRealData(QueryPageOptions option)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        var ret = new QueryData<SQLRealValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any(),
            IsSearch = option.Searches.Any() || option.CustomerSearches.Any()
        };

        var query = db.GetQuery<SQLRealValue>(option);

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

    internal ISugarQueryable<SQLHistoryValue> Query(DBHistoryValuePageInput input)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        var query = db.Queryable<SQLHistoryValue>().SplitTable()
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
        return data.Cast<IDBHistoryValue>().ToList();
    }

    public async Task<SqlSugarPagedList<IDBHistoryValue>> GetDBHistoryValuePagesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToPagedListAsync<SQLHistoryValue, IDBHistoryValue>(input.Current, input.Size);//分页
        return data;
    }
}

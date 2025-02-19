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

using ThingsGateway.Admin.Application;
using ThingsGateway.Debug;
using ThingsGateway.Foundation;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.Plugin.DB;

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducer
/// </summary>
public partial class SqlDBProducer : BusinessBaseWithCacheIntervalVariableModel<SQLHistoryValue>, IDBHistoryValueService
{
    internal readonly SqlDBProducerProperty _driverPropertys = new();
    private readonly SqlDBProducerVariableProperty _variablePropertys = new();

    public override Type DriverPropertyUIType => typeof(SqlDBProducerPropertyRazor);

    /// <inheritdoc/>
    public override Type DriverUIType
    {
        get
        {
            if (_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty() && _driverPropertys.BigTextScriptRealTable.IsNullOrEmpty())
                return typeof(SqlDBPage);
            else
                return null;
        }
    }

    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _driverPropertys;

    public async Task<SqlSugarPagedList<IDBHistoryValue>> GetDBHistoryValuePagesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToPagedListAsync<SQLHistoryValue, IDBHistoryValue>(input.Current, input.Size).ConfigureAwait(false);//分页
        return data;
    }

    public async Task<List<IDBHistoryValue>> GetDBHistoryValuesAsync(DBHistoryValuePageInput input)
    {
        var data = await Query(input).ToListAsync().ConfigureAwait(false);
        return data.Cast<IDBHistoryValue>().ToList();
    }

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(SqlDBProducer)}";
    }

    internal async Task<QueryData<SQLHistoryValue>> QueryHistoryData(QueryPageOptions option)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        var ret = new QueryData<SQLHistoryValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };

        var query = db.Queryable<SQLHistoryValue>().SplitTable();
        query = db.GetQuery<SQLHistoryValue>(option, query);
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

    internal async Task<QueryData<SQLRealValue>> QueryRealData(QueryPageOptions option)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        var ret = new QueryData<SQLRealValue>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };

        var query = db.Queryable<SQLRealValue>().AS(_driverPropertys.ReadDBTableName);
        query = db.GetQuery<SQLRealValue>(option, query);

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

    protected override async Task InitChannelAsync(IChannel? channel = null)
    {
        _config = new TypeAdapterConfig();
        _config.ForType<VariableRuntime, SQLHistoryValue>()
            //.Map(dest => dest.Id, (src) =>CommonUtils.GetSingleId())
            .Map(dest => dest.Id, src => src.Id)//Id更改为变量Id
            .Map(dest => dest.Value, src => src.Value == null ? string.Empty : src.Value.ToString() ?? string.Empty)
            .Map(dest => dest.CreateTime, (src) => DateTime.Now);

        _exRealTimerTick = new(_driverPropertys.RealTableBusinessInterval);

        await base.InitChannelAsync(channel).ConfigureAwait(false);

    }

    protected override async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
        db.DbMaintenance.CreateDatabase();

        //必须为间隔上传
        if (!_driverPropertys.BigTextScriptHistoryTable.IsNullOrEmpty())
        {
            var hisModel = CSharpScriptEngineExtension.Do<IDynamicSQL>(_driverPropertys.BigTextScriptHistoryTable);
            if (_driverPropertys.IsHistoryDB)
            {
                var type = hisModel.GetModelType();
                db.CodeFirst.InitTables(type);
            }

        }
        else
        {
            if (_driverPropertys.IsHistoryDB)
                db.CodeFirst.InitTables(typeof(SQLHistoryValue));
        }
        if (!_driverPropertys.BigTextScriptRealTable.IsNullOrEmpty())
        {
            var realModel = CSharpScriptEngineExtension.Do<IDynamicSQL>(_driverPropertys.BigTextScriptRealTable);
            if (_driverPropertys.IsReadDB)
            {
                var type = realModel.GetModelType();
                db.CodeFirst.InitTables(type);
            }
        }
        else
        {
            if (_driverPropertys.IsReadDB)
                db.CodeFirst.As<SQLRealValue>(_driverPropertys.ReadDBTableName).InitTables<SQLRealValue>();
        }

        await base.ProtectedStartAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_driverPropertys.IsReadDB)
        {
            if (_exRealTimerTick.IsTickHappen())
            {
                try
                {
                    var varList = VariableRuntimes.Select(a => a.Value).Adapt<List<SQLRealValue>>();

                    var result = await UpdateAsync(varList, cancellationToken).ConfigureAwait(false);
                    if (success != result.IsSuccess)
                    {
                        if (!result.IsSuccess)
                            LogMessage.LogWarning(result.ToString());
                        success = result.IsSuccess;
                    }
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }
        }

        if (_driverPropertys.IsHistoryDB)
        {
            await UpdateVarModelMemory(cancellationToken).ConfigureAwait(false);
            await UpdateVarModelCache(cancellationToken).ConfigureAwait(false);
        }
    }

    private ISugarQueryable<SQLHistoryValue> Query(DBHistoryValuePageInput input)
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
}

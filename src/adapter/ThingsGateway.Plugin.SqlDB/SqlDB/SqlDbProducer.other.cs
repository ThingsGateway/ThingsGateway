//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using ThingsGateway.Admin.Application;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducer
/// </summary>
public partial class SqlDBProducer : BusinessBaseWithCacheIntervalVarModel<SQLHistoryValue>
{
    private TypeAdapterConfig _config;
    private TimeTick _exRealTimerTick;
    private volatile bool _initRealData;

    protected override void VariableChange(VariableRunTime variableRunTime, VariableData variable)
    {
        if (_driverPropertys.IsHisDB)
        {
            AddQueueVarModel(new(variableRunTime.Adapt<SQLHistoryValue>(_config)));
            base.VariableChange(variableRunTime, variable);
        }
    }

    protected override Task<OperResult> UpdateVarModel(IEnumerable<CacheDBItem<SQLHistoryValue>> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item.Select(a => a.Value), cancellationToken);
    }

    private async Task<OperResult> UpdateVarModel(IEnumerable<SQLHistoryValue> item, CancellationToken cancellationToken)
    {
        var result = await InserableAsync(item.ToList(), cancellationToken);
        if (success != result.IsSuccess)
        {
            if (!result.IsSuccess)
                LogMessage.LogWarning(result.ToString());
            success = result.IsSuccess;
        }

        return result;
    }

    #region 方法

    private async Task<OperResult> InserableAsync(List<SQLHistoryValue> dbInserts, CancellationToken cancellationToken)
    {
        try
        {
            var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
            db.Ado.CancellationToken = cancellationToken;
            var result = await db.Fastest<SQLHistoryValue>().PageSize(50000).SplitTable().BulkCopyAsync(dbInserts);
            //var result = await db.Insertable(dbInserts).SplitTable().ExecuteCommandAsync();
            if (result > 0)
            {
                LogMessage.Trace($"TableName：{nameof(SQLHistoryValue)}，Count：{result}");
            }
            return new();
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    private async Task<OperResult> UpdateAsync(List<SQLRealValue> datas, CancellationToken cancellationToken)
    {
        try
        {
            var db = SqlDBBusinessDatabaseUtil.GetDb(_driverPropertys);
            db.Ado.CancellationToken = cancellationToken;
            if (!_initRealData)
            {
                if (datas?.Count != 0)
                {
                    var result = await db.Storageable(datas).As(_driverPropertys.ReadDBTableName).PageSize(5000).ExecuteSqlBulkCopyAsync();
                    if (result > 0)
                        LogMessage.Trace($"TableName：{nameof(SQLRealValue)}{Environment.NewLine} ，Count：{result}");
                    _initRealData = true;
                    return new();
                }
                return null;
            }
            else
            {
                if (datas?.Count != 0)
                {
                    var result = await db.Fastest<SQLRealValue>().AS(_driverPropertys.ReadDBTableName).PageSize(100000).BulkUpdateAsync(datas);
                    LogMessage.Trace($"TableName：{nameof(SQLRealValue)}{Environment.NewLine} ，Count：{result}");
                    return new();
                }
                return null;
            }
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    #endregion 方法
}
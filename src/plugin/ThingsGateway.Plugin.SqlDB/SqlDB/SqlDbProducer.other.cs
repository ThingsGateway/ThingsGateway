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

using ThingsGateway.Cache;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducer
/// </summary>
public partial class SqlDBProducer : BusinessBaseWithCacheInterval<SQLHistoryValue>
{
    private TypeAdapterConfig _config;
    private TimeTick _exRealTimerTick;
    private volatile bool _initRealData;

    protected override void VariableChange(VariableRunTime variableRunTime)
    {
        if (_driverPropertys.IsHisDB)
        {
            AddQueueT(new(variableRunTime.Adapt<SQLHistoryValue>(_config)));
            base.VariableChange(variableRunTime);
        }
    }

    protected override Task<OperResult> UpdateT(IEnumerable<LiteDBDefalutCacheItem<SQLHistoryValue>> item, CancellationToken cancellationToken)
    {
        return UpdateT(item.Select(a => a.Data), cancellationToken);
    }

    private async Task<OperResult> UpdateT(IEnumerable<SQLHistoryValue> item, CancellationToken cancellationToken)
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
                LogMessage.Trace($"主题：{nameof(SQLHistoryValue)}，数量：{result}");
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
                        LogMessage.Trace($"主题：{nameof(SQLRealValue)}{Environment.NewLine} ，数量：{result}");
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
                    LogMessage.Trace($"主题：{nameof(SQLRealValue)}{Environment.NewLine} ，数量：{result}");
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
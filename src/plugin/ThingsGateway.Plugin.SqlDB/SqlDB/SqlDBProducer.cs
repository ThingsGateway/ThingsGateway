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

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.SqlDB;

/// <summary>
/// SqlDBProducer
/// </summary>
public partial class SqlDBProducer : BusinessBaseWithCacheInterval<SQLHistoryValue>
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
.Map(dest => dest.Id, (src) =>
YitIdHelper.NextId())
.Map(dest => dest.CreateTime, (src) => DateTime.Now);

        _exRealTimerTick = new(_driverPropertys.BusinessInterval);

        #endregion 初始化
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        db.CodeFirst.InitTables(typeof(SQLHistoryValue));
        db.MappingTables.Add(nameof(SQLRealValue), _driverPropertys.ReadDBTableName);
        db.CodeFirst.InitTables(typeof(SQLRealValue));
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_driverPropertys.IsReadDB)
        {
            if (_exRealTimerTick.IsTickHappen())
            {
                try
                {
                    var varList = CurrentDevice.VariableRunTimes.ToList().Adapt<List<SQLRealValue>>();

                    var result = await UpdateAsync(varList, cancellationToken);
                    if (success != result.IsSuccess)
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
            await UpdateTMemory(cancellationToken);
            await UpdateTCache(cancellationToken);
        }
        await Delay(CurrentDevice.IntervalTime, cancellationToken);
    }
}

//using ThingsGateway.Gateway.Application;
//using ThingsGateway.Plugin.DB;
//using ThingsGateway.Plugin.SqlDB;
//using ThingsGateway.SqlOrm;

//using TouchSocket.Core;

//public class TestSQL : DynamicSQLBase
//{
//    public override Task DBInit(ISqlOrmClient db, CancellationToken cancellationToken)
//    {
//        db.DbMaintenance.CreateDatabase();
//        db.CodeFirst.InitTables<ThingsGateway.Plugin.SqlDB.SQLHistoryValue>();
//        return Task.CompletedTask;
//    }

//    public override async Task DBInsertable(ISqlOrmClient db, IEnumerable<object> datas, CancellationToken cancellationToken)
//    {
//        var sQLHistoryValues = datas.Cast<VariableBasicData>().AdaptListSQLHistoryValue();
//        var result = await db.Fastest<SQLHistoryValue>().SplitTable().BulkCopyAsync(sQLHistoryValues).ConfigureAwait(false);
//        Logger?.Trace($"InsertTable ,Count：{result}");
//    }
//}
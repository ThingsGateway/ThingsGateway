// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------


//using SqlSugar;
//using ThingsGateway;
//using ThingsGateway.Foundation;
//using ThingsGateway.NewLife.Extension;

//public class TestRealSQL : IDynamicSQL
//{
//    public IEnumerable<dynamic> GetList(IEnumerable<object> datas)
//    {
//        var sqlRealValues = datas.Cast<SQLRealValue>().OrderByDescending(a => a.CollectTime).DistinctBy(a => a.Name);
//        List<RealModel1> demoDatas = new List<RealModel1>();
//        RealModel1 demoData = new RealModel1();
//        demoData.IsOnline = !sqlRealValues.Any(a => !a.IsOnline);
//        demoData.CollectTime = sqlRealValues.Select(a => a.CollectTime).Max();
//        demoData.EmptyId = 1;
//        var dict = sqlRealValues.ToDictionary(a => a.Name);
//        demoData.Temp1 = dict["Device1_Temp1"].Value;
//        demoData.Temp2 = dict["Device1_Temp2"].Value;
//        demoData.Temp3 = dict["Device1_Temp3"].Value;
//        demoDatas.Add(demoData);
//        return demoDatas;
//    }

//    public Type GetModelType()
//    {
//        return typeof(RealModel1);

//    }

//    [SugarTable(TableName = "RealModel1", TableDescription = "设备采集实时表")]
//    [SugarIndex("{table}_index_CollectTime", nameof(SQLRealValue.CollectTime), OrderByType.Desc)]
//    public class RealModel1
//    {
//        [SugarColumn(IsPrimaryKey = true)]
//        public long EmptyId { get; set; }

//        [SugarColumn(ColumnDescription = "采集时间")]
//        public DateTime CollectTime { get; set; }

//        ///<summary>
//        ///是否在线
//        ///</summary>
//        [SugarColumn(ColumnDescription = "是否在线")]
//        public bool IsOnline { get; set; }

//        public string Temp1 { get; set; }

//        public string Temp2 { get; set; }

//        public string Temp3 { get; set; }
//    }
//}

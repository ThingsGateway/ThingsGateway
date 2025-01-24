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

//public class TestHistorySQL : IDynamicSQL
//{
//    public IEnumerable<dynamic> GetList(IEnumerable<object> datas)
//    {
//        var sqlHistoryValues = datas.Cast<SQLHistoryValue>().OrderByDescending(a => a.CollectTime).DistinctBy(a => a.Name);
//        List<HistoryModel1> demoDatas = new List<HistoryModel1>();
//        HistoryModel1 demoData = new HistoryModel1();
//        demoData.IsOnline = !sqlHistoryValues.Any(a => !a.IsOnline);
//        demoData.CreateTime = DateTime.Now;
//        var dict = sqlHistoryValues.ToDictionary(a => a.Name);
//        demoData.Temp1 = dict["Device1_Temp1"].Value;
//        demoData.Temp2 = dict["Device1_Temp2"].Value;
//        demoData.Temp3 = dict["Device1_Temp3"].Value;
//        demoDatas.Add(demoData);
//        return demoDatas;
//    }

//    public Type GetModelType()
//    {
//        return typeof(HistoryModel1);
//    }
//    [SplitTable(SplitType.Month)]//（自带分表支持 年、季、月、周、日）
//    [SugarTable("HistoryModel1_{year}{month}{day}", TableDescription = "设备采集历史表")]//3个变量必须要有
//    [SugarIndex("index_CreateTime", nameof(SQLHistoryValue.CreateTime), OrderByType.Desc)]
//    public class HistoryModel1
//    {
//        [SplitField] //分表字段 在插入的时候会根据这个字段插入哪个表，在更新删除的时候用这个字段找出相关表
//        public DateTime CreateTime { get; set; }

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


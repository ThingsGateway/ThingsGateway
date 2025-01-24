//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using System.Reflection;
using System.Text.RegularExpressions;

namespace ThingsGateway.Plugin.SqlDB;

public class SqlDBDateSplitTableService : DateSplitTableService
{
    private SqlDBProducerProperty sqlDBProducerProperty;

    public SqlDBDateSplitTableService(SqlDBProducerProperty sqlDBProducerProperty)
    {
        this.sqlDBProducerProperty = sqlDBProducerProperty;
    }

    #region Core

    public override List<SplitTableInfo> GetAllTables(ISqlSugarClient db, EntityInfo EntityInfo, List<DbTableInfo> tableInfos)
    {
        CheckTableName(EntityInfo.DbTableName);
        string regex = "^" + EntityInfo.DbTableName.Replace("{year}", "([0-9]{2,4})").Replace("{day}", "([0-9]{1,2})").Replace("{month}", "([0-9]{1,2})").Replace("{name}", sqlDBProducerProperty.HistoryDBTableName);
        List<string> list = (from it in tableInfos
                             where Regex.IsMatch(it.Name, regex, RegexOptions.IgnoreCase)
                             select it.Name).Reverse().ToList();
        List<SplitTableInfo> list2 = new List<SplitTableInfo>();
        foreach (string item in list)
        {
            SplitTableInfo splitTableInfo = new SplitTableInfo();
            splitTableInfo.TableName = item;
            Match match = Regex.Match(item, regex, RegexOptions.IgnoreCase);
            string value = match.Groups[1].Value;
            string value2 = match.Groups[2].Value;
            string value3 = match.Groups[3].Value;
            splitTableInfo.Date = SqlDBDateSplitTableService.GetDate(value, value2, value3, EntityInfo.DbTableName);
            list2.Add(splitTableInfo);
        }

        return list2.OrderByDescending((SplitTableInfo it) => it.Date).ToList();
    }

    public override string GetTableName(ISqlSugarClient db, EntityInfo EntityInfo)
    {
        var splitTableAttribute = EntityInfo.Type.GetCustomAttribute<SplitTableAttribute>();
        if (splitTableAttribute != null)
        {
            var type = splitTableAttribute.SplitType;
            return GetTableName(db, EntityInfo, type);
        }
        else
        {
            return GetTableName(db, EntityInfo, SplitType.Day);
        }
    }

    public override string GetTableName(ISqlSugarClient db, EntityInfo EntityInfo, SplitType splitType)
    {
        var date = db.GetDate();
        return GetTableNameByDate(EntityInfo, splitType, date);
    }

    public override string GetTableName(ISqlSugarClient db, EntityInfo entityInfo, SplitType splitType, object fieldValue)
    {
        var value = Convert.ToDateTime(fieldValue);
        return GetTableNameByDate(entityInfo, splitType, value);
    }

    #endregion Core

    #region Private Models

    internal sealed class SplitTableSort
    {
        public string Name { get; set; }
        public int Sort { get; set; }
    }

    #endregion Private Models

    #region Common Helper

    private static void CheckTableName(string dbTableName)
    {
        Check.Exception(!dbTableName.Contains("{year}"), "table name need {{year}}", "分表表名需要占位符 {{year}}");
        Check.Exception(!dbTableName.Contains("{month}"), "table name need {{month}}", "分表表名需要占位符 {{month}} ");
        Check.Exception(!dbTableName.Contains("{day}"), "table name need {{day}}", "分表表名需要占位符{{day}}");
        Check.Exception(Regex.Matches(dbTableName, @"\{year\}").Count > 1, " There can only be one {{year}}", " 只能有一个 {{year}}");
        Check.Exception(Regex.Matches(dbTableName, @"\{month\}").Count > 1, "There can only be one {{month}}", "只能有一个 {{month}} ");
        Check.Exception(Regex.Matches(dbTableName, @"\{day\}").Count > 1, "There can only be one {{day}}", "只能有一个{{day}}");
        Check.Exception(Regex.IsMatch(dbTableName, @"\d\{|\}\d"), " '{{' or  '}}'  can't be numbers nearby", "占位符相令一位不能是数字,比如 : 1{{day}}2 错误 , 正确: 1_{{day}}_2");
    }

    private static DateTime GetDate(string group1, string group2, string group3, string dbTableName)
    {
        var yearIndex = dbTableName.IndexOf("{year}");
        var dayIndex = dbTableName.IndexOf("{day}");
        var monthIndex = dbTableName.IndexOf("{month}");
        List<SplitTableSort> tables = new List<SplitTableSort>();
        tables.Add(new SplitTableSort() { Name = "{year}", Sort = yearIndex });
        tables.Add(new SplitTableSort() { Name = "{day}", Sort = dayIndex });
        tables.Add(new SplitTableSort() { Name = "{month}", Sort = monthIndex });
        tables = tables.OrderBy(it => it.Sort).ToList();
        var year = "";
        var month = "";
        var day = "";
        if (tables[0].Name == "{year}")
        {
            year = group1;
        }
        if (tables[1].Name == "{year}")
        {
            year = group2;
        }
        if (tables[2].Name == "{year}")
        {
            year = group3;
        }
        if (tables[0].Name == "{month}")
        {
            month = group1;
        }
        if (tables[1].Name == "{month}")
        {
            month = group2;
        }
        if (tables[2].Name == "{month}")
        {
            month = group3;
        }
        if (tables[0].Name == "{day}")
        {
            day = group1;
        }
        if (tables[1].Name == "{day}")
        {
            day = group2;
        }
        if (tables[2].Name == "{day}")
        {
            day = group3;
        }
        return Convert.ToDateTime($"{year}-{month}-{day}");
    }

    private string GetTableNameByDate(EntityInfo EntityInfo, SplitType splitType, DateTime date)
    {
        date = ConvertDateBySplitType(date, splitType);
        return EntityInfo.DbTableName.Replace("{year}", date.Year + "").Replace("{day}", SqlDBDateSplitTableService.PadLeft2(date.Day + "")).Replace("{month}", SqlDBDateSplitTableService.PadLeft2(date.Month + "")).Replace("{name}", sqlDBProducerProperty.HistoryDBTableName);
    }

    private static string PadLeft2(string str)
    {
        if (str.Length < 2)
        {
            return str.PadLeft(2, '0');
        }
        else
        {
            return str;
        }
    }

    #endregion Common Helper

    #region Date Helper

    private static DateTime ConvertDateBySplitType(DateTime time, SplitType type)
    {
        switch (type)
        {
            case SplitType.Day:
                return Convert.ToDateTime(time.ToString("yyyy-MM-dd"));

            case SplitType.Week:
                return SqlDBDateSplitTableService.GetMondayDate(time);

            case SplitType.Month:
                return Convert.ToDateTime(time.ToString("yyyy-MM-01"));

            case SplitType.Season:
                if (time.Month <= 3)
                {
                    return Convert.ToDateTime(time.ToString("yyyy-01-01"));
                }
                else if (time.Month <= 6)
                {
                    return Convert.ToDateTime(time.ToString("yyyy-04-01"));
                }
                else if (time.Month <= 9)
                {
                    return Convert.ToDateTime(time.ToString("yyyy-07-01"));
                }
                else
                {
                    return Convert.ToDateTime(time.ToString("yyyy-10-01"));
                }
            case SplitType.Year:
                return Convert.ToDateTime(time.ToString("yyyy-01-01"));

            case SplitType.Month_6:
                if (time.Month <= 6)
                {
                    return Convert.ToDateTime(time.ToString("yyyy-01-01"));
                }
                else
                {
                    return Convert.ToDateTime(time.ToString("yyyy-07-01"));
                }
            default:
                throw new Exception($"SplitType parameter error ");
        }
    }

    private static DateTime GetMondayDate(DateTime someDate)
    {
        int i = someDate.DayOfWeek - DayOfWeek.Monday;
        if (i == -1) i = 6;
        TimeSpan ts = new TimeSpan(i, 0, 0, 0);
        return someDate.Subtract(ts);
    }

    #endregion Date Helper
}

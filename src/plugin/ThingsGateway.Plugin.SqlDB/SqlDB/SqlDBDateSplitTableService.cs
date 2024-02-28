//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using System.Reflection;

namespace ThingsGateway.Plugin.SqlDB;

public class SqlDBDateSplitTableService : DateSplitTableService
{
    private SqlDBProducerProperty sqlDBProducerProperty;

    public SqlDBDateSplitTableService(SqlDBProducerProperty sqlDBProducerProperty)
    {
        this.sqlDBProducerProperty = sqlDBProducerProperty;
    }

    #region Core

    public override string GetTableName(ISqlSugarClient db, EntityInfo EntityInfo)
    {
        var splitTableAttribute = EntityInfo.Type.GetCustomAttribute<SplitTableAttribute>();
        if (splitTableAttribute != null)
        {
            var type = (splitTableAttribute as SplitTableAttribute).SplitType;
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

    #region Common Helper

    private string GetTableNameByDate(EntityInfo EntityInfo, SplitType splitType, DateTime date)
    {
        date = ConvertDateBySplitType(date, splitType);
        return EntityInfo.DbTableName.Replace("{year}", date.Year + "").Replace("{day}", PadLeft2(date.Day + "")).Replace("{month}", PadLeft2(date.Month + "")).Replace("{name}", sqlDBProducerProperty.HisDBTableName);
    }

    private string PadLeft2(string str)
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

    private DateTime ConvertDateBySplitType(DateTime time, SplitType type)
    {
        switch (type)
        {
            case SplitType.Day:
                return Convert.ToDateTime(time.ToString("yyyy-MM-dd"));

            case SplitType.Week:
                return GetMondayDate(time);

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

    private DateTime GetMondayDate(DateTime someDate)
    {
        int i = someDate.DayOfWeek - DayOfWeek.Monday;
        if (i == -1) i = 6;
        TimeSpan ts = new TimeSpan(i, 0, 0, 0);
        return someDate.Subtract(ts);
    }

    #endregion Date Helper
}
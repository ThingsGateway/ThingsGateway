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

using Furion.DependencyInjection;

using SqlSugar;

using System.Collections;
using System.Data;
using System.Reflection;

using ThingsGateway.Admin.Core.JsonExtensions;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 对象拓展
/// </summary>
[SuppressSniffer]
public static class ListExtensions
{
    /// <summary>
    /// List转DataTable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static DataTable ToDataTable<T>(this List<T> list)
    {
        DataTable result = new();
        if (list.Count > 0)
        {
            var propertys = list[0].GetType().GetPropertiesWithCache();
            foreach (PropertyInfo pi in propertys)
            {
                Type colType = pi.PropertyType;
                if (colType.IsGenericType && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    colType = colType.GetGenericArguments().First();
                }
                if (IsIgnoreColumn(pi))
                    continue;
                if (IsJsonColumn(pi))//如果是json特性就是sting类型
                    colType = typeof(string);
                if (colType.IsEnum)//如果是Enum需要转string才会保存Enum字符串
                    colType = typeof(string);
                result.Columns.Add(pi.Name, colType);
            }
            for (int i = 0; i < list.Count; i++)
            {
                ArrayList tempList = new();
                foreach (PropertyInfo pi in propertys)
                {
                    if (IsIgnoreColumn(pi))
                        continue;
                    object obj = pi.GetValue(list[i], null);
                    if (IsJsonColumn(pi))//如果是json特性就是转化为json格式
                        obj = obj?.ToJsonString();//如果json字符串是空就传null
                    tempList.Add(obj);
                }
                object[] array = tempList.ToArray();
                result.LoadDataRow(array, true);
            }
        }
        return result;
    }

    /// <summary>
    /// SqlSugar是否忽略字段
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    private static bool IsIgnoreColumn(PropertyInfo pi)
    {
        return pi.GetCustomAttribute<SugarColumn>(false).IsIgnore == true;
    }
    /// <summary>
    /// SqlSugar是否Json字段
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    private static bool IsJsonColumn(PropertyInfo pi)
    {
        return pi.GetCustomAttribute<SugarColumn>(false).IsJson == true;
    }
}
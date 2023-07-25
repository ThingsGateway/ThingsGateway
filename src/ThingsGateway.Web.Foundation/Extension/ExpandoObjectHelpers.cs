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

using System.Dynamic;
using System.Linq;
using System.Reflection;

using ThingsGateway.Core;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 动态类型扩展
/// </summary>
public static class ExpandoObjectHelpers
{
    /// <summary>
    /// 反射动态类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expandoObject">动态对象</param>
    /// <param name="filter">是否过滤<see cref="ExcelAttribute"/></param>
    /// <returns></returns>
    public static T ConvertToEntity<T>(this ExpandoObject expandoObject, bool filter) where T : new()
    {
        var entity = new T();
        var properties = typeof(T).GetAllProps().Where(a => !filter || a.GetCustomAttribute<ExcelAttribute>() != null).ToDictionary(a => a.FindDisplayAttribute());

        expandoObject.ForEach(keyValuePair =>
        {
            if (properties.TryGetValue(keyValuePair.Key, out var property))
            {
                var value = keyValuePair.Value;
                var objValue = property.ObjToTypeValue(value?.ToString() ?? "");
                property.SetValue(entity, objValue);
            }

        });
        return entity;
    }

    /// <summary>
    /// 反射动态类型转换，获取某个实体属性的值,动态类型Key可以是<see cref="ObjectExtensions. FindDisplayAttribute(MemberInfo, Func{MemberInfo, string})"/>返回值
    /// </summary>
    public static object GetProperty<T>(this ExpandoObject expandoObject, string propertyName)
    {
        var properties = typeof(T).GetAllProps();
        var propertyDes = properties.FirstOrDefault(p => p.Name == propertyName).FindDisplayAttribute();
        return expandoObject.FirstOrDefault(a => a.Key == propertyDes).Value;
    }
    /// <summary>
    /// 查找出列表中的所有重复元素及重复数量
    /// </summary>
    public static Dictionary<string, int> QueryRepeatElementAndCountOfList<T>(this IEnumerable<IDictionary<string, object>> list, string name)
    {
        Dictionary<string, int> DicTmp = new Dictionary<string, int>();
        if (list != null && list.Any())
        {

            DicTmp = list.GroupBy(x => ((ExpandoObject)x).GetProperty<T>(name).ToString())
                         .Where(g => g.Count() > 1)
           .ToDictionary(x => x.Key, y => y.Count());
        }
        return DicTmp;
    }
    /// <summary>
    /// 查找出列表中的是否重复元素
    /// </summary>
    public static bool HasDuplicateElements<T>(this IEnumerable<IDictionary<string, object>> list, string name, string value)
    {
        if (list != null && list.Any())
        {
            var duplicates = list.Any(x => ((ExpandoObject)x).GetProperty<T>(name).ToString() == value);
            return duplicates;
        }
        return false;
    }
    /// <summary>
    /// 查找出列表中的所有重复元素及其下标
    /// </summary>
    public static Dictionary<string, List<int>> QueryRepeatElementAndIndicesOfList<T>(this IEnumerable<IDictionary<string, object>> list, string name)
    {
        Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();

        if (list != null && list.Any())
        {
            result = list.Select((item, index) => new { Item = item, Index = index })
                         .GroupBy(x => ((ExpandoObject)x.Item).GetProperty<T>(name).ToString())
                         .Where(g => g.Count() > 1)
                         .ToDictionary(x => x.Key, y => y.Select(z => z.Index).ToList());
        }

        return result;
    }
}
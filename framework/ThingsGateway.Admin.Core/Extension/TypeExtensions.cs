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

using SqlSugar;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 扩展方法
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// 获取 DisplayName属性名称
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="accessor"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static string Description<T>(this T item, Expression<Func<T, object>> accessor)
    {
        if (accessor.Body == null)
        {
            throw new ArgumentNullException(nameof(accessor));
        }

        var expression = accessor.Body;
        if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert && unaryExpression.Type == typeof(object))
        {
            expression = unaryExpression.Operand;
        }

        if (expression is not MemberExpression memberExpression)
        {
            throw new ArgumentException("只能访问字段或属性");
        }

        return typeof(T).GetDescription(memberExpression.Member.Name) ?? memberExpression.Member.Name;
    }

    /// <summary>
    /// 获得类型
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public static ICollection<FieldInfo> GetFieldsWithCache(this Type modelType)
    {
        var cacheKey = $"{nameof(GetFieldsWithCache)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{modelType.TypeHandle.Value}";
        FieldInfo[] fieldInfos = CacheStatic.Cache.GetOrCreate(cacheKey, entry =>
         {
             return modelType.GetFields();
         }, false);
        return fieldInfos;
    }

    /// <summary>
    /// 获得类型
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public static ICollection<PropertyInfo> GetPropertiesWithCache(this Type modelType)
    {
        var cacheKey = $"{nameof(GetPropertiesWithCache)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{modelType.TypeHandle.Value}";
        PropertyInfo[] propertyInfos = CacheStatic.Cache.GetOrCreate(cacheKey, entry =>
        {
            return modelType.GetProperties();
        }, false);
        return propertyInfos;
    }

    /// <summary>
    /// 获得属性名称
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetPropertyNamesWithCache(this Type modelType)
    {
        return modelType.GetPropertiesWithCache().Select(a => a.Name);
    }

    /// <summary>
    /// 获得类型属性的描述信息
    /// </summary>
    /// <param name="modelType"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetDescription(this Type modelType, string name)
    {
        var cacheKey = $"{nameof(GetDescription)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{name}-{modelType.TypeHandle.Value}";
        var str = CacheStatic.Cache.GetOrCreate(cacheKey, entry =>
        {
            string dn = default;
            {

                var props = modelType.GetPropertiesWithCache();
                var propertyInfo = props.FirstOrDefault(p => p.Name == name);
                if (propertyInfo != null)
                {
                    dn = FindDisplayAttribute(propertyInfo);
                }
                else
                {
                    var fields = modelType.GetFieldsWithCache();
                    var fieldInfo = fields.FirstOrDefault(p => p.Name == name);
                    dn = FindDisplayAttribute(fieldInfo);

                }
            }
            return dn ?? name;
        }, false);

        return str;
    }

    /// <summary>
    /// 获取成员的描述信息
    /// </summary>
    /// <param name="memberInfo"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static string FindDisplayAttribute(this MemberInfo memberInfo, Func<MemberInfo, string> func = null)
    {
        var dn = memberInfo.GetCustomAttribute<DisplayAttribute>(true)?.Name
            ?? memberInfo.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName
            ?? memberInfo.GetCustomAttribute<DescriptionAttribute>(true)?.Description
            ?? memberInfo.GetCustomAttribute<SugarColumn>(true)?.ColumnDescription
            ?? func?.Invoke(memberInfo)
            ?? memberInfo.Name
            ;

        return dn;
    }

    /// <summary>
    /// 获取枚举类型的所有项，返回集合
    /// </summary>
    public static List<(string name, string des, int value)> GetEnumList<T>(this T type) where T : Type
    {
        var enumType = type;
        List<(string, string, int)> list = new();
        var fieldInfos = enumType.GetFieldsWithCache().ToList();
        for (int i = 1; i < fieldInfos.Count; i++)
        {
            var fieldInfo = fieldInfos[i];
            var des = fieldInfo.FindDisplayAttribute();
            int value = (int)Enum.Parse(enumType, fieldInfo.Name);
            list.Add((fieldInfo.Name, des, value));
        }
        return list;
    }

    /// <summary>
    /// 获得类型属性的值
    /// </summary>
    /// <returns></returns>
    public static object GetMemberInfoValue(this Type modelType, object forObject, string fieldName)
    {
        object dn = default;
        var propertyInfo = modelType.GetPropertiesWithCache().FirstOrDefault(p => p.Name == fieldName);
        if (propertyInfo != null)
        {
            dn = propertyInfo.GetValue(forObject);
        }
        else
        {
            var fieldInfo = modelType.GetFieldsWithCache().FirstOrDefault(p => p.Name == fieldName); ;
            dn = fieldInfo.GetValue(forObject);

        }
        return dn;
    }


}
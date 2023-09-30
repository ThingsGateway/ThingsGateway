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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;


namespace ThingsGateway.Core;

/// <summary>
/// 扩展方法
/// </summary>
public static class TypeExtensions
{
    private static SysMemoryCache SysMemoryCache = new SysMemoryCache();
    /// <summary>
    /// 获取 DisplayName属性名称
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static string DescriptionWithOutSugar<T>(this T item, Expression<Func<T, object>> accessor, Func<MemberInfo, string> func = null)
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

        return typeof(T).GetDescriptionWithOutSugar(memberExpression.Member.Name, func) ?? memberExpression.Member.Name;
    }

    /// <summary>
    /// 获取成员的描述信息
    /// </summary>
    /// <param name="memberInfo"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static string FindDisplayAttributeWithOutSugar(this MemberInfo memberInfo, Func<MemberInfo, string> func = null)
    {
        var dn = memberInfo.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName
            ?? memberInfo.GetCustomAttribute<DescriptionAttribute>(true)?.Description
            ?? memberInfo.GetCustomAttribute<DisplayAttribute>(true)?.Description
            //?? memberInfo.GetCustomAttribute<SqlSugar.SugarColumn>(true)?.ColumnDescription
            ?? func?.Invoke(memberInfo)
            ?? memberInfo.Name
            ;

        return dn;
    }

    /// <summary>
    /// 获得类型属性的描述信息
    /// </summary>
    /// <returns></returns>
    public static string GetDescriptionWithOutSugar(this Type modelType, string name, Func<MemberInfo, string> func = null)
    {
        var cacheKey = $"{nameof(GetDescriptionWithOutSugar)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{name}-{modelType.TypeHandle.Value}";
        var str = SysMemoryCache.GetOrCreate(cacheKey, entry =>
        {
            string dn = default;
            {

                var props = modelType.GetPropertiesWithCache();
                var propertyInfo = props.FirstOrDefault(p => p.Name == name);
                if (propertyInfo != null)
                {
                    dn = FindDisplayAttributeWithOutSugar(propertyInfo, func);
                }
                else
                {
                    var fields = modelType.GetFieldsWithCache();
                    var fieldInfo = fields.FirstOrDefault(p => p.Name == name);
                    dn = FindDisplayAttributeWithOutSugar(fieldInfo, func);

                }
            }
            return dn ?? name;
        }, false);

        return str;

    }

    /// <summary>
    /// 获取枚举类型的所有项，返回集合
    /// </summary>
    public static List<EnumItem> GetEnumListWithOutSugar<T>(this T type, Func<MemberInfo, string> func = null) where T : Type
    {
        var enumType = type;
        List<EnumItem> list = new();
        var fieldInfos = enumType.GetFieldsWithCache().ToList();
        for (int i = 1; i < fieldInfos.Count; i++)
        {
            var fieldInfo = fieldInfos[i];
            var des = fieldInfo.FindDisplayAttributeWithOutSugar(func);
            int value = (int)Enum.Parse(enumType, fieldInfo.Name);
            list.Add(new(fieldInfo.Name, des, value));
        }
        return list;
    }

    /// <summary>
    /// 获得类型
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public static ICollection<FieldInfo> GetFieldsWithCache(this Type modelType)
    {
        var cacheKey = $"{nameof(GetFieldsWithCache)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{modelType.TypeHandle.Value}";
        FieldInfo[] fieldInfos = SysMemoryCache.GetOrCreate(cacheKey, entry =>
        {
            return modelType.GetFields();
        }, false);
        return fieldInfos;

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

    /// <summary>
    /// 获得类型
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public static ICollection<PropertyInfo> GetPropertiesWithCache(this Type modelType)
    {
        var cacheKey = $"{nameof(GetPropertiesWithCache)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{modelType.TypeHandle.Value}";
        PropertyInfo[] propertyInfos = SysMemoryCache.GetOrCreate(cacheKey, entry =>
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
}
/// <summary>
/// EnumItem
/// </summary>
public class EnumItem
{
    /// <inheritdoc/>
    public EnumItem(string name, string des, int value)
    {
        Name = name;
        Description = des;
        Value = value;
    }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public string Name { get; set; }
    /// <inheritdoc/>
    public int Value { get; }
}
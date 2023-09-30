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

namespace ThingsGateway.Admin.Core;

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
    /// 获取成员的描述信息
    /// </summary>
    /// <param name="memberInfo"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static string FindDisplayAttribute(this MemberInfo memberInfo, Func<MemberInfo, string> func = null)
    {
        var dn = memberInfo.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName
            ?? memberInfo.GetCustomAttribute<DescriptionAttribute>(true)?.Description
            ?? memberInfo.GetCustomAttribute<DisplayAttribute>(true)?.Description
            ?? memberInfo.GetCustomAttribute<SqlSugar.SugarColumn>(true)?.ColumnDescription
            ?? func?.Invoke(memberInfo)
            ?? memberInfo.Name
            ;

        return dn;
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
        var str = SysMemoryCache.GetOrCreate(cacheKey, entry =>
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
    /// 获取枚举类型的所有项，返回集合
    /// </summary>
    public static List<EnumItem> GetEnumList<T>(this T type) where T : Type
    {
        var enumType = type;
        List<EnumItem> list = new();
        var fieldInfos = enumType.GetFieldsWithCache().ToList();
        for (int i = 1; i < fieldInfos.Count; i++)
        {
            var fieldInfo = fieldInfos[i];
            var des = fieldInfo.FindDisplayAttribute();
            int value = (int)Enum.Parse(enumType, fieldInfo.Name);
            list.Add(new(fieldInfo.Name, des, value));
        }
        return list;
    }



}

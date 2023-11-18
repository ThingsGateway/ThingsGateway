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

using Furion;

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
    public static List<Func<MemberInfo, string>> DefaultFuncs { get; set; } = new();

    /// <summary>
    /// 获取 DisplayName属性名称
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static string Description<T>(this T item, Expression<Func<T, object>> accessor, Func<MemberInfo, string> func = null)
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

        return typeof(T).GetDescription(memberExpression.Member.Name, func) ?? memberExpression.Member.Name;
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
            //?? memberInfo.GetCustomAttribute<SqlSugar.SugarColumn>(true)?.ColumnDescription
            ?? func?.Invoke(memberInfo);
        if (dn == null)
        {
            foreach (var defaultFunc in DefaultFuncs)
            {
                dn = defaultFunc?.Invoke(memberInfo);
                if (dn != null)
                {
                    return dn;
                }
            }
        }
        dn ??= memberInfo.Name;
        return dn;
    }

    /// <summary>
    /// 获得类型属性的描述信息
    /// </summary>
    /// <returns></returns>
    public static string GetDescription(this Type modelType, string name, Func<MemberInfo, string> func = null)
    {
        var cacheKey = $"{nameof(GetDescription)}-{CultureInfo.CurrentUICulture.Name}-{modelType.FullName}-{name}-{modelType.TypeHandle.Value}";
        var str = App.GetService<MemoryCache>().GetOrCreate(cacheKey, entry =>
        {
            string dn = default;
            {
                var props = modelType.GetProperties();
                var propertyInfo = props.FirstOrDefault(p => p.Name == name);
                if (propertyInfo != null)
                {
                    dn = FindDisplayAttribute(propertyInfo, func);
                }
                else
                {
                    var fields = modelType.GetFields();
                    var fieldInfo = fields.FirstOrDefault(p => p.Name == name);
                    dn = FindDisplayAttribute(fieldInfo, func);

                }
            }
            return dn ?? name;
        }, false);
        return str;
    }

    /// <summary>
    /// 获取枚举类型的所有项，返回集合
    /// </summary>
    public static List<EnumItem> GetEnumList<T>(this T type, Func<MemberInfo, string> func = null) where T : Type
    {
        var enumType = type;
        List<EnumItem> list = new();
        var fieldInfos = enumType.GetFields().ToList();
        for (int i = 1; i < fieldInfos.Count; i++)
        {
            var fieldInfo = fieldInfos[i];
            var des = fieldInfo.FindDisplayAttribute(func);
            int value = (int)Enum.Parse(enumType, fieldInfo.Name);
            list.Add(new(fieldInfo.Name, des, value));
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
        var propertyInfo = modelType.GetProperties().FirstOrDefault(p => p.Name == fieldName);
        if (propertyInfo != null)
        {
            dn = propertyInfo.GetValue(forObject);
        }
        else
        {
            var fieldInfo = modelType.GetFields().FirstOrDefault(p => p.Name == fieldName); ;
            dn = fieldInfo.GetValue(forObject);
        }
        return dn;
    }

    /// <summary>
    /// 获得属性名称
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetPropertyNames(this Type modelType)
    {
        return modelType.GetProperties().Select(a => a.Name);
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
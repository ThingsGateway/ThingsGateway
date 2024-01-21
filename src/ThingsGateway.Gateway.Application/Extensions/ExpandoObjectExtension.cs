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
using System.Reflection;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Gateway.Application.Extensions;

/// <summary>
/// 动态类型扩展
/// </summary>
public static class ExpandoObjectExtension
{
    /// <summary>
    /// 反射动态类型转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expandoObject">动态对象</param>
    /// <param name="filter">是否过滤<see cref="IgnoreExcelAttribute"/></param>
    /// <returns></returns>
    public static T ConvertToEntity<T>(this ExpandoObject expandoObject, bool filter) where T : new()
    {
        ThingsGatewayStringConverter converter = new ThingsGatewayStringConverter();
        var entity = new T();
        var properties = typeof(T).GetProperties().Where(a => !filter || a.GetCustomAttribute<IgnoreExcelAttribute>() == null).ToDictionary(a => a.FindDisplayAttribute());

        expandoObject.ForEach(keyValuePair =>
        {
            if (properties.TryGetValue(keyValuePair.Key, out var property))
            {
                var value = keyValuePair.Value;
                property.PropertyType.GetTypeValue(value?.ToString(), out var objValue);
                property.SetValue(entity, objValue);
            }
        });
        return entity;
    }

    /// <summary>
    /// 反射动态类型转换，获取某个实体属性的值,动态类型Key可以是<see cref="Admin.Core.TypeExtensions. FindDisplayAttribute(MemberInfo, Func{MemberInfo, string})"/>返回值
    /// </summary>
    public static object GetProperty<T>(this ExpandoObject expandoObject, string propertyName)
    {
        var properties = typeof(T).GetProperties();
        var propertyDes = properties.FirstOrDefault(p => p.Name == propertyName).FindDisplayAttribute();
        return expandoObject.FirstOrDefault(a => a.Key == propertyDes).Value;
    }
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Dynamic; // 引入 System.Dynamic 命名空间
using System.Reflection;

namespace ThingsGateway.Foundation.Extension.Dynamic;

/// <summary>
/// 提供对动态类型的扩展方法
/// </summary>
public static class ExpandoObjectExtensions
{
    /// <summary>
    /// 将动态对象转换为指定类型实体
    /// </summary>
    /// <param name="expandoObject">动态对象</param>
    /// <param name="type">要转换的目标实体类型</param>
    /// <param name="properties"></param>
    /// <returns>转换后的实体对象</returns>
    public static object ConvertToEntity(this ExpandoObject expandoObject, Type type, Dictionary<string, PropertyInfo> properties)
    {
        var entity = Activator.CreateInstance(type);

        // 遍历动态对象的属性
        expandoObject.ForEach(keyValuePair =>
        {
            // 检查动态对象的属性是否存在于目标类型的属性中
            if (properties.TryGetValue(keyValuePair.Key, out var property))
            {
                var value = keyValuePair.Value; // 获取动态属性的值
                // 将动态属性值转换为目标属性类型并设置到目标对象的属性中
                property.SetValue(entity, ThingsGatewayStringConverter.Default.Deserialize(null, value?.ToString(), property.PropertyType));
            }
        });
        return entity; // 返回转换后的实体对象
    }

    /// <summary>
    /// 将动态对象转换为指定类型实体
    /// </summary>
    /// <typeparam name="T">要转换的目标实体类型</typeparam>
    /// <param name="expandoObject">动态对象</param>
    /// <param name="properties"></param>
    /// <returns>转换后的实体对象</returns>
    public static T ConvertToEntity<T>(this ExpandoObject expandoObject, Dictionary<string, PropertyInfo> properties) where T : new()
    {
        var entity = new T(); // 创建目标类型的实例

        // 遍历动态对象的属性
        expandoObject.ForEach(keyValuePair =>
        {
            // 检查动态对象的属性是否存在于目标类型的属性中
            if (properties.TryGetValue(keyValuePair.Key, out var property))
            {
                var value = keyValuePair.Value; // 获取动态属性的值
                // 将动态属性值转换为目标属性类型并设置到目标对象的属性中
                property.SetValue(entity, ThingsGatewayStringConverter.Default.Deserialize(null, value?.ToString(), property.PropertyType));
            }
        });
        return entity; // 返回转换后的实体对象
    }

    ///// <summary>
    ///// 获取动态对象中指定属性的值
    ///// </summary>
    ///// <typeparam name="T">动态对象的类型</typeparam>
    ///// <param name="expandoObject">动态对象</param>
    ///// <param name="propertyName">要获取值的属性名称</param>
    ///// <returns>属性的值</returns>
    //public static object GetProperty<T>(this ExpandoObject expandoObject, string propertyName)
    //{
    //    var type = typeof(T); // 获取动态对象的类型
    //    var properties = type.GetRuntimeProperties(); // 获取动态对象的所有属性
    //    var propertyDes = type.GetPropertyDisplayName(propertyName); // 获取指定属性的描述名称
    //    return expandoObject.FirstOrDefault(a => a.Key == propertyDes).Value; // 返回指定属性的值
    //}
}

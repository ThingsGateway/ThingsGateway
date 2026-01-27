//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ThingsGateway.Gateway.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class PluginServiceUtil
{
    /// <summary>
    /// 通过特定类型模型获取模型属性集合
    /// </summary>
    /// <param name="type">绑定模型类型</param>
    /// <returns></returns>
    public static List<IEditorItem> GetEditorItems(Type type)
    {
        if (type == null)
            return new();

        var cols = new List<IEditorItem>(50);
        //获取属性
        var props = type.GetProperties().Where(p => !p.IsStatic());
        foreach (var prop in props)
        {
            //获取插件自定义属性
            var classAttribute = prop.GetCustomAttribute<DynamicPropertyAttribute>(false);
            var autoGenerateColumnAttribute = prop.GetCustomAttribute<AutoGenerateColumnAttribute>(true);
            if (classAttribute == null) continue;//删除不需要显示的属性

            var displayName = classAttribute.Description ?? BootstrapBlazor.Components.Utility.GetDisplayName(type, prop.Name);
            DefaultTableColumn tc = new DefaultTableColumn(prop.Name, prop.PropertyType, displayName);
            if (autoGenerateColumnAttribute != null)
                IEditItemExtensions.CopyValue(tc, autoGenerateColumnAttribute);
            tc.ComponentParameters ??= new Dictionary<string, object>();
            if (classAttribute.Remark != null)
            {
                var dict = new Dictionary<string, object>
                {
                    { "title", classAttribute.Remark }
                };
                tc.ComponentParameters = tc.ComponentParameters.Concat(
                   [new("title", classAttribute.Remark)]
               );
            }
            if (!classAttribute.GroupName.IsNullOrEmpty())
            {
                tc.GroupName = classAttribute.GroupName;
            }
            cols.Add(tc);
        }
        return cols;
    }


    public static bool HasDynamicProperty(object model)
    {
        var type = model.GetType();
        return type.GetRuntimeProperties().Any(a => a.GetCustomAttribute<DynamicPropertyAttribute>(false) != null);
    }

    /// <summary>
    /// 插件是否支持平台
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSupported(Type type)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return !Attribute.IsDefined(type, typeof(OnlyWindowsSupportAttribute));

        return true;
    }

    /// <summary>
    /// 插件是否专业版
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsEducation(Type type)
    {
        if (type == null) return false;
        return Attribute.IsDefined(type, typeof(EducationPluginAttribute));
    }
    public static bool IsDisplay(Type type)
    {
        if (type == null) return false;
        return !Attribute.IsDefined(type, typeof(DisplayNonePluginAttribute));
    }

    /// <summary>
    /// 通过实体赋值到字典中
    /// </summary>
    public static Dictionary<long, Dictionary<string, string>> SetDict(NonBlockingDictionary<long, ModelValueValidateForm>? models)
    {
        Dictionary<long, Dictionary<string, string>> results = new();
        models ??= new();
        foreach (var model in models)
        {
            var data = SetDict(model.Value.Value);
            results.TryAdd(model.Key, data);
        }
        return results;
    }

    /// <summary>
    /// 通过实体赋值到字典中
    /// </summary>
    /// <param name="model"></param>
    public static Dictionary<string, string> SetDict(object model)
    {
        Type type = model.GetType();
        var properties = type.GetRuntimeProperties();
        Dictionary<string, string> dict = new();
        foreach (var property in properties)
        {
            var classAttribute = property.GetCustomAttribute<DynamicPropertyAttribute>(false);
            if (classAttribute == null) continue;
            string propertyName = property.Name;
            // 如果属性存在且可写
            if (property?.CanWrite == true)
            {
                // 进行类型转换
                var value = ThingsGatewayStringConverter.Default.Serialize(null, property.GetValue(model));
                dict.Add(propertyName, value);
            }
        }
        return dict;
    }

    /// <summary>
    /// 通过字典赋值到实体中
    /// </summary>
    /// <param name="model"></param>
    /// <param name="dict"></param>
    public static void SetModel(object model, Dictionary<string, string> dict)
    {
        Type type = model.GetType();
        var properties = type.GetRuntimeProperties();

        foreach (var property in properties)
        {
            var classAttribute = property.GetCustomAttribute<DynamicPropertyAttribute>(false);
            if (classAttribute == null) continue;

            string propertyName = property.Name;

            // 如果属性存在且可写
            if (property?.CanWrite == true)
            {
                if (dict.TryGetValue(propertyName, out var dictValue))
                {
                    // 进行类型转换
                    object value = ThingsGatewayStringConverter.Default.Deserialize(null, dictValue, property.PropertyType);
                    // 设置属性值
                    property.SetValue(model, value);
                }
            }
        }
    }
}

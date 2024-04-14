
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using BootstrapBlazor.Components;

using NewLife.Extension;

using System.Collections.Concurrent;
using System.Reflection;

namespace ThingsGateway.Gateway.Application;

public static class PluginServiceUtil
{
    /// <summary>
    /// 通过实体赋值到字典中
    /// </summary>
    /// <param name="model"></param>
    /// <param name="dict"></param>
    public static ConcurrentDictionary<long, Dictionary<string, string>> SetDict(ConcurrentDictionary<long, ModelValueValidateForm> models)
    {
        ConcurrentDictionary<long, Dictionary<string, string>> results = new();
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
    /// <param name="dict"></param>
    public static Dictionary<string, string> SetDict(object model)
    {
        Type type = model.GetType();
        var properties = type.GetRuntimeProperties();
        Dictionary<string, string> dict = new();
        foreach (var property in properties)
        {
            string propertyName = property.Name;
            // 如果属性存在且可写
            if (property != null && property.CanWrite)
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
            string propertyName = property.Name;

            // 如果属性存在且可写
            if (property != null && property.CanWrite)
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

    /// <summary>
    /// 通过特定类型模型获取模型属性集合
    /// </summary>
    /// <param name="type">绑定模型类型</param>
    /// <returns></returns>
    public static IEnumerable<IEditorItem> GetEditorItems(Type type)
    {
        var cols = new List<IEditorItem>(50);
        //获取属性
        var props = type.GetProperties().Where(p => !p.IsStatic());
        foreach (var prop in props)
        {
            //获取插件自定义属性
            var classAttribute = prop.GetCustomAttribute<DynamicPropertyAttribute>(false);
            if (classAttribute == null) continue;//删除不需要显示的属性
            IEditorItem? tc;
            var displayName = classAttribute.Description ?? BootstrapBlazor.Components.Utility.GetDisplayName(type, prop.Name);
            tc = new InternalEditorItem(prop.Name, prop.PropertyType, displayName);
            if (classAttribute.Remark != null)
            {
                var dict = new Dictionary<string, object>
                {
                    { "title", classAttribute.Remark }
                };
                tc.ComponentParameters = dict;
            }

            cols.Add(tc);
        }
        return cols;
    }

    /// <summary>
    /// 根据插件FullName获取插件主程序集名称和插件类型名称
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public static (string FileName, string TypeName) GetFileNameAndTypeName(string pluginName)
    {
        if (pluginName.IsNullOrWhiteSpace())
            return (string.Empty, string.Empty);
        // 查找最后一个 '.' 的索引
        int lastIndex = pluginName.LastIndexOf('.');

        // 如果找到了最后一个 '.'，并且它不是最后一个字符
        if (lastIndex != -1 && lastIndex < pluginName.Length - 1)
        {
            // 获取子串直到最后一个 '.'
            string part1 = pluginName.Substring(0, lastIndex);
            // 获取最后一个 '.' 后面的部分
            string part2 = pluginName.Substring(lastIndex + 1);
            return (part1, part2);
        }
        else
        {
            // 如果没有找到 '.'，或者 '.' 是最后一个字符，则返回默认的键和插件名称
            return (PluginService.DefaultKey, pluginName);
        }
    }

    /// <summary>
    /// 根据插件主程序集名称和插件类型名称获取插件FullName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetFullName(string fileName, string name)
    {
        return string.IsNullOrEmpty(fileName) ? name : $"{fileName}.{name}";
    }
}
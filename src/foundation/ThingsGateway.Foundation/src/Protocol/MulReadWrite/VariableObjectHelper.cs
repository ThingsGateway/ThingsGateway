//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Reflection;

using ThingsGateway.Foundation;

internal static class VariableObjectHelper
{
    private static ConcurrentDictionary<Type, Dictionary<string, VariableRuntimeProperty>> m_pairs = new ConcurrentDictionary<Type, Dictionary<string, VariableRuntimeProperty>>();

    public static Dictionary<string, VariableRuntimeProperty> GetPairs(Type type)
    {
        if (m_pairs.TryGetValue(type, out var value))
        {
            return value;
        }

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Dictionary<string, VariableRuntimeProperty> dictionary = new Dictionary<string, VariableRuntimeProperty>();
        PropertyInfo[] array = properties;
        foreach (PropertyInfo propertyInfo in array)
        {
            VariableRuntimeAttribute customAttribute = propertyInfo.GetCustomAttribute<VariableRuntimeAttribute>();
            if (customAttribute == null)
            {
                continue;
            }

            dictionary.Add(propertyInfo.Name, new VariableRuntimeProperty(customAttribute, propertyInfo));
        }

        m_pairs.TryAdd(type, dictionary);
        return dictionary;
    }
}
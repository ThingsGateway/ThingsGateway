//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;

/// <summary>
/// VariableObjectHelper
/// </summary>
public static class VariableObjectHelper
{
    /// <summary>
    /// GetVariableRuntimePropertyDict
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Dictionary<string, VariableRuntimeProperty> GetVariableRuntimePropertyDict(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var dictionary = new Dictionary<string, VariableRuntimeProperty>();
        foreach (var propertyInfo in properties)
        {
            VariableRuntimeAttribute variableRuntimeAttribute = propertyInfo.GetCustomAttribute<VariableRuntimeAttribute>();
            if (variableRuntimeAttribute == null)
            {
                continue;
            }
            dictionary.Add(propertyInfo.Name, new VariableRuntimeProperty(variableRuntimeAttribute, propertyInfo));
        }

        return dictionary;
    }
}

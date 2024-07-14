//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;

namespace ThingsGateway.Extension.Generic;

/// <inheritdoc/>
public static class GenericExtensions
{
    /// <summary>
    /// 把已修改的属性赋值到列表中，并返回字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="models"></param>
    /// <param name="oldModel"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static Dictionary<string, object?> GetDiffProperty<T>(this IEnumerable<T> models, T oldModel, T model)
    {
        // 获取Channel类型的所有公共属性
        var properties = typeof(T).GetRuntimeProperties();

        // 比较oldModel和model的属性，找出差异
        var differences = properties
            .Where(prop => prop.CanRead && prop.CanWrite) // 确保属性可读可写
            .Where(prop => !Equals(prop.GetValue(oldModel), prop.GetValue(model))) // 找出值不同的属性
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(model)); // 将属性名和新值存储到字典中

        // 应用差异到channels列表中的每个Channel对象
        foreach (var channel in models)
        {
            foreach (var difference in differences)
            {
                BootstrapBlazor.Components.Utility.SetPropertyValue(channel, difference.Key, difference.Value);
            }
        }

        return differences;
    }
}

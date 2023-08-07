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

using Furion.DependencyInjection;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 对象拓展
/// </summary>
[SuppressSniffer]
public static class JsonExtensions
{
    /// <summary>
    /// 转换为Json
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static string ToJsonString(this object item)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(item);
    }

    /// <summary>
    /// 从字符串到json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T ToJsonWithT<T>(this string json)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }

}
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

namespace ThingsGateway.Components;

/// <summary>
/// 过滤选择Model
/// </summary>
public class Filters
{
    /// <summary>
    /// DateTable Value
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// DateTable Text
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// 是否显示
    /// </summary>
    public bool Value { get; set; }
}
/// <summary>
/// 分页选择Model
/// </summary>
public class PageSize
{
    /// <summary>
    /// 显示
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// 值
    /// </summary>
    public int Value { get; set; }
}
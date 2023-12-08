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
/// 资源标识常量
/// </summary>
public static class BlazorResourceConst
{
    /// <summary>
    /// 资源默认路径
    /// </summary>
    public const string ResourceUrl = "/_content/ThingsGateway.Components/";

    /// <summary>
    /// 表格操作列标识
    /// </summary>
    public const string DataTableActions = "DataTableActions";

    /// <summary>
    /// 主题Cookie
    /// </summary>
    public const string ThemeCookieKey = "ThemeCookieKey";

    /// <summary>
    /// AppBarHeight
    /// </summary>
    public const int AppBarHeight = 48;

    /// <summary>
    /// Tab高度
    /// </summary>
    public const int PageTabsHeight = 36;

    /// <summary>
    /// FooterHeight
    /// </summary>
    public const int FooterHeight = 36;

    /// <summary>
    /// DefaultHeight
    /// </summary>
    public const int DefaultHeight = AppBarHeight + PageTabsHeight + FooterHeight + 12;
}
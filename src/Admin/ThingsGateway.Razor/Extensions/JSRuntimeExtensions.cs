//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.JSInterop;

namespace ThingsGateway.Razor;

/// <summary>
/// JSRuntime扩展方法
/// </summary>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class JSRuntimeExtensions
{
    /// <summary>
    /// 获取文化信息
    /// </summary>
    /// <param name="jsRuntime"></param>
    public static ValueTask<string> GetCulture(this IJSRuntime jsRuntime) => jsRuntime.InvokeAsync<string>("getCultureLocalStorage");

    /// <summary>
    /// 设置文化信息
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="cultureName"></param>
    public static ValueTask SetCulture(this IJSRuntime jsRuntime, string cultureName) => jsRuntime.InvokeVoidAsync("setCultureLocalStorage", cultureName);
}

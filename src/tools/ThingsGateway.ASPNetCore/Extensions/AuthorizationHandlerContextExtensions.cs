//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Authorization;

/// <summary>
/// 授权处理上下文拓展类
/// </summary>
public static class AuthorizationHandlerContextExtensions
{
    /// <summary>
    /// 获取当前 HttpContext 上下文
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static DefaultHttpContext GetCurrentHttpContext(this AuthorizationHandlerContext context)
    {
        DefaultHttpContext? httpContext;

        // 获取 httpContext 对象
        if (context.Resource is AuthorizationFilterContext filterContext) httpContext = (DefaultHttpContext)filterContext.HttpContext;
        else if (context.Resource is DefaultHttpContext defaultHttpContext) httpContext = defaultHttpContext;
        else httpContext = null;

        return httpContext;
    }
}

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;

namespace ThingsGateway.Server;

public abstract class AppAuthorizeHandler : IAuthorizationHandler
{
    /// <summary>
    /// 刷新 Token 身份标识
    /// </summary>
    private readonly string[] _refreshTokenClaims = new[] { "f", "e", "s", "l", "k" };

    /// <summary>
    /// 授权验证核心方法
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public virtual async Task HandleAsync(AuthorizationHandlerContext context)
    {
        // 判断是否授权
        var isAuthenticated = context.User.Identity?.IsAuthenticated;
        if (isAuthenticated == true)
        {
            // 禁止使用刷新 Token 进行单独校验
            if (_refreshTokenClaims.All(k => context.User.Claims.Any(c => c.Type == k)))
            {
                context.Fail();
                return;
            }

            await AuthorizeHandleAsync(context);
        }
        else context.GetCurrentHttpContext()?.SignoutToSwagger();    // 退出Swagger登录
    }

    /// <summary>
    /// 验证管道
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public virtual Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// 策略验证管道
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <param name="requirement"></param>
    /// <returns></returns>
    public virtual Task<bool> PolicyPipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext, IAuthorizationRequirement requirement)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// 授权处理
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected async Task AuthorizeHandleAsync(AuthorizationHandlerContext context)
    {
        // 获取所有未成功验证的需求
        var pendingRequirements = context.PendingRequirements;

        // 获取 HttpContext 上下文
        var httpContext = context.GetCurrentHttpContext();

        // 调用子类管道
        var pipeline = await PipelineAsync(context, httpContext);
        if (pipeline)
        {
            // 通过授权验证
            foreach (var requirement in pendingRequirements)
            {
                // 验证策略管道
                var policyPipeline = await PolicyPipelineAsync(context, httpContext, requirement);
                if (policyPipeline) context.Succeed(requirement);
                else context.Fail();
            }
        }
        else context.Fail();
    }
}
#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.Authorization;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;


namespace ThingsGateway.Web.Core
{
    /// <inheritdoc/>
    public class BlazorAuthorizeHandler : AppAuthorizeHandler
    {
        private SysCacheService _sysCacheService;
        private IServiceScope _serviceScope;
        /// <inheritdoc/>
        public BlazorAuthorizeHandler(SysCacheService sysCacheService, IServiceScopeFactory serviceScopeFactory)
        {
            _sysCacheService = sysCacheService;
            _serviceScope = serviceScopeFactory.CreateScope();
        }

        /// <inheritdoc/>
        public override async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var isAuthenticated = context.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                if (CheckVerificatFromCache(context))
                {
                    await AuthorizeHandleAsync(context);
                }
                else
                {
                    Fail(context);
                    await App.HttpContext?.SignOutAsync();
                }
            }
            else
            {
                context.GetCurrentHttpContext()?.SignoutToSwagger();
            }

            static void Fail(AuthorizationHandlerContext context)
            {
                context.Fail(); // 授权失败
                DefaultHttpContext currentHttpContext = context.GetCurrentHttpContext();
                if (currentHttpContext == null)
                    return;
                currentHttpContext.SignoutToSwagger();
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
        {

            //这里鉴别密码是否改变
            var userId = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.UserId).Value.ToLong();
            var isOpenApi = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.IsOpenApi)?.Value?.ToBoolean(false) == true;
            if (isOpenApi)
            {

                var _openApiUserService = _serviceScope.ServiceProvider.GetService<OpenApiUserService>();
                var user = await _openApiUserService.GetUsertById(userId);
                if (user == null) { return false; }
                // 此处已经自动验证 Jwt Verificat的有效性了，无需手动验证
                // 路由名称
                var routeName = httpContext.Request.Path.Value;
                var isRolePermission = httpContext.GetMetadata<OpenApiPermissionAttribute>();

                if (isRolePermission == null || user.PermissionCodeList?.Contains(routeName) == true)//如果当前路由信息不包含在角色授权路由列表中则认证失败
                    return true;
                else

                    return false;
            }
            else
            {
                var _sysUserService = _serviceScope.ServiceProvider.GetService<SysUserService>();
                var user = await _sysUserService.GetUsertById(userId);
                if (user == null) { return false; }

                //超级管理员都能访问
                if (UserManager.SuperAdmin) return true;
                if (context.Resource is RouteData routeData)
                {
                    // 获取超级管理员特性
                    var isSpuerAdmin = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
                       x.AttributeType == typeof(SuperAdminAttribute));
                    if (isSpuerAdmin != null)//如果是超级管理员才能访问的接口
                    {
                        //获取忽略超级管理员特性
                        var ignoreSpuerAdmin = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
                    x.AttributeType == typeof(IgnoreSuperAdminAttribute));
                        if (ignoreSpuerAdmin == null && !UserManager.SuperAdmin)//如果只能超级管理员访问并且用户不是超级管理员
                            return false;//直接没权限
                    }
                    //获取角色授权特性
                    var isRolePermission = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
               x.AttributeType == typeof(RolePermissionAttribute));
                    if (isRolePermission != null)
                    {
                        //获取忽略角色授权特性
                        var ignoreRolePermission = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
                   x.AttributeType == typeof(IgnoreRolePermissionAttribute));
                        if (ignoreRolePermission == null)
                        {
                            // 路由名称
                            var routeName = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
        x.AttributeType == typeof(RouteAttribute)).ConstructorArguments[0].Value as string;
                            if (routeName == null) return true;
                            if (user != null)
                            {
                                if (!user.PermissionCodeList.Contains(routeName))//如果当前路由信息不包含在角色授权路由列表中则认证失败
                                    return false;
                            }
                            else
                            {
                                return false;//没有用户信息则返回认证失败
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }

        }

        /// <summary>
        /// 检查token有效性
        /// </summary>
        /// <param name="context">DefaultHttpContext</param>
        /// <returns></returns>
        private bool CheckVerificatFromCache(AuthorizationHandlerContext context)
        {
            var userId = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.UserId).Value;
            var verificatId = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.VerificatId)?.Value;
            var isOpenApi = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.IsOpenApi)?.Value?.ToBoolean(false) == true;
            if (isOpenApi)
            {
                var openapiverificat = _sysCacheService.GetOpenApiVerificatId(userId.ToLong());
                if (openapiverificat == null)
                {
                    return false;
                }
                if (openapiverificat.Any(it => it.Id.ToString() == verificatId))
                {
                    return true;
                }
            }

            else
            {
                var verificat = _sysCacheService.GetVerificatId(userId.ToLong());
                if (verificat == null)
                {
                    return false;
                }
                if (verificat.Any(it => it.Id.ToString() == verificatId))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}
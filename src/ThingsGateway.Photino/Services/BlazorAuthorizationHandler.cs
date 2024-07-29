//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using System.Security.Claims;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Server;

/// <inheritdoc/>
public class BlazorAuthorizationHandler : IAuthorizationHandler
{
    private readonly ISysDictService _sysDictService;
    private readonly ISysRoleService _sysRoleService;
    private readonly ISysUserService _sysUserService;
    private readonly IVerificatInfoService _verificatInfoService;
    private readonly NetCoreAppService _appService;

    public BlazorAuthorizationHandler(IVerificatInfoService verificatInfoService,
        NetCoreAppService netCoreAppService,
        ISysUserService sysUserService, ISysRoleService sysRoleService, ISysDictService sysDictService)
    {
        _sysUserService = sysUserService;
        _sysRoleService = sysRoleService;
        _sysDictService = sysDictService;
        _verificatInfoService = verificatInfoService;
        _appService = netCoreAppService;
    }

    /// <inheritdoc/>
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var isAuthenticated = context.User.Identity?.IsAuthenticated;
        if (isAuthenticated == true)
        {
            _appService.User = context.User;
            if (await CheckVerificatFromCacheAsync(context))
            {
                // 获取所有未成功验证的需求
                var pendingRequirements = context.PendingRequirements;

                // 调用子类管道
                var pipeline = await PipelineAsync(context);
                if (pipeline)
                {
                    // 通过授权验证
                    foreach (var requirement in pendingRequirements)
                    {
                        context.Succeed(requirement);
                    }
                }
                else context.Fail();
            }
            else
            {
                _appService.User = new ClaimsPrincipal(new ClaimsIdentity());
                context.Fail(); // 授权失败
            }
        }
        else
        {
            context.Fail(); // 授权失败
        }


    }

    /// <inheritdoc/>
    public async Task<bool> PipelineAsync(AuthorizationHandlerContext context)
    {
        var userId = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.UserId)?.Value?.ToLong(0) ?? 0;

        var user = await _sysUserService.GetUserByIdAsync(userId);
        if (context.Resource is Microsoft.AspNetCore.Components.RouteData routeData)
        {
            var roles = await _sysRoleService.GetRoleListByUserIdAsync(userId);
            if (roles.All(a => a.Category != RoleCategoryEnum.Global))
                return false;
            //这里鉴别用户使能状态
            if (user == null || !user.Status)
            { return false; }

            //超级管理员都能访问
            var isSuperAdmin = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.SuperAdmin)?.Value.ToBoolean();
            if (isSuperAdmin == true) return true;

            // 获取超级管理员特性
            var superAdminAttr = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
               x.AttributeType == typeof(SuperAdminAttribute));

            if (superAdminAttr != null) //如果是超级管理员才能访问的接口
            {
                return false; //直接没权限
            }
            //获取角色授权特性
            var isRolePermission = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
               x.AttributeType == typeof(RolePermissionAttribute));
            if (isRolePermission != null)
            {
                //获取忽略角色授权特性
                var isIgnoreRolePermission = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
       x.AttributeType == typeof(IgnoreRolePermissionAttribute));
                if (isIgnoreRolePermission == null)
                {
                    // 路由名称
                    var routeName = routeData.PageType.CustomAttributes.FirstOrDefault(x =>
                        x.AttributeType == typeof(RouteAttribute))?.ConstructorArguments?[0].Value as string;
                    if (routeName == null) return true;

                    if (!user.PermissionCodeList.Contains(routeName))//如果当前路由信息不包含在角色授权路由列表中则认证失败
                        return false;
                    else
                        return true;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        else
        {
            //这里鉴别用户使能状态
            if (user == null || !user.Status) { return false; }

            //超级管理员都能访问
            var isSuperAdmin = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.SuperAdmin)?.Value?.ToBoolean();
            if (isSuperAdmin == true) return true;

                //非API请求
                return true;

        }

    }

    /// <summary>
    /// 检查 BearerToken/Cookie 有效性
    /// </summary>
    /// <param name="context">DefaultHttpContext</param>
    /// <returns></returns>
    private async Task<bool> CheckVerificatFromCacheAsync(AuthorizationHandlerContext context)
    {
        var userId = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.UserId)?.Value?.ToLong();
        var verificatId = context.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.VerificatId)?.Value?.ToLong();
        var expire = (await _sysDictService.GetAppConfigAsync()).LoginPolicy.VerificatExpireTime;
        {
            var verificatInfo = userId != null ? _verificatInfoService.GetOne(verificatId ?? 0) : null;//获取token信息

            if (verificatInfo != null)
            {
                if (verificatInfo.VerificatTimeout < DateTime.Now.AddMinutes(5))
                {
                    verificatInfo.VerificatTimeout = DateTime.Now.AddMinutes(30); //新的过期时间
                    _verificatInfoService.Update(verificatInfo); //更新tokne信息到cache
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }


}

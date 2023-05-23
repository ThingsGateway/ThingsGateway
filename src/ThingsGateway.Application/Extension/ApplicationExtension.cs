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

using Microsoft.AspNetCore.Mvc;

using System.Globalization;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ApplicationExtension
    {
        private static ApplicationCacheService ApplicationCacheService;

        static ApplicationExtension()
        {
            ApplicationCacheService = App.GetService<ApplicationCacheService>();
        }

        /// <summary>
        /// 获取全部Api授权树
        /// </summary>
        /// <returns></returns>
        public static List<OpenApiPermissionTreeSelector> OpenApiPermissionTreeSelector()
        {
            var cacheKey = $"{nameof(OpenApiPermissionTreeSelector)}-{CultureInfo.CurrentUICulture.Name}";
            List<OpenApiPermissionTreeSelector> displayName = ApplicationCacheService.GetOrAdd("", cacheKey, entry =>
            {
                List<OpenApiPermissionTreeSelector> openApiGroups = new();
                var controllerTypes = App.EffectiveTypes
                .Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(OpenApiPermissionAttribute), false));
                foreach (var controller in controllerTypes)
                {
                    var GroupName = controller.GetCustomAttribute<ApiDescriptionSettingsAttribute>().GroupName;
                    if (GroupName == CateGoryConst.ThingsGatewayOpenApi)
                    {
                        var Description = controller.GetCustomAttribute<DescriptionAttribute>().Description;
                        var parid = YitIdHelper.NextId();
                        OpenApiPermissionTreeSelector openApiGroup = new() { ApiName = Description, Id = parid, PermissionName = Description };
                        var routeName = "/" + controller.GetCustomAttribute<Microsoft.AspNetCore.Mvc.RouteAttribute>().Template;
                        //获取所有方法
                        var menthods = controller.GetMethods();
                        //遍历方法
                        foreach (var menthod in menthods)
                        {
                            //获取忽略数据权限特性
                            var ignoreOpenApiPermission = menthod.GetCustomAttribute<IgnoreOpenApiPermissionAttribute>();
                            if (ignoreOpenApiPermission == null)//如果是空的代表需要数据权限
                            {
                                //获取接口描述
                                var description = menthod.GetCustomAttribute<DescriptionAttribute>();
                                if (description != null)
                                {
                                    //默认路由名称
                                    var apiRoute = menthod.Name.FirstCharToLower();
                                    //获取get特性
                                    var requestGet = menthod.GetCustomAttribute<HttpGetAttribute>();
                                    if (requestGet != null)//如果是get方法
                                        apiRoute = requestGet.Template;
                                    else
                                    {
                                        //获取post特性
                                        var requestPost = menthod.GetCustomAttribute<HttpPostAttribute>();
                                        if (requestPost != null)//如果是post方法
                                            apiRoute = requestPost.Template;
                                    }
                                    apiRoute = routeName + $"/{apiRoute}";
                                    var apiName = description.Description;//如果描述不为空则接口名称用描述的名称
                                                                          //合并
                                    var permissionName = apiRoute + $"[{apiName}]";
                                    //添加到权限列表
                                    openApiGroup.Children.Add(new OpenApiPermissionTreeSelector
                                    {
                                        Id = YitIdHelper.NextId(),
                                        ParentId = parid,
                                        ApiName = apiName,
                                        ApiRoute = apiRoute,
                                        PermissionName = permissionName
                                    });
                                }
                            }
                        }

                        openApiGroups.Add(openApiGroup);
                    }
                }
                return openApiGroups;
            });
            return displayName;
        }

        /// <summary>
        /// 获取全部页面权限内容
        /// </summary>
        /// <param name="routers"></param>
        /// <returns></returns>
        public static List<PermissionTreeSelector> PermissionTreeSelector(this List<string> routers)
        {
            var cacheKey = $"{nameof(PermissionTreeSelector)}-{CultureInfo.CurrentUICulture.Name}-{routers.ToJson()}";
            List<PermissionTreeSelector> displayName = ApplicationCacheService.GetOrAdd("", cacheKey, entry =>
            {
                List<PermissionTreeSelector> permissions = new List<PermissionTreeSelector>();//权限列表

                // 获取所有需要数据权限的控制器
                var controllerTypes = App.EffectiveTypes.
                    Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
                    && u.IsDefined(typeof(AuthorizeAttribute), false)
                    && u.IsDefined(typeof(RolePermissionAttribute), false)
                    && u.IsDefined(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false));
                foreach (var controller in controllerTypes)
                {
                    //获取数据权限特性
                    var routeName = controller.GetCustomAttribute<Microsoft.AspNetCore.Components.RouteAttribute>()?.Template;
                    if (routeName == null)
                        continue;
                    if (routers.Contains(routeName))
                    {
                        var apiRoute = $"{routeName}";
                        permissions.Add(new() { ApiRoute = apiRoute });
                    }
                }
                return permissions;
            }
            );
            return displayName;
        }
    }
}
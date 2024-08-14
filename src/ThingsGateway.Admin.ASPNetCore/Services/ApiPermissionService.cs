// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://kimdiego2098.github.io/
// QQ群：605534569
// ------------------------------------------------------------------------------


using BootstrapBlazor.Components;

using Mapster;

using System.Globalization;
using System.Reflection;

using ThingsGateway;
using ThingsGateway.Admin.Application;
using ThingsGateway.Core.Extension;
using ThingsGateway.NewLife.X;

public class ApiPermissionService : IApiPermissionService
{
    /// <inheritdoc />
    public List<OpenApiPermissionTreeSelector> ApiPermissionTreeSelector()
    {
        var cacheKey = $"{nameof(ApiPermissionTreeSelector)}-{CultureInfo.CurrentUICulture.Name}";
        var permissions = NetCoreApp.CacheService.GetOrCreate(cacheKey, entry =>
        {
            List<OpenApiPermissionTreeSelector> permissions = new();//权限列表

            // 获取所有需要数据权限的控制器
            var controllerTypes =
                NetCoreApp.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(RolePermissionAttribute), false));
            foreach (var controller in controllerTypes)
            {
                //获取数据权限特性
                var route = controller.GetCustomAttributes<Microsoft.AspNetCore.Mvc.RouteAttribute>().FirstOrDefault();
                if (route == null) continue;

                var description = controller.GetTypeDisplayName();
                var routeName = ResourceUtil.GetRouteName(controller.Name, route.Template);//赋值路由名称
                OpenApiPermissionTreeSelector openApiGroup = new() { ApiName = description ?? routeName, ApiRoute = routeName };
                //获取所有方法
                var menthods = controller.GetRuntimeMethods();
                //遍历方法
                foreach (var menthod in menthods)
                {
                    //获取忽略数据权限特性
                    var ignoreRolePermission = menthod.GetCustomAttribute<IgnoreRolePermissionAttribute>();
                    if (ignoreRolePermission == null)//如果是空的代表需要数据权限
                    {
                        //获取接口描述
                        var methodDesc = controller.GetMethodDisplayName(menthod.Name);
                        //if (methodDesc != null)
                        {
                            //默认路由名称
                            var apiRoute = menthod.Name.ToLowerCamelCase();
                            //获取get特性
                            var requestGet = menthod.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpGetAttribute>();
                            if (requestGet != null)//如果是get方法
                                apiRoute = requestGet.Template;
                            else
                            {
                                //获取post特性
                                var requestPost = menthod.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPostAttribute>();
                                if (requestPost != null)//如果是post方法
                                    apiRoute = requestPost.Template;
                                else
                                    continue;
                            }

                            //apiRoute = route.Template + $"/{apiRoute}";
                            apiRoute = routeName + $"/{apiRoute}";

                            //添加到权限列表
                            openApiGroup.Children ??= new();
                            openApiGroup.Children.Add(new OpenApiPermissionTreeSelector
                            {
                                ApiName = methodDesc,
                                ApiRoute = apiRoute,
                            });
                        }
                    }
                }

                permissions.Add(openApiGroup);
            }
            return permissions;
        });
        return permissions;
    }


}

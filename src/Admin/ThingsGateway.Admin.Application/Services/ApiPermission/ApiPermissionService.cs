// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------


using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Globalization;
using System.Reflection;

using ThingsGateway.Extension;
using ThingsGateway.SpecificationDocument;

namespace ThingsGateway.Admin.Application;

internal sealed class ApiPermissionService : IApiPermissionService
{
    private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionGroupCollectionProvider;
    private readonly SwaggerGeneratorOptions _generatorOptions;

    public ApiPermissionService(
        IOptions<SwaggerGeneratorOptions> generatorOptions,
        IApiDescriptionGroupCollectionProvider apiDescriptionGroupCollectionProvider)
    {
        _generatorOptions = generatorOptions.Value;
        _apiDescriptionGroupCollectionProvider = apiDescriptionGroupCollectionProvider;
    }
    private IEnumerable<string> GetDocumentNames()
    {
        return _generatorOptions.SwaggerDocs.Keys;
    }

    /// <inheritdoc />
    public List<OpenApiPermissionTreeSelector> ApiPermissionTreeSelector()
    {
        var cacheKey = $"{nameof(ApiPermissionTreeSelector)}-{CultureInfo.CurrentUICulture.Name}";
        var permissions = App.CacheService.Get<List<OpenApiPermissionTreeSelector>>(cacheKey);
        if (permissions == null)
        {
            permissions = new();

            Dictionary<string, OpenApiPermissionTreeSelector> groupOpenApis = new();
            foreach (var item in GetDocumentNames())
            {
                OpenApiPermissionTreeSelector openApiPermissionTreeSelector = new() { ApiName = item ?? "Default" };
                groupOpenApis.TryAdd(openApiPermissionTreeSelector.ApiName, openApiPermissionTreeSelector);
            }
            var apiDescriptions = _apiDescriptionGroupCollectionProvider.ApiDescriptionGroups.Items;

            // 获取所有需要数据权限的控制器
            var controllerTypes =
                App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(RolePermissionAttribute), false));

            foreach (var groupOpenApi in groupOpenApis)
            {

                foreach (var apiDescriptionGroup in apiDescriptions)
                {


                    var routes = apiDescriptionGroup.Items.Where(api => api.ActionDescriptor is ControllerActionDescriptor);

                    OpenApiPermissionTreeSelector openApiPermissionTreeSelector = groupOpenApi.Value;

                    Dictionary<string, OpenApiPermissionTreeSelector> openApiPermissionTreeSelectorDict = new();

                    foreach (var route in routes)
                    {
                        if (!SpecificationDocumentBuilder.CheckApiDescriptionInCurrentGroup(groupOpenApi.Key, route))
                        {
                            continue;
                        }

                        var actionDesc = (ControllerActionDescriptor)route.ActionDescriptor;
                        if (!actionDesc.ControllerTypeInfo.CustomAttributes.Any(a => a.AttributeType == typeof(RolePermissionAttribute)))
                            continue;
                        var controllerDescription = actionDesc.ControllerTypeInfo.GetTypeDisplayName();

                        if (openApiPermissionTreeSelectorDict.TryGetValue(actionDesc.ControllerName, out var openApiControllerGroup))
                        {

                        }
                        else
                        {
                            openApiControllerGroup = new() { ApiName = controllerDescription, ApiRoute = actionDesc.ControllerName };

                            openApiPermissionTreeSelectorDict.Add(actionDesc.ControllerName, openApiControllerGroup);
                        }

                        var ignoreRolePermission = actionDesc.MethodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(IgnoreRolePermissionAttribute));
                        if (ignoreRolePermission)
                            continue;
                        var routePath = route.RelativePath;
                        var methodDesc = actionDesc.MethodInfo.DeclaringType.GetMethodDisplayName(actionDesc.MethodInfo.Name);

                        //添加到权限列表
                        openApiControllerGroup.Children ??= new();
                        openApiControllerGroup.Children.Add(new OpenApiPermissionTreeSelector
                        {
                            ApiName = methodDesc,
                            ApiRoute = routePath,
                        });
                    }


                    openApiPermissionTreeSelector.Children.AddRange(openApiPermissionTreeSelectorDict.Values);

                    if (openApiPermissionTreeSelector.Children.Any(a => a.Children.Count > 0))
                        permissions.Add(openApiPermissionTreeSelector);

                }

            }

            App.CacheService.Set(cacheKey, permissions);
        }
        return permissions;
    }


    /// <summary>
    /// 获取路由地址名称
    /// </summary>
    /// <param name="controllerName">控制器地址</param>
    /// <param name="template">路由名称</param>
    /// <returns></returns>
    public string GetRouteName(string controllerName, string template)
    {
        if (!template.StartsWith('/'))
            template = "/" + template;//如果路由名称不是/开头则加上/防止控制器没写
        if (template.Contains("[controller]"))
        {
            controllerName = controllerName.Replace("Controller", "");//去掉Controller
            controllerName = controllerName.ToLowerCamelCase();//转首字母小写写
            template = template.Replace("[controller]", controllerName);//替换[controller]
        }

        return template;
    }

    /// <inheritdoc />
    public IEnumerable<PermissionTreeSelector> PermissionTreeSelector(IEnumerable<string> routes)
    {
        List<PermissionTreeSelector> permissions = PermissionTreeSelector();
        return permissions.Where(a => routes.ToHashSet().Contains(a.ApiRoute));
    }

    /// <inheritdoc />
    public List<PermissionTreeSelector> PermissionTreeSelector()
    {
        var cacheKey = $"{nameof(PermissionTreeSelector)}-{CultureInfo.CurrentUICulture.Name}";
        var permissions = App.CacheService.GetOrAdd(cacheKey, entry =>
        {
            List<PermissionTreeSelector> permissions = new();//权限列表

            // 获取所有需要数据权限的控制器
            var controllerTypes = App.EffectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
        && u.IsDefined(typeof(AuthorizeAttribute), false)
        && u.IsDefined(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false));

            foreach (var controller in controllerTypes)
            {
                //获取数据权限特性
                var route = controller.GetCustomAttributes<Microsoft.AspNetCore.Components.RouteAttribute>().FirstOrDefault();
                if (route == null) continue;
                var apiRoute = GetRouteName(controller.Name, route.Template);//赋值路由名称

                var desc = controller.GetTypeDisplayName();
                //添加到权限列表
                permissions.Add(new PermissionTreeSelector
                {
                    ApiName = desc,
                    ApiRoute = apiRoute,
                });
            }
            return permissions;
        });

        return permissions;
    }
}

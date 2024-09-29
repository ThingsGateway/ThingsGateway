﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using ThingsGateway.Razor;

namespace ThingsGateway.Debug;

public partial class MainLayout
{
    private List<Assembly> _assemblyList = new();

    private string _versionString = string.Empty;

    [Inject]
    [NotNull]
    private DialogService? DialogService { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<MainLayout>? Localizer { get; set; }

    [Inject]
    [NotNull]
    private IMenuService? MenuService { get; set; }

    [Inject]
    [NotNull]
    private IAppVersionService? VersionService { get; set; }

    [Inject]
    [NotNull]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }

    protected override void OnInitialized()
    {
        _assemblyList = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
 a.GetTypes()).Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass
&& u.IsDefined(typeof(Microsoft.AspNetCore.Components.RouteAttribute), true)).Select(a => a.Assembly)
//.Where(a => a != typeof(BlazorApp).Assembly)
.Distinct().ToList();
        base.OnInitialized();
    }

    protected override Task OnInitializedAsync()
    {
        _versionString = $"v{VersionService.Version}";
        return base.OnInitializedAsync();
    }

    private async Task ShowAbout()
    {
        DialogOption? op = null;

        op = new DialogOption()
        {
            IsScrolling = false,
            Size = Size.Medium,
            ShowFooter = false,
            Title = Localizer["About"],
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<About>().Render(),
        };
        await DialogService.Show(op);
    }
}


//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using NewLife;

namespace ThingsGateway.Razor;

public partial class CultureChooser
{
    [Inject]
    [NotNull]
    private IOptionsMonitor<BootstrapBlazorOptions>? BootstrapOptions { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    private string SelectedCulture { get; set; } = CultureInfo.CurrentUICulture.Name;
    private bool _firstRender;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _firstRender = true;
            if (OperatingSystem.IsBrowser() || !Runtime.IsWeb)
            {
                var cultureName = await JSRuntime.GetCulture();
                if (SelectedCulture != cultureName)
                {
                    SelectedCulture = cultureName ?? "zh-CN";
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    SelectedCulture = cultureName ?? "zh-CN";
                }
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task SetCulture(SelectedItem item)
    {
        if (_firstRender)
        {
            if (OperatingSystem.IsBrowser() || !Runtime.IsWeb)
            {
                var cultureName = item.Value;
                if (cultureName != CultureInfo.CurrentCulture.Name)
                {
                    await JSRuntime.SetCulture(cultureName);
                    var culture = new CultureInfo(cultureName);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;

                    NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
                }
            }
            else
            {
                // 使用 api 方式 适用于 Server-Side 模式
                if (SelectedCulture != item.Value)
                {
                    var culture = item.Value;
                    var uri = new Uri(NavigationManager.Uri).GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
                    var query = $"?culture={Uri.EscapeDataString(culture)}&redirectUri={Uri.EscapeDataString(uri)}";

                    // use a path that matches your culture redirect controller from the previous steps
                    NavigationManager.NavigateTo("/Culture/SetCulture" + query, forceLoad: true);
                }
            }
        }
    }

    private static string GetDisplayName(CultureInfo culture)
    {
        string? ret;
        if (OperatingSystem.IsBrowser())
        {
            ret = culture.Name switch
            {
                "zh-CN" => "中文（中国）",
                "en-US" => "English (United States)",
                _ => ""
            };
        }
        else
        {
            ret = culture.DisplayName;
        }
        return ret;
    }
}
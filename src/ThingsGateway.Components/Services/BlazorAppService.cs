#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using BlazorComponent;
using BlazorComponent.I18n;

using Masa.Blazor;

using Microsoft.AspNetCore.Components.Web;

using System.Globalization;

using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Extension.Json;

namespace ThingsGateway.Components;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

public class BlazorAppService : IAsyncDisposable
{
    public const string DefaultProjectName = "ThingsGateway.Components";

    /// <summary>
    /// 资源默认路径
    /// </summary>
    public const string DefaultResourceUrl = "/_content/ThingsGateway.Components/";

    /// <summary>
    /// {/_content/{0}/} 填入工程名称
    /// </summary>
    public const string Format = @"/_content/{0}/";

    /// <summary>
    /// 表格操作列标识
    /// </summary>
    public const string DataTableActions = "DataTableActions";

    /// <summary>
    /// AppBarHeight
    /// </summary>
    public const int AppBarHeight = 48;

    /// <summary>
    /// Tab高度
    /// </summary>
    public const int PageTabsHeight = 36;

    public int LogoHeight => IsPageTab ? AppBarHeight + PageTabsHeight : AppBarHeight;

    /// <summary>
    /// FooterHeight
    /// </summary>
    public const int FooterHeight = 36;

    /// <summary>
    /// 计算顶部，PageTab，页脚的高度和+12
    /// </summary>
    public const int DefaultHeight = AppBarHeight + PageTabsHeight + FooterHeight + 12;

    public BlazorAppService(MasaBlazor masaBlazor, IJSRuntime jsRuntime, LocalStorage localStorage, I18n i18n)
    {
        MasaBlazor = masaBlazor;
        JSRuntime = jsRuntime;
        LocalStorage = localStorage;
        I18n = i18n;
    }

    public bool InitSuccess { get; private set; }

    public async Task<bool> InitAll()
    {
        if (!InitSuccess)
        {
            await InitTimezoneOffsetAsync();
            await InitI18nAsync();
            await InitThemeAsync();
            await InitRTLAsync();
            await InitTabAsync();
            InitSuccess = true;
            return true;
        }
        return false;
    }

    public I18n I18n { get; set; }
    public MasaBlazor MasaBlazor { get; set; }
    public IJSRuntime JSRuntime { get; set; }
    public LocalStorage LocalStorage { get; set; }

    #region 推送导出txt

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public async Task DownLogTxtAsync(IEnumerable<string> values)
    {
        using var memoryStream = new MemoryStream();
        using StreamWriter writer = new(memoryStream);
        foreach (var item in values)
        {
            writer.WriteLine(item);
        }
        writer.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var streamRef = new DotNetStreamReference(stream: memoryStream);
        DownloadStreamJS ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
        await DownloadStreamJS.InvokeVoidAsync("downloadFileFromStream", $"{I18n.T("Log Export")}{DateTime.Now.ToFileDateTimeFormat(TimezoneOffset)}.txt", streamRef);
    }

    /// <summary>
    /// 导出
    /// </summary>
    public async Task DownXlsxAsync(MemoryStream memoryStream, string fileName)
    {
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var streamRef = new DotNetStreamReference(stream: memoryStream);
        DownloadStreamJS ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
        await DownloadStreamJS.InvokeVoidAsync("downloadFileFromStream", $"{fileName}{DateTime.Now.ToFileDateTimeFormat(TimezoneOffset)}.xlsx", streamRef);
        memoryStream.Dispose();
    }

    private IJSObjectReference DownloadStreamJS;

    #endregion 推送导出txt

    #region 时差

    private const string _timezoneOffsetKey = "timezoneOffset";
    private TimeSpan _timezoneOffset = TimeSpan.FromHours(8);
    public IJSObjectReference CommonJS { get; set; }

    /// <summary>
    /// 当前的客户端时差
    /// </summary>
    public TimeSpan TimezoneOffset
    {
        get => _timezoneOffset;
        private set
        {
            _timezoneOffset = value;
        }
    }

    /// <summary>
    /// 获取Web客户端时差
    /// </summary>
    /// <returns></returns>
    public async Task InitTimezoneOffsetAsync()
    {
        var timezoneOffsetResult = await LocalStorage.GetItemAsync<double?>(_timezoneOffsetKey);
        if (timezoneOffsetResult != null)
        {
            TimezoneOffset = TimeSpan.FromMinutes(timezoneOffsetResult.Value);
            await LocalStorage.SetItemAsync(_timezoneOffsetKey, TimezoneOffset.TotalMinutes.ToString());
            return;
        }
        CommonJS ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorAppService.DefaultResourceUrl}js/common.js");
        var offset = await CommonJS.InvokeAsync<double>("getTimezoneOffset");
        TimezoneOffset = TimeSpan.FromMinutes(-offset);
        await LocalStorage.SetItemAsync(_timezoneOffsetKey, TimezoneOffset.TotalMinutes.ToJsonString());
    }

    #endregion 时差

    #region 夜间/白天，主题颜色

    public bool Dark => MasaBlazor.Theme.Dark;
    public ThemeOptions ThemeOptions => Dark ? MasaBlazor.Theme.Themes.Dark : MasaBlazor.Theme.Themes.Light;
    private ThemeCssBuilder ThemeCssBuilder = new();

    /// <summary>
    /// 更改主颜色
    /// </summary>
    public async Task ThemeChange(string primary)
    {
        ThemeOptions.Primary = primary;
        var style = ThemeCssBuilder.Build(ThemeOptions, Dark);
        await JSRuntime.InvokeVoidAsync(JsInteropConstants.UpsertThemeStyle, "masa-blazor-theme-stylesheet", style);
        await LocalStorage.SetItemAsync($"masablazor@theme@primary@{Dark}", primary);
    }

    /// <summary>
    /// 切换主题模式, 传入按键参数,objRef,以及是否夜间主题
    /// </summary>
    public async ValueTask SwitchTheme(object objRef, string option, MouseEventArgs e)
    {
        CommonJS ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorAppService.DefaultResourceUrl}js/common.js");

        await CommonJS.InvokeVoidAsync("switchTheme", objRef, option, e.ClientX, e.ClientY);
    }

    public string Theme
    {
        get;
        set;
    }

    public async Task InitThemeAsync()
    {
        CommonJS ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorAppService.DefaultResourceUrl}js/common.js");
        var option = await GetThemeFromLocalStorage() ?? "System";
        await ToggleTheme(option);
        var primary = await LocalStorage.GetItemAsync($"masablazor@theme@primary@{Dark}") ?? ThemeOptions.Primary;
        await ThemeChange(primary);
    }

    private ValueTask<bool> IsDarkPreferColor() => CommonJS.InvokeAsync<bool>("isDarkPreferColor");

    private Task<string?> GetThemeFromLocalStorage() => LocalStorage.GetItemAsync("masablazor@theme");

    private Task UpdateThemeInLocalStorage(string value) => LocalStorage.SetItemAsync("masablazor@theme", value);

    private async Task ToggleThemeInternal(bool isDark, bool isSystem = false)
    {
        if (isDark != Dark)
        {
            MasaBlazor.ToggleTheme();
        }
        var data = isSystem ? "System" : isDark ? "Dark" : "Light";
        await UpdateThemeInLocalStorage(data);
        Theme = data;
    }

    //更换外观主题
    public async Task ToggleTheme(string option)
    {
        var isSetDark = await GetTheme(option);
        if (isSetDark != null)
        {
            await ToggleThemeInternal(isSetDark.Value, option == "System");
        }
        else if (option == "System")
        {
            await ToggleThemeInternal(false, option == "System");
        }
        else
        {
            Theme = option;
        }
    }

    //更换外观主题
    public async Task ToggleTheme(bool isSetDark, bool isSystem = false)
    {
        await ToggleThemeInternal(isSetDark, isSystem);
    }

    private async Task<bool?> GetTheme(string option)
    {
        CommonJS ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorAppService.DefaultResourceUrl}js/common.js");
        bool? isSetDark = null;
        switch (option)
        {
            case "System":
                try
                {
                    var isDark = await IsDarkPreferColor();
                    if (isDark != Dark)
                    {
                        isSetDark = isDark;
                    }
                }
                catch (JSException)
                {
                    // ignored
                }
                break;

            case "Light" when Dark:
                isSetDark = false;
                break;

            case "Dark" when !Dark:
                isSetDark = true;
                break;
        }

        return isSetDark;
    }

    #endregion 夜间/白天，主题颜色

    #region RTL

    public void ToggleRTL(bool rtl)
    {
        if (rtl == MasaBlazor.RTL)
        {
            return;
        }

        MasaBlazor.RTL = rtl;

        _ = LocalStorage.SetItemAsync("masablazor@rtl", rtl ? "rtl" : "ltr");
    }

    public async Task InitRTLAsync()
    {
        var rtlStr = await LocalStorage.GetItemAsync("masablazor@rtl");
        MasaBlazor.RTL = rtlStr == "rtl";
    }

    #endregion RTL

    #region 语言

    public CultureInfo Culture { get; set; }

    public async Task InitI18nAsync()
    {
        var langStr = await LocalStorage.GetItemAsync("masablazor@lang");
        if (langStr is not null)
        {
            Culture = new CultureInfo(langStr);
            I18n.SetCulture(CultureInfo.CurrentCulture, Culture);
        }
    }

    public async Task OnCultureChanged(string cultureName)
    {
        Culture = new CultureInfo(cultureName);
        I18n.SetCulture(CultureInfo.CurrentCulture, Culture);
        await LocalStorage.SetItemAsync("masablazor@lang", cultureName);
    }

    #endregion 语言

    #region Mobile

    public bool IsMobile => MasaBlazor.Breakpoint.Mobile;

    #endregion Mobile

    #region 标签页/面包屑

    public bool IsPageTab { get; private set; } = true;

    public async Task ToggleTabAsync(bool isPageTab)
    {
        if (isPageTab == IsPageTab)
        {
            return;
        }

        IsPageTab = isPageTab;

        await LocalStorage.SetItemAsync("masablazor@isPageTab", isPageTab ? "true" : "false");
    }

    public async Task InitTabAsync()
    {
        var isPageTab = await LocalStorage.GetItemAsync<bool>("masablazor@isPageTab");
        IsPageTab = isPageTab;
    }

    #endregion 标签页/面包屑

    #region Ajax

    private IJSObjectReference AjaxJS;

    public async Task GotoAsync(string url)
    {
        AjaxJS ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorAppService.DefaultResourceUrl}js/blazor_ajax.js");
        await AjaxJS.InvokeVoidAsync("blazor_ajax_goto", url);
    }

    public async Task DownFileAsync(string url, string fileName, object dtoObject)
    {
        AjaxJS ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorAppService.DefaultResourceUrl}js/blazor_ajax.js");
        await AjaxJS.InvokeVoidAsync("blazor_downloadFile", url, fileName, dtoObject);
    }

    /// <summary>
    /// 请求并返回消息
    /// </summary>
    /// <param name="option">Ajax配置</param>
    /// <returns></returns>
    public async Task<string> GetMessageAsync(AjaxOption option)
    {
        AjaxJS ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorAppService.DefaultResourceUrl}js/blazor_ajax.js");
        return await AjaxJS.InvokeAsync<string>("blazor_ajax", option.Url, option.Method, option.Data);
    }

    public async ValueTask DisposeAsync()
    {
        if (CommonJS != null)
            try { await CommonJS.DisposeAsync(); } catch { }
        if (AjaxJS != null)
            try { await AjaxJS.DisposeAsync(); } catch { }
        if (DownloadStreamJS != null)
            try { await DownloadStreamJS.DisposeAsync(); } catch { }
    }

    #endregion Ajax
}
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

using Masa.Blazor;
using Masa.Blazor.Presets;

using Microsoft.Extensions.DependencyInjection;
namespace ThingsGateway.Components;

/// <summary>
/// AppStartup启动类
/// </summary>
public static class Startup
{
    /// <inheritdoc/>
    public static void ThingsGatewayComponentsConfigureServices(this IServiceCollection services)
    {
        services.AddMasaBlazor(options =>
        {
            options.Defaults = new Dictionary<string, IDictionary<string, object>>()
            {
                {
                    PopupComponents.SNACKBAR, new Dictionary<string, object>()
                    {
                        { nameof(PEnqueuedSnackbars.Closeable), true },
                        { nameof(PEnqueuedSnackbars.Position), SnackPosition.TopCenter }
                    }
                },
        { nameof(MErrorHandler), new Dictionary<string, object>() { { nameof(MErrorHandler.ShowDetail), true } } },

        { nameof(MIcon), new Dictionary<string, object>() { { nameof(MIcon.Dense), true } } },
        { nameof(MAlert), new Dictionary<string, object>() { { nameof(MAlert.Dense), true } } },
        { "MCascaderColumn", new Dictionary<string, object>() { { "Dense", true } } },
        { nameof(MChip), new Dictionary<string, object>() { { nameof(MChip.Small), true } } },
        { "MDataTable", new Dictionary<string, object>() { { "Dense", true } } },
        { nameof(MSimpleTable), new Dictionary<string, object>() { { nameof(MSimpleTable.Dense), true } } },
        { nameof(MDescriptions), new Dictionary<string, object>() { { nameof(MDescriptions.Dense), true } } },
        { nameof(MRow), new Dictionary<string, object>() { { nameof(MRow.Dense), true } } },
        { "MAutocomplete", new Dictionary<string, object>() { { "Dense", true } } },
        { "MCascader", new Dictionary<string, object>() { { "Dense", true } } },
        { "MCheckbox", new Dictionary<string, object>() { { "Dense", true } } },
        { "MFileInput", new Dictionary<string, object>() { { "Dense", true } } },
        { "MRadioGroup", new Dictionary<string, object>() { { "Dense", true } } },
        { "MRangeSlider", new Dictionary<string, object>() { { "Dense", true } } },
        { "MSelect", new Dictionary<string, object>() { { "Dense", true } } },
        { "MSlider", new Dictionary<string, object>() { { "Dense", true } } },
        { "MSwitch", new Dictionary<string, object>() { { "Dense", true } } },
        { "MTextarea", new Dictionary<string, object>() { { "Dense", true } } },
        { "MTextField", new Dictionary<string, object>() { { "Dense", true } } },
        { nameof(MButtonGroup), new Dictionary<string, object>() { { nameof(MButtonGroup.Dense), true } } },
        { nameof(MListItem), new Dictionary<string, object>() { { nameof(MListItem.Dense), true } } },
        { nameof(MRating), new Dictionary<string, object>() { { nameof(MRating.Dense), true } } },
        { nameof(MTimeline), new Dictionary<string, object>() { { nameof(MTimeline.Dense), true } } },
        { nameof(MToolbar), new Dictionary<string, object>() { { nameof(MToolbar.Dense), true } } },
        { "MTreeview", new Dictionary<string, object>() { { "Dense", true } } },
        { nameof(PImageCaptcha), new Dictionary<string, object>() { { nameof(PImageCaptcha.Dense), true } } }



            };
            options.ConfigureTheme(theme =>
            {
                theme.Themes.Dark.Accent = "#FF4081";
                theme.Themes.Dark.Error = "#FF5252";
                theme.Themes.Dark.Info = "#2196F3";
                theme.Themes.Dark.Primary = "#2196F3";
                theme.Themes.Dark.Secondary = "#424242";
                theme.Themes.Dark.Success = "#4CAF50";
                theme.Themes.Dark.Warning = "#FB8C00";
                theme.Themes.Dark.UserDefined.Add("barcolor", "#1e1e1e");

                theme.Themes.Light.Accent = "#82B1FF";
                theme.Themes.Light.Error = "#FF5252";
                theme.Themes.Light.Info = "#2196F3";
                theme.Themes.Light.Primary = "#1976D2";
                theme.Themes.Light.Secondary = "#424242";
                theme.Themes.Light.Success = "#4CAF50";
                theme.Themes.Light.Warning = "#FB8C00";
                theme.Themes.Light.UserDefined.Add("barcolor", "#fff");

            });
            options.Locale = new Locale("zh-CN", "en-US");

        });

        services.AddScoped<InitTimezone>();
        services.AddScoped<AjaxService>();
        services.AddScoped<CookieStorage>();

    }
}
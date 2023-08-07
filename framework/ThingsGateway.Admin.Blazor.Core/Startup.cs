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

using Furion;

using Masa.Blazor;
using Masa.Blazor.Presets;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor.Core;

/// <summary>
/// AppStartup启动类
/// </summary>
public class Startup : AppStartup
{
    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
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
                }
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
        });

        services.AddScoped<InitTimezone>();
        services.AddScoped<AjaxService>();
        services.AddScoped<UserResoures>();

    }
}
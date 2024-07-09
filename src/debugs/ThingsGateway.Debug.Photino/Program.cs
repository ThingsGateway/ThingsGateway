//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Photino.Blazor;

using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using ThingsGateway.Razor;

namespace ThingsGateway.Debug;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        //当前工作目录设为程序集的基目录
        System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        // 增加中文编码支持
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

#if Pro

        MenuConfigs.Default.MenuItems.AddRange(ThingsGateway.Debug.ProRcl.ProMenuConfigs.Default.MenuItems);

#endif

        var builder = PhotinoBlazorAppBuilder.CreateDefault(args);

        // 创建配置生成器并读取appsettings.json
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        // 构建配置
        var configuration = configBuilder.Build();

        // 添加配置服务
        builder.Services.AddSingleton<IConfiguration>(configuration);

        // 增加中文编码支持网页源码显示汉字
        builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

        builder.Services.AddBlazorRcl().AddDebugRcl();

        builder.RootComponents.Add<BlazorApp>("#app");

        var app = builder.Build();

        app.MainWindow.SetUseOsDefaultLocation(false);
        app.MainWindow.SetUseOsDefaultSize(false);
        app.MainWindow.SetSize(new System.Drawing.Size(1600, 900));
        app.MainWindow.SetTitle("ThingsGateway.Debug");
        app.MainWindow.SetIconFile("wwwroot/favicon.ico");
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
        };
        app.Run();
    }
}

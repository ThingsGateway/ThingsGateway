//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Photino.Blazor;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using ThingsGateway.Admin.NetCore;

namespace ThingsGateway.Photino;

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

        builder.Services.ConfigureServicesWithoutWeb();

        // 添加配置服务
        builder.Services.AddSingleton<IConfiguration>(NetCoreApp.Configuration);

        // 增加中文编码支持网页源码显示汉字
        builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

        builder.RootComponents.Add<Routes>("#app");


        var app = builder.Build();

        app.Services.UseServicesWithoutWeb();

        app.MainWindow.ContextMenuEnabled = false;
        app.MainWindow.DevToolsEnabled = true;
        app.MainWindow.GrantBrowserPermissions = true;
        app.MainWindow.SetUseOsDefaultLocation(false);
        app.MainWindow.SetUseOsDefaultSize(false);
        app.MainWindow.SetSize(new System.Drawing.Size(1920, 1080));
        app.MainWindow.SetTitle("ThingsGateway");
        app.MainWindow.SetIconFile("favicon.ico");
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
        };

        app.MainWindow.WindowClosing += (sender, e) =>
        {
            StopHostedService(app.Services);
            return false;
        };
        StartHostedService(app.Services);
        app.Run();
        Thread.Sleep(2000);
    }

    public static void StartHostedService(IServiceProvider serviceProvider)
    {

        var applicationLifetime = serviceProvider.GetRequiredService<ApplicationLifetime>();
        var hostedServiceExecutor = serviceProvider.GetRequiredService<HostedServiceExecutor>();
        // Fire IHostedService.Start
        hostedServiceExecutor.StartAsync(default).ConfigureAwait(false).GetAwaiter().GetResult();

        applicationLifetime.NotifyStarted();

    }

    public static void StopHostedService(IServiceProvider serviceProvider)
    {

        var applicationLifetime = serviceProvider.GetRequiredService<ApplicationLifetime>();
        applicationLifetime.StopApplication();

        var _hostedServiceExecutor = serviceProvider.GetRequiredService<HostedServiceExecutor>();
        _hostedServiceExecutor.StopAsync(default).ConfigureAwait(false).GetAwaiter().GetResult();
        applicationLifetime.NotifyStopped();

    }

}

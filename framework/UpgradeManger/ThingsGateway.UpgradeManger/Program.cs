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

using Microsoft.Extensions.DependencyInjection;

using Photino.Blazor;

namespace ThingsGateway.UpgradeManger;

internal class Program
{
    internal static PhotinoBlazorApp app;
    [STAThread]
    private static void Main(string[] args)
    {
        System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);

        appBuilder.RootComponents.Add<App>("#app");
        appBuilder.Services.ThingsGatewayComponentsConfigureServices();
        appBuilder.Services.AddSingleton<UpgradeManger>();

        app = appBuilder.Build();
        app.MainWindow.SetTitle("ThingsGateway.UpgradeManger");
        app.MainWindow.SetIconFile("wwwroot/favicon.ico");
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
        };
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        var upgradeManger = app.Services.GetService<UpgradeManger>();
        _ = upgradeManger.ExecuteAsync(cancellationTokenSource.Token);
        app.Run();

        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();

        Thread.Sleep(2000);
    }
}
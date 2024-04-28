
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

        var builder = PhotinoBlazorAppBuilder.CreateDefault(args);

        builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

        // 增加中文编码支持网页源码显示汉字
        builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

        builder.RootComponents.Add<BlazorApp>("#app");
        builder.Services.AddBlazorRcl().AddDebugRcl();

        builder.Services.Configure<BootstrapBlazorOptions>(op =>
        {
            op.ToastDelay = 4000;
            op.SupportedCultures = new List<string> { "zh-CN", "en-US" };
            op.MessageDelay = 4000;
            op.SwalDelay = 4000;
            op.EnableErrorLogger = true;
            op.FallbackCulture = "zh-CN";
            op.DefaultCultureInfo = "zh-CN"; //修改默认语言
            op.TableSettings = new TableSettings
            {
                CheckboxColumnWidth = 36
            };
            op.IgnoreLocalizerMissing = true;
            op.StepSettings = new StepSettings
            {
                Short = 1,
                Int = 1,
                Long = 1,
                Float = 0.1f,
                Double = 0.01,
                Decimal = 0.01m
            };
            var culture = new CultureInfo("zh-CN");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        });

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
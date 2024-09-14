//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Windows.Forms;

using ThingsGateway.Admin.NetCore;

namespace ThingsGateway.Winform;

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

        ServiceCollection serviceDescriptors = new();

        serviceDescriptors.AddWindowsFormsBlazorWebView();

        serviceDescriptors.ConfigureServicesWithoutWeb();

        // 添加配置服务
        serviceDescriptors.AddSingleton<IConfiguration>(NetCoreApp.Configuration);

        // 增加中文编码支持网页源码显示汉字
        serviceDescriptors.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));


        var services = serviceDescriptors.BuildServiceProvider();


        services.UseServicesWithoutWeb();


        StartHostedService(services);
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Error");
        };

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm(services));

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


}

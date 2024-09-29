//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Text;
using System.Windows.Forms;

using ThingsGateway.NewLife.Log;

namespace ThingsGateway.Debug;

internal class Program
{
    internal static void Closing(object? sender, FormClosingEventArgs e)
    {
        host.StopAsync();
    }

    [STAThread]
    private static void Main(string[] args)
    {
        //当前工作目录设为程序集的基目录
        System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        // 增加中文编码支持
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        #region 控制台输出Logo

        Console.Write(Environment.NewLine);
        Console.ForegroundColor = ConsoleColor.Yellow;
        XTrace.WriteLine(string.Empty);
        Console.WriteLine(
            """

               _______  _      _                      _____         _
              |__   __|| |    (_)                    / ____|       | |
                 | |   | |__   _  _ __    __ _  ___ | |  __   __ _ | |_  ___ __      __ __ _  _   _
                 | |   | '_ \ | || '_ \  / _` |/ __|| | |_ | / _` || __|/ _ \\ \ /\ / // _` || | | |
                 | |   | | | || || | | || (_| |\__ \| |__| || (_| || |_|  __/ \ V  V /| (_| || |_| |
                 |_|   |_| |_||_||_| |_| \__, ||___/ \_____| \__,_| \__|\___|  \_/\_/  \__,_| \__, |
                                          __/ |                                                __/ |
                                         |___/                                                |___/

            """
         );
        Console.ResetColor();

        #endregion 控制台输出Logo


        var options = GenericRunOptions.DefaultSilence
              .ConfigureServices(services =>
              {
                  services.AddWindowsFormsBlazorWebView();
              });
        ;

        Serve.BuildApplication(options, out var app);
        host = app;
        app.Start();

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Error");
        };

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm(app.Services));

        Thread.Sleep(5000);
    }


    private static IHost host;

}

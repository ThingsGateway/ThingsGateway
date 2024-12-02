//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.ResponseCompression;

using System.Runtime.InteropServices;
using System.Text;

using ThingsGateway.NewLife.Log;

using Console = System.Console;

namespace ThingsGateway.Server;

public class Program
{
    private static readonly string[] second = new[] { "application/octet-stream" };

    public static async Task Main(string[] args)
    {
        await Task.Delay(2000).ConfigureAwait(false);
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


        await Serve.RunAsync(RunOptions.Default.ConfigureBuilder(builder =>
           {

               if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                   builder.Host.UseWindowsService();
               else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                   builder.Host.UseSystemd();

               if (!builder.Environment.IsDevelopment())
               {
                   builder.Services.AddResponseCompression(
                       opts =>
                       {
                           opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(second);
                       });
               }

               builder.WebHost.UseWebRoot("wwwroot");
               builder.WebHost.UseStaticWebAssets();
               // 设置接口超时时间和上传大小-Kestrel
               builder.WebHost.ConfigureKestrel(u =>
               {
                   u.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
                   u.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
                   u.Limits.MaxRequestBodySize = null;
               });




           })
            .Configure(app =>
            {
#if NET8_0_OR_GREATER
                app.MapRazorComponents<BlazorApp>()
                    .AddAdditionalAssemblies(App.RazorAssemblies.Distinct().Where(a => a != typeof(Program).Assembly).ToArray())
                    .AddInteractiveServerRenderMode();
#else

                app.MapBlazorHub();
                app.MapFallbackToPage("/_Host");

#endif

            })
            ).ConfigureAwait(false);



    }

}

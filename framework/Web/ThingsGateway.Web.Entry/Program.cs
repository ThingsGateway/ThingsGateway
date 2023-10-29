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

using Furion.Templates;

using System.Reflection;

namespace ThingsGateway.Web.Entry;

/// <summary>
/// 启动
/// </summary>
public class Program
{
    /// <summary>
    /// 程序运行入口
    /// </summary>
    /// <param name="args"></param>
    public static Task Main(string[] args)
    {
        //当前工作目录设为程序集的基目录
        System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        #region 控制台输出Logo
        Console.Write(Environment.NewLine);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("████████╗    ██╗  ██╗    ██╗    ███╗   ██╗     ██████╗     ███████╗     ██████╗      █████╗     ████████╗    ███████╗    ██╗    ██╗     █████╗     ██╗   ██╗\r\n╚══██╔══╝    ██║  ██║    ██║    ████╗  ██║    ██╔════╝     ██╔════╝    ██╔════╝     ██╔══██╗    ╚══██╔══╝    ██╔════╝    ██║    ██║    ██╔══██╗    ╚██╗ ██╔╝\r\n   ██║       ███████║    ██║    ██╔██╗ ██║    ██║  ███╗    ███████╗    ██║  ███╗    ███████║       ██║       █████╗      ██║ █╗ ██║    ███████║     ╚████╔╝ \r\n   ██║       ██╔══██║    ██║    ██║╚██╗██║    ██║   ██║    ╚════██║    ██║   ██║    ██╔══██║       ██║       ██╔══╝      ██║███╗██║    ██╔══██║      ╚██╔╝  \r\n   ██║       ██║  ██║    ██║    ██║ ╚████║    ╚██████╔╝    ███████║    ╚██████╔╝    ██║  ██║       ██║       ███████╗    ╚███╔███╔╝    ██║  ██║       ██║   \r\n   ╚═╝       ╚═╝  ╚═╝    ╚═╝    ╚═╝  ╚═══╝     ╚═════╝     ╚══════╝     ╚═════╝     ╚═╝  ╚═╝       ╚═╝       ╚══════╝     ╚══╝╚══╝     ╚═╝  ╚═╝       ╚═╝   \r\n                                                                                                                                                            ");
        Console.WriteLine(TP.Wrapper("ThingsGateway边缘网关",
         "设备采集，多向扩展",
         "##作者## Diego",
         $"##当前版本## {Assembly.GetExecutingAssembly().GetName().Version}",
         "##文档地址## https://diego2098.gitee.io/thingsgateway-docs/",
         "##作者信息## Diego QQ 2248356998"));
        Console.ResetColor();
        #endregion

        var builder = WebApplication.CreateBuilder(args);

        //注意，在发布生产环境时，必须使用dotnet publish功能，否则会生成staticwebassets.runtime.json，导致文件路径失效闪退

        //注意，单文件发布目前是不支持的，原因是因为sugar的td驱动不支持单文件发布(或者还有其他问题)，如果你没有用到TDengine插件，也可以用单文件发布

        builder.WebHost.UseWebRoot("wwwroot");
        builder.WebHost.UseStaticWebAssets();
        builder.Host.UseWindowsService();
        builder.Host.UseSystemd();

#if AF2021

        builder.WebHost.UseKestrel(
    o =>
    {
        var config = Furion.App.GetConfig<ThingsGateway.Gateway.LK.MqttConfig>("MqttConfig", true);
        if (config.Enable)
        {
            o.ListenAnyIP(config.Port, a => MQTTnet.AspNetCore.ConnectionBuilderExtensions.UseMqtt(a));
            o.ListenAnyIP(config.WebSocketPort);
        }
    });
#endif

        //Furion便利方法
        builder.Inject();
        var app = builder.Build();
        return app.RunAsync();

    }

}
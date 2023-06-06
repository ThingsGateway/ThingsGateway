#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion


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
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        System.IO.Directory.SetCurrentDirectory(AppContext.BaseDirectory);


#if KINGVIEW //读取组态王不能后台启动，所以这里多出来一个解决方案配置
        SevicesExtension.KINGVIEWCONFIG();
#endif

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseWebRoot("wwwroot");
        builder.WebHost.UseStaticWebAssets();
        //需要服务守护可安装
        builder.Host.UseWindowsService();
        builder.Host.UseSystemd();
        //Furion便利方法
        builder.Inject();
        var app = builder.Build();
        app.Run();
    }
}
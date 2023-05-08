using Furion.Templates;

using System.Drawing;

namespace ThingsGateway.Web.Core;


/// <summary>
/// logo显示
/// </summary>
public static class LogoExtension
{
    /// <summary>
    /// 添加Logo显示
    /// </summary>
    /// <param name="services"></param>
    public static void AddLogoDisplay(this IServiceCollection services)
    {
        Colorful.Console.WriteAsciiAlternating("ThingsGateway", new Colorful.FrequencyBasedColorAlternator(3, Color.Yellow, Color.GreenYellow));
        var template = TP.Wrapper("ThingsGateway边缘网关",
         "设备采集，多向扩展",
         "##作者## Diego",
         "##当前版本## " + Assembly.GetExecutingAssembly().GetName().Version,
         "##文档地址## " + @"https://diego2098.gitee.io/thingsgateway/",
         "##作者信息## Diego QQ 2248356998") + Environment.NewLine;
        Colorful.Console.WriteAlternating(template, new Colorful.FrequencyBasedColorAlternator(3, Color.Yellow, Color.GreenYellow));

    }
}
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
         "##文档地址## " + @"https://diego2098.gitee.io/thingsgateway-docs/",
         "##作者信息## Diego QQ 2248356998") + Environment.NewLine;
        Colorful.Console.WriteAlternating(template, new Colorful.FrequencyBasedColorAlternator(3, Color.Yellow, Color.GreenYellow));

    }
}
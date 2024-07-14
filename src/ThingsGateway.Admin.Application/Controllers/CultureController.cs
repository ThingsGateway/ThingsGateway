//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 文化 Controller
/// </summary>
[Route("[controller]/[action]")]
public class CultureController : Controller
{
    /// <summary>
    /// 重置文化方法
    /// </summary>
    /// <param name="redirectUri"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult ResetCulture(string redirectUri)
    {
        HttpContext.Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);

        return LocalRedirect(redirectUri);
    }

    /// <summary>
    /// 设置文化方法
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="redirectUri"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult SetCulture(string culture, string redirectUri)
    {
        if (string.IsNullOrEmpty(culture))
        {
            HttpContext.Response.Cookies.Delete(CookieRequestCultureProvider.DefaultCookieName);
        }
        else
        {
            HttpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)), new CookieOptions()
                {
                    Expires = DateTimeOffset.Now.AddYears(1)
                });

            //更改全局文化，采集后台也会变化
            //var cultureInfo = new CultureInfo(culture);
            //CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            //CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            //CultureInfo.CurrentCulture = cultureInfo;
            //CultureInfo.CurrentUICulture = cultureInfo;
        }

        return LocalRedirect(redirectUri);
    }
}

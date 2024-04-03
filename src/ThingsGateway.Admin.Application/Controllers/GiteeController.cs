//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using ThingsGateway.Core;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// Gitee WebHook
/// </summary>
[Route("api/[controller]/[action]")]
[ApiController]
public class GiteeController : ControllerBase
{
    /// <summary>
    /// Gitee Webhook
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Webhook([FromQuery] string? id, [FromServices] IConfiguration config, [FromServices] IDispatchService<GiteePostBody> dispatch, [FromBody] GiteePostBody payload)
    {
        bool ret = false;
        if (Check())
        {
            // 全局推送
            if (payload.HeadCommit != null || payload.Commits?.Count > 0)
            {
                dispatch.Dispatch(new DispatchEntry<GiteePostBody>()
                {
                    Name = "Gitee",
                    Entry = payload
                });
            }
            ret = true;
        }
        return ret ? Ok() : Unauthorized();

        bool Check()
        {
            var configId = config.GetValue<string>("WebHooks:Gitee:Id");
            var configToken = config.GetValue<string>("WebHooks:Gitee:Token");
            var token = "";
            if (Request.Headers.TryGetValue("X-Gitee-Token", out var val))
            {
                token = val.FirstOrDefault() ?? string.Empty;
            }
            return id == configId && token == configToken
                    && payload.Id == configId && payload.Password == configToken;
        }
    }

    /// <summary>
    /// Webhook 测试接口
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Webhook()
    {
        return Ok(new { Message = "Ok" });
    }

    /// <summary>
    /// 跨域握手协议
    /// </summary>
    /// <returns></returns>
    [HttpOptions]
    public string Options() => string.Empty;
}
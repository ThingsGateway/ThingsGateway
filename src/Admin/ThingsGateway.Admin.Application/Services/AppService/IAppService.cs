//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------


using System.Net;
using System.Security.Claims;

using UAParser;

namespace ThingsGateway.Admin.Application;

public interface IAppService
{
    /// <summary>
    /// ClientInfo
    /// </summary>
    public ClientInfo? ClientInfo { get; }

    /// <summary>
    /// ClaimsPrincipal
    /// </summary>
    public ClaimsPrincipal? User { get; }

    /// <summary>
    /// RemoteIpAddress
    /// </summary>
    public IPAddress? RemoteIpAddress { get; }

    /// <summary>
    /// GetReturnUrl
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    public string GetReturnUrl(string returnUrl);

    /// <summary>
    /// LoginOutAsync
    /// </summary>
    public Task LoginOutAsync();

    /// <summary>
    /// LoginAsync
    /// </summary>
    /// <returns></returns>
    public Task LoginAsync(ClaimsIdentity claimsIdentity, int expire);



}



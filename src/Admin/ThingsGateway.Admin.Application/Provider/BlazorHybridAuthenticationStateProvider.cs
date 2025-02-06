//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Authorization;

using System.Security.Claims;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
public class BlazorHybridAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAppService _appService;
    private CancellationTokenSource _loopCancellationTokenSource = new CancellationTokenSource();

    public BlazorHybridAuthenticationStateProvider(IAppService appService)
    {
        _appService = appService;
    }

    private AuthenticationState AuthenticationState = new AuthenticationState(new ClaimsPrincipal());
    public void UserChanged(ClaimsPrincipal user)
    {
        AuthenticationState = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        var oldCancellationTokenSource = _loopCancellationTokenSource;
        if (oldCancellationTokenSource is not null)
        {
            oldCancellationTokenSource.Cancel();
            oldCancellationTokenSource.Dispose();
        }

        _loopCancellationTokenSource = new CancellationTokenSource();
        if (AuthenticationState.User?.Identity?.IsAuthenticated == true)
            _ = RevalidationLoop(_loopCancellationTokenSource.Token);
    }
    private async Task RevalidationLoop(CancellationToken cancellationToken)
    {
        try
        {
            if (AuthenticationState.User.Identity?.IsAuthenticated == true)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    bool isValid = false;

                    try
                    {
                        await Task.Delay(30000, cancellationToken).ConfigureAwait(false);
                        isValid = await ValidateAuthenticationStateAsync(AuthenticationState, cancellationToken).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }

                    if (!isValid)
                    {
                        ForceSignOut();
                        break;
                    }
                }
            }
        }
        catch
        {
            ForceSignOut();
        }
    }
    protected virtual async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var userId = authenticationState.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.UserId)?.Value?.ToLong();
        var verificatId = authenticationState.User.Claims.FirstOrDefault(it => it.Type == ClaimConst.VerificatId)?.Value?.ToLong();
        var result = BlazorAuthenticationHandler.CheckVerificat(userId, verificatId, false);
        if (!result)
        {
            var verificatInfo = App.GetService<IVerificatInfoService>().GetOne(verificatId ?? 0, false);
            if (App.HttpContext != null)
            {
                var identity = new ClaimsIdentity();
                App.HttpContext.User = new ClaimsPrincipal(identity);
            }

            if (verificatInfo != null)
                await App.GetService<INoticeService>().UserLoginOut(verificatInfo.ClientIds, App.CreateLocalizerByType(typeof(BlazorAuthenticationHandler))["UserExpire"]).ConfigureAwait(false);
        }
        return result;
    }
    private void ForceSignOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        UserChanged(anonymousUser);
    }
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(AuthenticationState);
    }
}

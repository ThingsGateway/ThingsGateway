
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Microsoft.AspNetCore.SignalR.Client;

using System.Security.Claims;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class SysSignalR : IAsyncDisposable
{
    private HubConnection _hubConnection;

    [Inject]
    private ToastService ToastService { get; set; }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                //SignalR
                _hubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri(HubConst.SysHubUrl), (opts) =>
            {
                opts.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                    {
                        // 绕过SSL证书
                        clientHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            return true;
                        };
                    };
                    return message;
                };
                foreach (var item in App.User?.Claims ?? new List<Claim>())
                {
                    if (item.Type == ClaimConst.UserId || item.Type == ClaimConst.VerificatId)
                        opts.Headers.Add(item.Type, item.Value);
                }
            }
            ).Build();

                _hubConnection.On<string>(nameof(ISysHub.LoginOut), async (message) =>
                {
                    await InvokeAsync(async () => await ToastService.Warning(message));
                    await Task.Delay(2000);
                    NavigationManager.NavigateTo(NavigationManager.Uri, true);
                });
                _hubConnection.On<SignalRMessage>(nameof(ISysHub.NewMessage), async (message) =>
                {
                    if ((byte)message.LogLevel <= 2)
                        await InvokeAsync(async () => await ToastService.Information(message.Data));
                    else
                        await InvokeAsync(async () => await ToastService.Warning(message.Data));
                });
                await _hubConnection.StartAsync();
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
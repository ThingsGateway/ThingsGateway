//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------


using ThingsGateway.Admin.Application;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Razor;

public partial class SysSignalR : ComponentBase, IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private ToastService ToastService { get; set; }

    [Inject]
    private ISysHub SysHub { get; set; }
    [Inject]
    private IEventService<SignalRMessage> NewMessage { get; set; }
    [Inject]
    private IEventService<string> LoginOut { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        SysHub.UpdateVerificat(ClientId, VerificatId, isConnect: false);
        var clientId = ClientId.ToString();
        NewMessage.UnSubscribe(clientId);
        LoginOut.UnSubscribe(clientId);
    }
    private long VerificatId;
    private long ClientId;
    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                ClientId = YitIdHelper.NextId();
                VerificatId = UserManager.VerificatId;
                var clientId = ClientId.ToString();
                LoginOut.Subscribe(clientId, async (message) =>
                {
                    await InvokeAsync(async () => await ToastService.Warning(message));
                    await Task.Delay(2000);
                    NavigationManager.NavigateTo(NavigationManager.Uri, true);
                });
                NewMessage.Subscribe(clientId, async (message) =>
                {
                    if ((byte)message.LogLevel <= 2)
                        await InvokeAsync(async () => await ToastService.Information(message.Data));
                    else
                        await InvokeAsync(async () => await ToastService.Warning(message.Data));
                });

                SysHub.UpdateVerificat(ClientId, VerificatId, isConnect: true);
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

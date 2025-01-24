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
using ThingsGateway.Extension.Generic;

namespace ThingsGateway.Admin.Razor;

public partial class LoginConnectionHub : ComponentBase, IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private ToastService ToastService { get; set; }

    [Inject]
    private IVerificatInfoService VerificatInfoService { get; set; }
    [Inject]
    private IEventService<AppMessage> NewMessage { get; set; }
    [Inject]
    private IEventService<UserLoginOut> LoginOut { get; set; }
    [Inject]
    private IEventService<NavigationUri> NavigationUri { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        UpdateVerificat(ClientId, VerificatId, isConnect: false);
        var clientId = ClientId.ToString();
        NewMessage.UnSubscribe(clientId);
        LoginOut.UnSubscribe(clientId);
        NavigationUri.UnSubscribe(clientId);
    }
    private long VerificatId;
    private long ClientId;
    protected override Task OnInitializedAsync()
    {
        try
        {
            ClientId = CommonUtils.GetSingleId();
            VerificatId = UserManager.VerificatId;
            var clientId = ClientId.ToString();
            LoginOut.Subscribe(clientId, async (message) =>
            {
                await InvokeAsync(async () => await ToastService.Warning(message.Message));
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
            NavigationUri.Subscribe(clientId, async (message) =>
            {
                await ShowMessage(message);
            });
            UpdateVerificat(ClientId, VerificatId, isConnect: true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            NewLife.Log.XTrace.WriteException(ex);
        }

        return base.OnInitializedAsync();
    }

    [Inject]
    private IStringLocalizer<LoginConnectionHub> Localizers { get; set; }
    [Inject]
    private MessageService MessageService { get; set; }
    private async Task ShowMessage(NavigationUri navigationUri)
    {
        await MessageService.Show(new MessageOption()
        {
            Icon = "fa-solid fa-circle-info",
            ShowDismiss = true,
            IsAutoHide = false,
            ChildContent = RenderItem(navigationUri),
            OnDismiss = () =>
            {
                return Task.CompletedTask;
            }
        });
    }
    /// <summary>
    /// 更新cache
    /// </summary>
    /// <param name="clientId">用户id</param>
    /// <param name="verificatId">上线时的验证id</param>
    /// <param name="isConnect">上线</param>
    private void UpdateVerificat(long clientId, long verificatId = 0, bool isConnect = true)
    {
        if (clientId != 0)
        {
            //获取cache当前用户的verificat信息列表
            if (isConnect)
            {
                //获取cache中当前verificat
                var verificatInfo = VerificatInfoService.GetOne(verificatId);
                if (verificatInfo != null)
                {
                    verificatInfo.ClientIds.Add(clientId);//添加到客户端列表
                    VerificatInfoService.Update(verificatInfo);//更新Cache
                }
            }
            else
            {
                //获取当前客户端ID所在的verificat信息
                var verificatInfo = VerificatInfoService.GetOne(verificatId);
                if (verificatInfo != null)
                {
                    verificatInfo.ClientIds.RemoveWhere(it => it == clientId);//从客户端列表删除
                    VerificatInfoService.Update(verificatInfo);//更新Cache
                }
            }
        }
    }

}

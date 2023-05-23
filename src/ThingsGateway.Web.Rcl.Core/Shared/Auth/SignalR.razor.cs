#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.SignalR.Client;

using System.Net.Http;

using ThingsGateway.Web.Rcl.Core;

namespace ThingsGateway.Web.Rcl
{
    public partial class SignalR
    {
        public HubConnection _hubConnection;

        [Inject]
        public AjaxService AjaxService { get; set; }

        protected override async Task DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                await _hubConnection.DisposeAsync();
            }
            await base.DisposeAsync(disposing);
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                //SignalR
                _hubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri(HubConst.HubUrl), (opts) =>
            {
                opts.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                    {
                        // �ƹ�SSL֤��
                        clientHandler.ServerCertificateCustomValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            return true;
                        };
                    };
                    return message;
                };
                opts.Headers = new Dictionary<string, string>();
                foreach (var item in App.User?.Claims)
                {
                    if (item.Type == ClaimConst.UserId || item.Type == ClaimConst.VerificatId)
                        opts.Headers.Add(item.Type, item.Value);
                }
            }
            ).Build();
                _hubConnection.On<object>("LoginOut", async (message) =>
                {
                    try
                    {
                        await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync(message.ToString(), AlertTypes.Warning));

                    }
                    catch (Exception ex)
                    {
                    }
                    await Task.Delay(2000);
                    await AjaxService.GotoAsync("/");
                });

                await _hubConnection.StartAsync();
            }
            catch
            {

            }
            await base.OnInitializedAsync();
        }
    }
}
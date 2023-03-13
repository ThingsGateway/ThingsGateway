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

        protected override async Task Dispose(bool disposing)
        {
            if (disposing)
            {
                await _hubConnection.DisposeAsync();
            }
            await base.Dispose(disposing);
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
                        // ÈÆ¹ýSSLÖ¤Êé
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
                    await PopupService.EnqueueSnackbarAsync(message.ToString(), AlertTypes.Warning);
                    await Task.Delay(2000);
                    await AjaxService.Goto("/");
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
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Demo
{
    public partial class OpcUaMaster
    {
        private ThingsGateway.Foundation.OpcUa.OpcUaMaster Plc => opcUaMasterConnectPage.Plc;
        private OpcUaMasterConnectPage opcUaMasterConnectPage;
        private string RegisterAddress;
        private AdapterDebugPage adapterDebugPage { get; set; }

        private void OnConnectClick()
        {
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                //载入配置
                StateHasChanged();
            }
        }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        private string WriteValue;

        private async Task Add()
        {
            if (Plc.Connected)
                await Plc.AddSubscriptionAsync(Guid.NewGuid().ToString(), new[] { RegisterAddress });
            else
            {
                opcUaMasterConnectPage.LogMessage?.LogWarning($"未连接");
            }
        }

        private async Task WriteAsync()
        {
            try
            {
                if (Plc.Connected)
                {
                    var data = await Plc.WriteNodeAsync(
                        new()
                        {
                        {RegisterAddress, WriteValue.GetJTokenFromString()}
                        }
                        );

                    foreach (var item in data)
                    {
                        if (item.Value.Item1)
                            opcUaMasterConnectPage.LogMessage?.LogInformation(item.ToJsonString());
                        else
                            opcUaMasterConnectPage.LogMessage?.LogWarning(item.ToJsonString());
                    }
                }
                else
                {
                    opcUaMasterConnectPage.LogMessage?.LogWarning($"未连接");
                }
            }
            catch (Exception ex)
            {
                opcUaMasterConnectPage.LogMessage?.LogWarning(ex, $"写入失败");
            }
        }

        private async Task ShowImport()
        {
            await PopupService.OpenAsync(typeof(OpcUaImportVariable), new Dictionary<string, object?>()
        {
            {nameof(OpcUaImportVariable.Plc),Plc},
            {nameof(OpcUaImportVariable.IsShowSubvariable),IsShowSubvariable},
        });
        }

        private bool IsShowSubvariable;

        private async Task ReadAsync()

        {
            if (Plc.Connected)
            {
                try
                {
                    var data = await Plc.ReadJTokenValueAsync(new string[] { RegisterAddress });

                    opcUaMasterConnectPage.LogMessage?.LogInformation($" {data[0].Item1}：{data[0].Item3}");
                }
                catch (Exception ex)
                {
                    opcUaMasterConnectPage.LogMessage?.LogWarning(ex, $"读取失败");
                }
            }
            else
            {
                opcUaMasterConnectPage.LogMessage?.LogWarning($"未连接");
            }
        }

        private void Remove()
        {
            if (Plc.Connected)
                Plc.RemoveSubscription("");
            else
            {
                opcUaMasterConnectPage.LogMessage?.LogWarning($"未连接");
            }
        }
    }
}
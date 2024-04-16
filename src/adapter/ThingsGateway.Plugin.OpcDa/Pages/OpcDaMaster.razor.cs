//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using ThingsGateway.Debug;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.OpcDa.Da;

using TouchSocket.Core;

namespace ThingsGateway.Debug
{
    public partial class OpcDaMaster
    {
        private ThingsGateway.Foundation.OpcDa.OpcDaMaster Plc => opcDaMasterConnectPage.Plc;
        private OpcDaMasterConnectPage opcDaMasterConnectPage;
        private string RegisterAddress;

        private string LogPath;
        private AdapterDebugComponent AdapterDebugPage { get; set; }

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

        private void Add()
        {
            var tags = new Dictionary<string, List<OpcItem>>();
            var tag = new OpcItem(RegisterAddress);
            tags.Add(Guid.NewGuid().ToString(), new List<OpcItem>() { tag });
            try
            {
                Plc.AddItems(tags);
            }
            catch (Exception ex)
            {
                opcDaMasterConnectPage.LogMessage?.LogWarning(ex, $"添加失败");
            }
        }

        private async Task WriteAsync()
        {
            try
            {
                JToken tagValue = WriteValue.GetJTokenFromString();
                var obj = tagValue.GetObjectFromJToken();

                var data = Plc.WriteItem(
                    new()
                    {
                    {RegisterAddress,  obj}
                    }
                    );
                if (data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        if (item.Value.Item1)
                            opcDaMasterConnectPage.LogMessage?.LogInformation(item.ToJsonString());
                        else
                            opcDaMasterConnectPage.LogMessage?.LogWarning(item.ToJsonString());
                    }
                }
            }
            catch (Exception ex)
            {
                opcDaMasterConnectPage.LogMessage?.LogWarning(ex, $"写入失败");
            }

            await Task.CompletedTask;
        }

        private async Task ShowImport()
        {
            await PopupService.OpenAsync(typeof(OpcDaImportVariable), new Dictionary<string, object?>()
        {
            {nameof(OpcDaImportVariable.Plc),Plc},
        });
        }

        private async Task ReadAsync()
        {
            try
            {
                Plc.ReadItemsWithGroup();
            }
            catch (Exception ex)
            {
                opcDaMasterConnectPage.LogMessage?.LogWarning(ex, $"读取失败");
            }

            await Task.CompletedTask;
        }

        private void Remove()
        {
            Plc.RemoveItems(new List<string>() { RegisterAddress });
        }
    }
}
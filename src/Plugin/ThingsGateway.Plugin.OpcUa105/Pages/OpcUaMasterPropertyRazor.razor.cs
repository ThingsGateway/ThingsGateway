// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using ThingsGateway.Razor;

namespace ThingsGateway.Plugin.OpcUa105
{
    public partial class OpcUaMasterPropertyRazor : IPropertyUIBase
    {
        [Parameter, EditorRequired]
        public string Id { get; set; }
        [Parameter, EditorRequired]
        public bool CanWrite { get; set; }
        [Parameter, EditorRequired]
        public ModelValueValidateForm Model { get; set; }

        [Parameter, EditorRequired]
        public IEnumerable<IEditorItem> PluginPropertyEditorItems { get; set; }

        IStringLocalizer Localizer { get; set; }

        protected override Task OnParametersSetAsync()
        {
            Localizer = App.CreateLocalizerByType(Model.Value.GetType());

            return base.OnParametersSetAsync();
        }

        [Inject]
        private DownloadService DownloadService { get; set; }
        [Inject]
        private ToastService ToastService { get; set; }


        private async Task Export()
        {
            try
            {
                var plc = new ThingsGateway.Foundation.OpcUa105.OpcUaMaster();
                await plc.CheckApplicationInstanceCertificate().ConfigureAwait(false);
                string path = $"{AppContext.BaseDirectory}OPCUAClientCertificate/pki/trustedPeer/certs";
                Directory.CreateDirectory(path);
                var files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    return;
                }
                foreach (var item in files)
                {
                    using var fileStream = new FileStream(item, FileMode.Open, FileAccess.Read);

                    var extension = Path.GetExtension(item);
                    extension ??= ".der";

                    await DownloadService.DownloadFromStreamAsync($"ThingsGateway{extension}", fileStream).ConfigureAwait(false);
                }
                await ToastService.Default();
            }
            catch (Exception ex)
            {
                await ToastService.Warn(ex);
            }

        }
    }
}
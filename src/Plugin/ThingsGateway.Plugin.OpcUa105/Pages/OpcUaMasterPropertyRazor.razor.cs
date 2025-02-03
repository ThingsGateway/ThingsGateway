// ------------------------------------------------------------------------------
// �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
// �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
// Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
// GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
// GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
// ʹ���ĵ���https://thingsgateway.cn/
// QQȺ��605534569
// ------------------------------------------------------------------------------

#pragma warning disable CA2007 // ���ǶԵȴ���������� ConfigureAwait
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
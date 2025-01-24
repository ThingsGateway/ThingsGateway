// ------------------------------------------------------------------------------
// �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
// �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
// Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
// GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
// GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
// ʹ���ĵ���https://thingsgateway.cn/
// QQȺ��605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using ThingsGateway.Razor;

namespace ThingsGateway.Debug
{
    public partial class SqlDBProducerPropertyRazor : IPropertyUIBase
    {


        [Parameter, EditorRequired]
        public IEnumerable<IEditorItem> PluginPropertyEditorItems { get; set; }
        [Parameter, EditorRequired]
        public string Id { get; set; }
        [Parameter, EditorRequired]
        public bool CanWrite { get; set; }
        [Parameter, EditorRequired]
        public ModelValueValidateForm Model { get; set; }

        IStringLocalizer SqlDBProducerPropertyLocalizer { get; set; }

        protected override Task OnParametersSetAsync()
        {
            SqlDBProducerPropertyLocalizer = App.CreateLocalizerByType(Model.Value.GetType());

            return base.OnParametersSetAsync();
        }


    }
}
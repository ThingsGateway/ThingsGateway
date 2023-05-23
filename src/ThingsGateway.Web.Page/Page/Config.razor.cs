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

namespace ThingsGateway.Web.Rcl
{
    public partial class Config
    {
        private List<DevConfig> _alarmConfig = new();
        private List<DevConfig> _hisConfig = new();
        protected override async Task OnInitializedAsync()
        {
            _alarmConfig = await ConfigService.GetListByCategory(ThingsGatewayConst.ThingGateway_AlarmConfig_Base);
            _hisConfig = await ConfigService.GetListByCategory(ThingsGatewayConst.ThingGateway_HisConfig_Base);
            await base.OnInitializedAsync();
        }


    }
}
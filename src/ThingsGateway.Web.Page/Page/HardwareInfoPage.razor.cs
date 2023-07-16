#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/dotnetchina/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using TouchSocket.Core;

namespace ThingsGateway.Web.Page
{
    public partial class HardwareInfoPage
    {
        private System.Timers.Timer DelayTimer;
        [Inject]
        HardwareInfoService HardwareInfoService { get; set; }

        protected override Task OnInitializedAsync()
        {
            DelayTimer = new System.Timers.Timer(8000);
            DelayTimer.Elapsed += timer_Elapsed;
            DelayTimer.AutoReset = true;
            DelayTimer.Start();
            return base.OnInitializedAsync();
        }

        async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await InvokeAsync(StateHasChanged);
        }

        protected override async Task DisposeAsync(bool disposing)
        {
            await base.DisposeAsync(disposing);
            DelayTimer?.SafeDispose();
        }
    }
}
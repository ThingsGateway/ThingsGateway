#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.Components;

using System.Threading;

using ThingsGateway.Application;

namespace ThingsGateway.Blazor;

/// <summary>
/// Ӳ����Ϣҳ��
/// </summary>
public partial class HardwareInfoPage
{
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(5));

    [Inject]
    HardwareInfoService HardwareInfoService { get; set; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }
    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }

        }
    }

}
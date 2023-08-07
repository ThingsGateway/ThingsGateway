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

using Mapster;

using SqlSugar;

using System.Threading;

using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

namespace ThingsGateway.Blazor;
/// <summary>
/// ʵʱ����
/// </summary>
public partial class RealAlarmPage
{
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(5));
    private IAppDataTable _datatable;
    AlarmWorker AlarmHostService { get; set; }
    VariablePageInput SearchModel { get; set; } = new();
    /// <inheritdoc/>
    public override void Dispose()
    {
        _periodicTimer.Dispose();
        base.Dispose();
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        AlarmHostService = ServiceHelper.GetBackgroundService<AlarmWorker>();
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private async Task DatatableQuery()
    {
        await _datatable?.QueryClickAsync();
    }

    private Task<SqlSugarPagedList<HistoryAlarm>> QueryCallAsync(VariablePageInput input)
    {
        var devices = AlarmHostService.RealAlarmDeviceVariables.Adapt<List<HistoryAlarm>>();
        var data = devices
            .WhereIF(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName.Contains(input.DeviceName))
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name))
            .ToList().ToPagedList(input);
        return Task.FromResult(data);
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await InvokeAsync(DatatableQuery);
            }
            catch
            {
            }

        }
    }
}
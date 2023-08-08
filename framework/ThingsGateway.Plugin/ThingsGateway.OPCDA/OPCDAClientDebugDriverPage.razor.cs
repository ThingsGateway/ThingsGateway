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

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Blazor;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.OPCDA;

/// <summary>
/// OPCDA����ҳ��
/// </summary>
public partial class OPCDAClientDebugDriverPage : IDisposable
{
    private ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient _plc;
    private DefalutDebugDriverPage defalutDebugDriverPage;
    bool IsShowImportVariableList;
    private OPCDAClientPage opcDAClientPage;
    private ImportVariable ImportVariable { get; set; }
    [Inject]
    IPopupService PopupService { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        _plc.SafeDispose();
        opcDAClientPage.SafeDispose();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return nameof(OPCDAClient);
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            opcDAClientPage.LogAction = defalutDebugDriverPage.LogOut;
            opcDAClientPage.ValueAction = ValueOut;
            //��������
            _plc = opcDAClientPage.OPC;
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }
    [Inject]
    private InitTimezone InitTimezone { get; set; }
    private void Add()
    {
        var tags = new Dictionary<string, List<OpcItem>>();
        var tag = new OpcItem(defalutDebugDriverPage.Address);
        tags.Add(YitIdHelper.NextId().ToString(), new List<OpcItem>() { tag });
        var result = _plc.AddItems(tags);
        if (!result.IsSuccess)
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + result.Message));
        }
    }

    private async Task DownDeviceExport()
    {
        var data = ImportVariable?.GetImportVariableList();
        if (data != null)
        {
            if (data?.Item2?.Count == 0)
            {
                await PopupService.EnqueueSnackbarAsync("�޿��ñ���", AlertTypes.Warning);
                return;
            }
            await defalutDebugDriverPage.DownDeviceExportAsync(data?.Item1);
            await defalutDebugDriverPage.DownDeviceExportAsync(data?.Item2);
            await PopupService.EnqueueSnackbarAsync("�ɹ�", AlertTypes.Success);
        }
    }

    private Task ReadAsync()
    {
        var data = _plc.ReadItemsWithGroup();
        if (data.IsSuccess)
        {
        }
        else
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + data.Message));
        }

        return Task.CompletedTask;
    }
    private void Remove()
    {
        _plc.RemoveItems(new List<string>() { defalutDebugDriverPage.Address });
    }

    private void ValueOut(List<ItemReadResult> values)
    {
        defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + values.ToJson()));
        if (defalutDebugDriverPage.Messages.Count > 2500)
        {
            defalutDebugDriverPage.Messages.Clear();
        }
    }

    private Task WriteAsync()
    {
        try
        {
            var data = _plc.WriteItem(defalutDebugDriverPage.Address, defalutDebugDriverPage.WriteValue);
            if (data.IsSuccess)
            {
                defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - д��" + data.Message));
            }
            else
            {
                defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + data.Message));
            }
        }
        catch (Exception ex)
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + "д��ʧ�ܣ�" + ex.Message));
        }

        return Task.CompletedTask;
    }
}
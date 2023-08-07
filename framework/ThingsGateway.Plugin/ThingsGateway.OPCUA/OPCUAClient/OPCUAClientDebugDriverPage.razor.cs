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

using Newtonsoft.Json.Linq;

using Opc.Ua;

using System;
using System.Threading.Tasks;

using ThingsGateway.Admin.Core;
using ThingsGateway.Blazor;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.OPCUA;
/// <summary>
/// OPCUA����ҳ��
/// </summary>
public partial class OPCUAClientDebugDriverPage
{
    private ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient _plc;
    private DefalutDebugDriverPage defalutDebugDriverPage;
    bool IsShowImportVariableList;
    private OPCUAClientPage opcUAClientPage;
    private ImportVariable ImportVariable { get; set; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        _plc.SafeDispose();
        opcUAClientPage.SafeDispose();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return nameof(OPCUAClient);
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            opcUAClientPage.LogAction = defalutDebugDriverPage.LogOut;
            //��������
            _plc = opcUAClientPage.OPC;
            _plc.DataChangedHandler += Plc_DataChangedHandler;
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }

    private void Add()
    {
        if (_plc.Connected)
            _plc.AddSubscription(YitIdHelper.NextId().ToString(), new[] { defalutDebugDriverPage.Address });
        else
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "δ����"));
        }
    }
    [Inject]
    IPopupService PopupService { get; set; }
    private async Task DownDeviceExport()
    {
        var data = await ImportVariable?.GetImportVariableListAsync();
        if (data.Item2?.Count == 0)
        {
            await PopupService.EnqueueSnackbarAsync("�޿��ñ���", AlertTypes.Warning);
            return;
        }
        await defalutDebugDriverPage.DownDeviceExportAsync(data.Item1);
        await defalutDebugDriverPage.DownDeviceExportAsync(data.Item2);
        await PopupService.EnqueueSnackbarAsync("�ɹ�", AlertTypes.Success);
    }

    private void Plc_DataChangedHandler((VariableNode variableNode, DataValue dataValue, Newtonsoft.Json.Linq.JToken jToken) item)
    {
        defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + (item.variableNode.NodeId + ":" + item.jToken)));
        if (defalutDebugDriverPage.Messages.Count > 2500)
        {
            defalutDebugDriverPage.Messages.Clear();
        }

    }
    private async Task ReadAsync()
    {
        if (_plc.Connected)
        {
            try
            {
                var data = await _plc.ReadJTokenValueAsync(new string[] { defalutDebugDriverPage.Address });
                defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + data.ToJson()));
            }
            catch (Exception ex)
            {

                defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + ex.Message));
            }
        }
        else
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "δ����"));
        }
    }
    private void Remove()
    {
        if (_plc.Connected)
            _plc.RemoveSubscription("");
        else
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "δ����"));
        }
    }

    private async Task WriteAsync()
    {
        try
        {
            if (_plc.Connected)
            {
                var data = await _plc.WriteNodeAsync(defalutDebugDriverPage.Address, JToken.Parse(defalutDebugDriverPage.WriteValue));
                if (data.IsSuccess)
                {
                    defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + " - д��ɹ�"));
                }
                else
                {
                    defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + " - д��ʧ�� " + data.Message));
                }
            }
            else
            {
                defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "δ����"));
            }
        }
        catch (Exception ex)
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + " - " + "д��ʧ�ܣ�" + ex.Message));
        }
    }
}
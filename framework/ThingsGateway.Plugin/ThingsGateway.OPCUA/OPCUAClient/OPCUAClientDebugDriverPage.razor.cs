#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
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
/// OPCUA调试页面
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
            //载入配置
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
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Debug, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "未连接"));
        }
    }
    [Inject]
    IPopupService PopupService { get; set; }
    private async Task DownDeviceExport()
    {
        var data = await ImportVariable?.GetImportVariableListAsync();
        if (data.Item2?.Count == 0)
        {
            await PopupService.EnqueueSnackbarAsync("无可用变量", AlertTypes.Warning);
            return;
        }
        await defalutDebugDriverPage.DownDeviceExportAsync(data.Item1);
        await defalutDebugDriverPage.DownDeviceExportAsync(data.Item2);
        await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
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
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "未连接"));
        }
    }
    private void Remove()
    {
        if (_plc.Connected)
            _plc.RemoveSubscription("");
        else
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "未连接"));
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
                    defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + " - 写入成功"));
                }
                else
                {
                    defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + " - 写入失败 " + data.Message));
                }
            }
            else
            {
                defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "未连接"));
            }
        }
        catch (Exception ex)
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + " - " + "写入失败：" + ex.Message));
        }
    }
}
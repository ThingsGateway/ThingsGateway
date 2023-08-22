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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Blazor;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;

using TouchSocket.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.OPCDA;

/// <summary>
/// OPCDA调试页面
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
            //载入配置
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
                await PopupService.EnqueueSnackbarAsync("无可用变量", AlertTypes.Warning);
                return;
            }
            await defalutDebugDriverPage.DownDeviceExportAsync(data?.Item1);
            await defalutDebugDriverPage.DownDeviceVariableExportAsync(data?.Item2, data?.Item1.Name);
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
        }
    }
    private async Task DeviceImport()
    {
        var data = ImportVariable?.GetImportVariableList();
        if (data != null)
        {
            if (data?.Item2?.Count == 0)
            {
                await PopupService.EnqueueSnackbarAsync("无可用变量", AlertTypes.Warning);
                return;
            }
            await defalutDebugDriverPage.DeviceImportAsync(data?.Item1);
            await defalutDebugDriverPage.DeviceVariableImportAsync(data?.Item2);
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
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
            var data = _plc.WriteItem(
                new()
                {
                    {defalutDebugDriverPage.Address, defalutDebugDriverPage.WriteValue }
                }
                );
            if (data.Values.FirstOrDefault().IsSuccess)
            {
                defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - 写入" + data.Values.FirstOrDefault().Message));
            }
            else
            {
                defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + data.Values.FirstOrDefault().Message));
            }
        }
        catch (Exception ex)
        {
            defalutDebugDriverPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset) + " - " + "写入失败：" + ex.Message));
        }

        return Task.CompletedTask;
    }
}
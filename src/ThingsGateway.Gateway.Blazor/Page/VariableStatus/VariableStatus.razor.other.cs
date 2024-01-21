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

using Mapster;

using Masa.Blazor;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class VariableStatus
{
    private List<Device> _devices = new();
    private List<Device> _businessDevices = new();

    private void FilterHeaders(List<DataTableHeader<VariableRunTime>> datas)
    {
        datas.RemoveWhere(it => it.Value == nameof(VariableRunTime.DeviceId));
        var data = datas.FirstOrDefault(a => a.Value == BlazorAppService.DataTableActions);
        data.Width = 170;
    }

    private void Filters(List<Filters> datas)
    {
        foreach (var item in datas)
        {
            switch (item.Key)
            {
                case nameof(VariableRunTime.WriteExpressions):
                case nameof(VariableRunTime.ReadExpressions):
                case nameof(VariableRunTime.ChangeTime):
                case nameof(VariableRunTime.LastSetValue):
                case nameof(VariableRunTime.Unit):
                case nameof(VariableRunTime.IntervalTime):
                case nameof(VariableRunTime.RpcWriteEnable):
                case nameof(VariableRunTime.ProtectType):
                case nameof(VariableRunTime.Index):
                case nameof(VariableRunTime.DataType):
                    item.Value = false;
                    break;
            }
        }
    }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        _devices = (_serviceScope.ServiceProvider.GetService<IDeviceService>().GetCacheList()).Where(a => a.PluginType == PluginTypeEnum.Collect).ToList();
        _businessDevices = (_serviceScope.ServiceProvider.GetService<IDeviceService>().GetCacheList()).Where(a => a.PluginType == PluginTypeEnum.Business).ToList();
        _queryBusinessDevices = _businessDevices.OrderBy(a => a.Name.Length).Take(20).ToList();
        _queryDevices = _devices.OrderBy(a => a.Name.Length).Take(20).ToList();
        await base.OnParametersSetAsync();
    }

    private async Task OnWriteValueAsync(VariableRunTime tag, string value)
    {
        var data = await tag.SetValueToDeviceAsync(value, "DEFAULT");
        if (!data.IsSuccess)
        {
            await PopupService.EnqueueSnackbarAsync(data.ErrorMessage, AlertTypes.Warning);
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync(AppService.I18n.T("成功"), AlertTypes.Info);
        }
    }

    private async Task WriteAsync(VariableRunTime variableRunTime)
    {
        // 将异步方法添加到事件回调上
        var onWrite = EventCallback.Factory.Create<string>(this, value => OnWriteValueAsync(variableRunTime, value));
        var data = variableRunTime.Value?.ToString();
        await PopupService.OpenAsync(typeof(WriteValue), new Dictionary<string, object?>()
        {
            { nameof(WriteValue.Content), data=="0"?"":data },
            { nameof(WriteValue.OnSave), onWrite }
        });
    }
}
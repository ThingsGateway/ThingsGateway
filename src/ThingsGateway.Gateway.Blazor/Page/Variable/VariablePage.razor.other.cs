//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Masa.Blazor;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class VariablePage
{
    private List<Device> _devices = new();
    private List<Device> _businessDevices = new();

    [Inject]
    private IPluginService PluginService { get; set; }

    private ConcurrentDictionary<long, List<DependencyProperty>> _otherMethods = new();

    private void DeviceChanged(long devId)
    {
        if (devId > 0)
        {
            var data = WorkerUtil.GetWoker<CollectDeviceWorker>().GetDeviceMethodInfos(devId);
            _otherMethods.AddOrUpdate(devId, a => data, (a, b) => data);
        }
        else
            _otherMethods = new();
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

    private async Task CopyClickAsync(IEnumerable<Variable> variables)
    {
        if (!variables.Any())
        {
            await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
            return;
        }
        var input = await PopupService.PromptAsync(AppService.I18n.T("复制设备"), AppService.I18n.T("输入复制数量")
            , a => int.TryParse(a, out var result1) ? variables.Count() * result1 > 100000 ? "不支持大批量" : true : "填入数字");
        if (int.TryParse(input, out var result))
        {
            await _serviceScope.ServiceProvider.GetService<IVariableService>().CopyAsync(variables, result);
            await _datatable.QueryClickAsync();
            await PopupService.EnqueueSnackbarAsync(AppService.I18n.T("成功"), AlertTypes.Success);
            await MainLayout.StateHasChangedAsync();
        }
    }

    private List<DependencyProperty> GetVariableProperties(string pluginName, List<DependencyProperty> dependencyProperties)
    {
        return WorkerUtil.GetWoker<BusinessDeviceWorker>().GetVariablePropertys(pluginName, dependencyProperties);
    }

    private long _choiceUploadDeviceId;

    /// <summary>
    /// 刷新变量属性
    /// </summary>
    /// <param name="variable"></param>
    /// <returns></returns>
    private async Task RefreshClickAsync(Variable variable)
    {
        if (_choiceUploadDeviceId > 0)
        {
            var data = GetVariableProperties(_businessDevices.FirstOrDefault(a => a.Id == _choiceUploadDeviceId).PluginName, variable.VariablePropertys.ContainsKey(_choiceUploadDeviceId) ? variable.VariablePropertys[_choiceUploadDeviceId] : null);
            if (data == null)
            {
                return;
            }
            if (data.Count > 0)
            {
                variable.VariablePropertys.AddOrUpdate(_choiceUploadDeviceId, a => data.Adapt<List<DependencyProperty>>(), (a, b) => data);
            }
            else
            {
                variable.VariablePropertys.AddOrUpdate(_choiceUploadDeviceId, a => data, (a, b) => data);
            }
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync("需选择业务设备，再添加业务属性", AlertTypes.Warning);
        }
    }

    private async Task ImportClickAsync()
    {
        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => _serviceScope.ServiceProvider.GetService<IVariableService>().PreviewAsync(a));
        var import = EventCallback.Factory.Create<Dictionary<string, ImportPreviewOutputBase>>(this, value => _serviceScope.ServiceProvider.GetService<IVariableService>().ImportAsync(value));
        var data = (bool?)await PopupService.OpenAsync(typeof(ImportExcel), new Dictionary<string, object?>()
        {
            {nameof(ImportExcel.Import),import },
            {nameof(ImportExcel.Preview),preview },
        });
        if (data == true)
        {
            await _datatable.QueryClickAsync();
            await MainLayout.StateHasChangedAsync();
        }
    }

    private async Task DownExportAsync(bool isAll = false)
    {
        var query = _search?.Adapt<VariableInput>();
        query.All = isAll;
        await AppService.DownFileAsync("gatewayExport/variable", DateTime.Now.ToFileDateTimeFormat(), query);
    }
}
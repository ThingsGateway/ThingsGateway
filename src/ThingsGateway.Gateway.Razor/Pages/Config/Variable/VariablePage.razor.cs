//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using System.Data;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public partial class VariablePage : IDisposable
{
    protected IEnumerable<SelectedItem> CollectDeviceNames;
    protected IEnumerable<SelectedItem> BusinessDeviceNames;
    private Dictionary<long, Device> CollectDeviceDict { get; set; } = new();
    private Dictionary<long, Device> BusinessDeviceDict { get; set; } = new();

    private int TestCount { get; set; }

    [Inject]
    [NotNull]
    private IDeviceService? DeviceService { get; set; }

    [Inject]
    [NotNull]
    private IVariableService? VariableService { get; set; }

    [Inject]
    [NotNull]
    private IDispatchService<Device>? DeviceDispatchService { get; set; }

    private VariableSearchInput CustomerSearchModel { get; set; } = new VariableSearchInput();

    protected override Task OnInitializedAsync()
    {
        DeviceDispatchService.Subscribe(Notify);
        return base.OnInitializedAsync();
    }

    private async Task Notify(DispatchEntry<Device> entry)
    {
        await Change();
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        DeviceDispatchService.UnSubscribe(Notify);
    }

    protected override Task OnParametersSetAsync()
    {
        CollectDeviceDict = DeviceService.GetAll().Where(a => a.PluginType == PluginTypeEnum.Collect).ToDictionary(a => a.Id);
        BusinessDeviceDict = DeviceService.GetAll().Where(a => a.PluginType == PluginTypeEnum.Business).ToDictionary(a => a.Id);

        CollectDeviceNames = DeviceService.GetAll().Where(a => a.PluginType == PluginTypeEnum.Collect).BuildDeviceSelectList().Concat(new List<SelectedItem>() { new SelectedItem(string.Empty, "none") });
        BusinessDeviceNames = DeviceService.GetAll().Where(a => a.PluginType == PluginTypeEnum.Business).BuildDeviceSelectList().Concat(new List<SelectedItem>() { new SelectedItem(string.Empty, "none") });

        return base.OnParametersSetAsync();
    }

    #region 查询

    private async Task<QueryData<Variable>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await VariableService.PageAsync(options);
        return data;
    }

    #endregion 查询

    #region 修改

    private async Task DeleteAllAsync()
    {
        try
        {
            await VariableService.ClearVariableAsync();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
        }
    }

    private async Task<bool> Save(Variable variable, ItemChangedType itemChangedType)
    {
        try
        {
            variable.VariablePropertyModels ??= new();
            foreach (var item in variable.VariablePropertyModels)
            {
                var result = item.Value.ValidateForm?.Validate() != false;
                if (result == false)
                {
                    return false;
                }
            }

            variable.VariablePropertys = PluginServiceUtil.SetDict(variable.VariablePropertyModels);
            return await VariableService.SaveVariableAsync(variable, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    private async Task BatchEdit(IEnumerable<Variable> variables)
    {
        var op = new DialogOption()
        {
            Title = DefaultLocalizer["BatchEdit"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        var oldmodel = variables.FirstOrDefault();//默认值显示第一个
        var model = variables.FirstOrDefault().Adapt<Variable>();//默认值显示第一个
        op.Component = BootstrapDynamicComponent.CreateComponent<VariableEditComponent>(new Dictionary<string, object?>
        {
             {nameof(VariableEditComponent.OnValidSubmit), async () =>
            {
                await VariableService.BatchEditAsync(variables,oldmodel,model);

                await InvokeAsync(async ()=>
                {
        await table.QueryAsync();
        await Change();
                });
            }},
            {nameof(VariableEditComponent.Model),model },
            {nameof(VariableEditComponent.ValidateEnable),true },
            {nameof(VariableEditComponent.BatchEditEnable),true },
            {nameof(VariableEditComponent.BusinessDevices),BusinessDeviceNames },
            {nameof(VariableEditComponent.CollectDevices),CollectDeviceNames },
            {nameof(VariableEditComponent.BusinessDeviceDict),BusinessDeviceDict },
            {nameof(VariableEditComponent.CollectDeviceDict),CollectDeviceDict },
        });
        await DialogService.Show(op);
    }

    private async Task<bool> Delete(IEnumerable<Variable> devices)
    {
        try
        {
            return await VariableService.DeleteVariableAsync(devices.Select(a => a.Id));
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    #endregion 修改

    #region 导出

    [Inject]
    [NotNull]
    private ITableExport? TableExport { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<Variable> tableExportContext)
    {
        await using var ajaxJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
        string url = "api/gatewayExport/variable";
        string fileName = DateTime.Now.ToFileDateTimeFormat();
        var dtoObject = tableExportContext.BuildQueryPageOptions();
        await ajaxJS.InvokeVoidAsync("blazor_downloadFile", url, fileName, dtoObject);
        // 返回 true 时自动弹出提示框
        await ToastService.Default();
    }

    private async Task ExcelImportAsync(ITableExportContext<Variable> tableExportContext)
    {
        var op = new DialogOption()
        {
            Title = Localizer["ImportExcel"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => VariableService.PreviewAsync(a));
        Func<Dictionary<string, ImportPreviewOutputBase>, Task> import = (value => VariableService.ImportVariableAsync(value));
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
            {nameof(ImportExcel.Preview),preview },
        });
        await DialogService.Show(op);

        await Change();
        await table.QueryAsync();
    }

    #endregion 导出

    private async Task Change()
    {
        await OnParametersSetAsync();
    }
}

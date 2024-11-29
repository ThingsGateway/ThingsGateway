//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;
using ThingsGateway.Extension.Generic;
using ThingsGateway.Core.Json.Extension;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Razor;

public partial class VariableRuntimePage : IDisposable
{
    protected IEnumerable<SelectedItem> CollectDeviceNames;
    protected IEnumerable<SelectedItem> BusinessDeviceNames;

    public bool Disposed { get; set; }

    [Inject]
    [NotNull]
    private IDispatchService<DeviceRunTime>? DeviceDispatchService { get; set; }

    private VariableSearchInput? CustomerSearchModel { get; set; } = new();

    public void Dispose()
    {
        Disposed = true;
        DeviceDispatchService.UnSubscribe(Notify);
        GC.SuppressFinalize(this);
    }
    private List<long>? DataScope;
    [Inject]
    private ISysUserService SysUserService { get; set; }
    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }
    private ExecutionContext? context;
    protected override async Task OnInitializedAsync()
    {
        DataScope = await SysUserService.GetCurrentUserDataScopeAsync();
        context = ExecutionContext.Capture();
        DeviceDispatchService.Subscribe(Notify);
        await base.OnInitializedAsync();
    }
    private async Task Notify()
    {
        var current = ExecutionContext.Capture();
        try
        {
            ExecutionContext.Restore(context);
            await Change();
            await InvokeAsync(table.QueryAsync);
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            ExecutionContext.Restore(current);
        }
    }

    /// <summary>
    /// 设备
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery]
    public long? DeviceId { get; set; }
    /// <summary>
    /// 上传设备
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery]
    public long? BusinessDeviceId { get; set; }

    [CascadingParameter]
    [NotNull]
    private TabItem? TabItem { get; set; }
    protected override Task OnParametersSetAsync()
    {
        CollectDeviceNames = new List<SelectedItem>() { new SelectedItem(string.Empty, "All") }.Concat(GlobalData.ReadOnlyCollectDevices
                       .WhereIf(DataScope != null && DataScope?.Count > 0, u => DataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
         .WhereIf(DataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId)
            .Select(a => new SelectedItem(a.Value.Id.ToString(), a.Key)));
        BusinessDeviceNames = new List<SelectedItem>() { new SelectedItem(string.Empty, "All") }.Concat(GlobalData.ReadOnlyBusinessDevices
            .WhereIf(DataScope != null && DataScope?.Count > 0, u => DataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
         .WhereIf(DataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId).Select(a => new SelectedItem(a.Value.Id.ToString(), a.Key)));

        if (DeviceId != null)
            if (CustomerSearchModel.DeviceId != DeviceId)
                CustomerSearchModel.DeviceId = DeviceId;
        if (BusinessDeviceId != null)
            if (CustomerSearchModel.BusinessDeviceId != BusinessDeviceId)
                CustomerSearchModel.BusinessDeviceId = BusinessDeviceId;

        return base.OnParametersSetAsync();
    }
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            TabItem?.SetHeader(AppContext.TitleLocalizer["实时数据"]);
        return base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// IntFormatter
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    private static Task<string> JsonFormatter(object? d)
    {
        var ret = "";
        if (d is TableColumnContext<VariableRunTime, object?> data && data?.Value != null)
        {
            ret = data.Value.ToJsonNetString();
        }
        return Task.FromResult(ret);
    }

    private async Task Change()
    {
        await OnParametersSetAsync();
    }

    private async Task Notify(DispatchEntry<DeviceRunTime> entry)
    {
        await Notify();
    }

    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                await InvokeAsync(StateHasChanged);
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }

    #region 查询

    private Task<QueryData<VariableRunTime>> OnQueryAsync(QueryPageOptions options)
    {
        var data = GlobalData.ReadOnlyVariables.WhereIf(DataScope != null && DataScope?.Count > 0, u => DataScope.Contains(u.Value.CreateOrgId))//在指定机构列表查询
         .WhereIf(DataScope?.Count == 0, u => u.Value.CreateUserId == UserManager.UserId).Select(a => a.Value)
            .WhereIf(!options.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(options.SearchText))
            .WhereIf(!CustomerSearchModel.Name.IsNullOrWhiteSpace(), a => a.Name.Contains(CustomerSearchModel.Name))
            .WhereIf(!CustomerSearchModel.RegisterAddress.IsNullOrWhiteSpace(), a => a.RegisterAddress.Contains(CustomerSearchModel.RegisterAddress))
            .WhereIf(CustomerSearchModel.DeviceId > 0, a => a.DeviceId == CustomerSearchModel.DeviceId)
                 .WhereIf(CustomerSearchModel.BusinessDeviceId > 0, a => a.VariablePropertys?.ContainsKey(CustomerSearchModel.BusinessDeviceId.Value) == true)

            .GetQueryData(options);
        return Task.FromResult(data);
    }

    #endregion 查询

    #region 写入变量

    private string WriteValue { get; set; }

    private async Task OnWriteVariable(VariableRunTime variableRunTime)
    {
        try
        {
            var data = await variableRunTime.SetValueToDeviceAsync(WriteValue);
            if (!data.IsSuccess)
            {
                await ToastService.Warning(null, data.ErrorMessage);
            }
            else
            {
                await ToastService.Default();
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    #endregion 写入变量
}

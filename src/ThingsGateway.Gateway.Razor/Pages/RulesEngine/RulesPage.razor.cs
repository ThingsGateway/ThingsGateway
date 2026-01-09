//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait

using System.Data;

using ThingsGateway.Admin.Razor;

namespace ThingsGateway.Gateway.Razor;

[JSModuleAutoLoader("Pages/RulesEngine/RulesPage.razor.js", AutoInvokeDispose = false)]
public partial class RulesPage : ThingsGatewayModuleComponentBase
{
    [Inject]
    [NotNull]
    private IRulesPageService? RulesService { get; set; }


    protected override async Task InvokeInitAsync()
    {
        await InvokeVoidAsync("initJS", DiagramsJS, "./_content/ThingsGateway.Blazor.Diagrams/diagram.js");
        await InvokeVoidAsync("initCSS", DiagramsCSS, "./_content/ThingsGateway.Blazor.Diagrams/diagram.css");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        RulesDispatchService.UnSubscribe(Notify);
        context?.Dispose();
        if (Module != null)
        {
            await Module.InvokeVoidAsync("disposeJS", DiagramsJS);
            await Module.InvokeVoidAsync("disposeCSS", DiagramsCSS);
        }

        await base.DisposeAsync(disposing);
    }

    private string DiagramsCSS => $"{Id}DiagramsCSS";
    private string DiagramsJS => $"{Id}DiagramsJS";

    [Inject]
    ToastService ToastService { get; set; }

    [Inject]
    [NotNull]
    protected BlazorAppContext? AppContext { get; set; }
    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    public string RouteName => NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

    protected bool AuthorizeButton(string operate)
    {
        return AppContext.IsHasButtonWithRole(RouteName, operate);
    }

    #region 查询

    private async Task<QueryData<Rules>> OnQueryAsync(QueryPageOptions options)
    {
        return await Task.Run(async () =>
        {

            var data = await RulesService.RulesPageAsync(options);
            return data;
        });
    }

    #endregion 查询

    #region 修改

    private async Task<bool> Delete(IEnumerable<Rules> ruless)
    {
        try
        {
            return await Task.Run(async () =>
            {
                var result = await RulesService.DeleteRulesAsync(ruless.Select(a => a.Id).ToList());
                return result;
            });
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
            return false;
        }
    }

    private async Task<bool> Save(Rules rules, ItemChangedType itemChangedType)
    {
        try
        {
            var result = await RulesService.SaveRulesAsync(rules, itemChangedType);
            return result;
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
            return false;
        }
    }

    [Inject]
    private IStringLocalizer<ThingsGateway.Gateway.Razor._Imports> StringLocalizer { get; set; }

    #endregion 修改

    #region 清空

    private async Task ClearRulesAsync()
    {
        try
        {
            await Task.Run(async () =>
            {
                await RulesService.ClearRulesAsync();
                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await InvokeAsync(table.QueryAsync);
                });
            });
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () => await ToastService.Warn(ex));
        }
    }
    #endregion

    #region status

    [Inject]
    [NotNull]
    private IDispatchService<Rules>? RulesDispatchService { get; set; }

    private ExecutionContext? context;

    protected override async Task OnInitializedAsync()
    {
        context = ExecutionContext.Capture();
        RulesDispatchService.Subscribe(Notify);
        await base.OnInitializedAsync();
    }

    private async Task Notify()
    {
        var current = ExecutionContext.Capture();
        try
        {
            ExecutionContext.Restore(context);
            await InvokeAsync(async () =>
            {
                if (table != null)
                    await table.QueryAsync();

                StateHasChanged();
            });
        }
        finally
        {
            ExecutionContext.Restore(current);
        }

    }

    private async Task Notify(DispatchEntry<Rules> entry)
    {
        await Notify();
    }

    #endregion
}

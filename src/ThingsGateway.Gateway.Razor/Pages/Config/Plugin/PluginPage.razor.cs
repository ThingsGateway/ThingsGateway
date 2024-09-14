//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public partial class PluginPage
{
    #region 查询

    [Inject]
    [NotNull]
    private IPluginService? PluginService { get; set; }

    private PluginOutput SearchModel { get; set; } = new();

    private async Task<QueryData<PluginOutput>> OnQueryAsync(QueryPageOptions options)
    {
        return await Task.Run(() =>
        {
            var data = PluginService.Page(options);
            return data;
        });
    }

    #endregion 查询

    #region 导出

    [Inject]
    [NotNull]
    private ITableExport? TableExport { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<PluginOutput> context)
    {
        var ret = await TableExport.ExportExcelAsync(context.Rows, context.Columns, $"PluginOutput_{DateTime.Now:yyyyMMddHHmmss}.xlsx");

        // 返回 true 时自动弹出提示框
        await ToastService.Default(ret);
    }

    #endregion 导出

    #region 添加

    [Inject]
    private IStringLocalizer<PluginAddInput> PluginAddInputLoaclozer { get; set; }

    private async Task OnAdd(IEnumerable<PluginOutput> pluginOutputs)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            Title = PluginAddInputLoaclozer["SavePlugin"],
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () =>
            {
                await InvokeAsync(table.QueryAsync);
            },
            Size = Size.ExtraLarge
        };
        op.Component = BootstrapDynamicComponent.CreateComponent<SavePlugin>(new Dictionary<string, object?>
        {
            [nameof(SavePlugin.OnSavePlugin)] = new Func<PluginAddInput, Task>(PluginService.SavePlugin),
        });
        await DialogService.Show(op);
    }

    #endregion 添加
}

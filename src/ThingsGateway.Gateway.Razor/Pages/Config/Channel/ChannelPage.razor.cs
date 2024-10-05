//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using System.Data;

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class ChannelPage : IDisposable
{
    [Inject]
    [NotNull]
    private IChannelService? ChannelService { get; set; }

    [Inject]
    [NotNull]
    private IDispatchService<bool>? DispatchService { get; set; }

    private Channel? SearchModel { get; set; } = new();

    public void Dispose()
    {
        DispatchService.UnSubscribe(Notify);
    }

    protected override Task OnInitializedAsync()
    {
        DispatchService.Subscribe(Notify);
        return base.OnInitializedAsync();
    }

    private async Task Notify(DispatchEntry<bool> entry)
    {
        await InvokeAsync(table.QueryAsync);
        await InvokeAsync(StateHasChanged);
    }


    #region 查询

    private async Task<QueryData<Channel>> OnQueryAsync(QueryPageOptions options)
    {
        return await Task.Run(async () =>
        {
            var data = await ChannelService.PageAsync(options);
            return data;
        });
    }

    #endregion 查询

    #region 修改

    private async Task BatchEdit(IEnumerable<Channel> channels)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            Title = DefaultLocalizer["BatchEdit"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        var oldmodel = channels.FirstOrDefault();//默认值显示第一个
        var model = channels.FirstOrDefault().Adapt<Channel>();//默认值显示第一个
        op.Component = BootstrapDynamicComponent.CreateComponent<ChannelEditComponent>(new Dictionary<string, object?>
        {
             {nameof(ChannelEditComponent.OnValidSubmit), async () =>
            {
                await ChannelService.BatchEditAsync(channels,oldmodel,model);

                await InvokeAsync(async ()=>
                {
        await InvokeAsync(table.QueryAsync);
                });
            }},
            {nameof(ChannelEditComponent.Model),model },
            {nameof(ChannelEditComponent.ValidateEnable),true },
            {nameof(ChannelEditComponent.BatchEditEnable),true },
        });
        await DialogService.Show(op);
    }

    private async Task<bool> Delete(IEnumerable<Channel> channels)
    {
        try
        {
            return await Task.Run(async () =>
            {
                var result = await ChannelService.DeleteChannelAsync(channels.Select(a => a.Id));
                return result;
            });

        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
            return false;
        }
    }


    private async Task<bool> Save(Channel channel, ItemChangedType itemChangedType)
    {
        try
        {
            var result = await ChannelService.SaveChannelAsync(channel, itemChangedType);
            return result;
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
            return false;
        }
    }

    #endregion 修改

    #region 导出

    [Inject]
    [NotNull]
    private IGatewayExportService? GatewayExportService { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<Channel> tableExportContext)
    {
        await GatewayExportService.OnChannelExport(tableExportContext.BuildQueryPageOptions());
        // 返回 true 时自动弹出提示框
        await ToastService.Default();
    }

    private async Task ExcelImportAsync(ITableExportContext<Channel> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = false,
            Title = Localizer["ImportExcel"],
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () =>
            {
                await InvokeAsync(table.QueryAsync);
            },
            Size = Size.ExtraLarge
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => ChannelService.PreviewAsync(a));
        Func<Dictionary<string, ImportPreviewOutputBase>, Task> import = (value => ChannelService.ImportChannelAsync(value));
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
            {nameof(ImportExcel.Preview),preview },
        });
        await DialogService.Show(op);

        await InvokeAsync(table.QueryAsync);
    }

    #endregion 导出

    #region 清空

    private async Task ClearChannelAsync()
    {
        try
        {
            await Task.Run(async () =>
            {

                await ChannelService.ClearChannelAsync();
                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await InvokeAsync(table.QueryAsync);
                });
            });
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
        }

    }
    #endregion
}

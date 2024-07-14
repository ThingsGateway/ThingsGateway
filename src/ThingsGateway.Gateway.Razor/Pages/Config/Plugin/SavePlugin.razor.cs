//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public partial class SavePlugin
{
    private ButtonUpload<IBrowserFile> ButtonUpload;

    [Parameter]
    [NotNull]
    public Func<PluginAddInput, Task>? OnSavePlugin { get; set; }

    private PluginAddInput Model { get; set; } = new();

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    private async Task OnSave(EditContext editContext)
    {
        try
        {
            if (OnSavePlugin != null)
                await OnSavePlugin.Invoke(Model);
            if (OnCloseAsync != null)
                await OnCloseAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }
}

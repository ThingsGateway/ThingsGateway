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
namespace ThingsGateway.Management;

public partial class RedundancyOptionsHeader : IDisposable
{


    [Inject]
    [NotNull]
    private IStringLocalizer<RedundancyOptions>? RedundancyLocalizer { get; set; }

    private bool Disposed;
    public void Dispose()
    {
        Disposed = true;
    }
    protected override Task OnInitializedAsync()
    {
        _ = Task.Run(async () =>
        {
            while (!Disposed)
            {
                await InvokeAsync(StateHasChanged);
                await Task.Delay(3000);
            }
        });
        return base.OnInitializedAsync();
    }
}

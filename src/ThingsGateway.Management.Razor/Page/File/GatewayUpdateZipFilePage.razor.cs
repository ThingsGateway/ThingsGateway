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

namespace ThingsGateway.Management.Razor;

public partial class GatewayUpdateZipFilePage
{
    #region 查询

    [Inject]
    [NotNull]
    private IUpdateZipFileService? UpdateZipFileService { get; set; }
    [Inject]
    IManagementUpgradeRpcServer UpgradeRpcServer { get; set; }
    UpdateZipFileInput UpdateZipFileInput { get; set; }
    List<UpdateZipFile> UpdateZipFiles { get; set; } = new List<UpdateZipFile>();
    protected override async Task OnInitializedAsync()
    {
        UpdateZipFileInput = await UpgradeRpcServer.GetUpdateZipFileInputAsync();
        UpdateZipFiles = UpdateZipFileService.GetList(UpdateZipFileInput);
        await base.OnInitializedAsync();
    }

    #endregion 查询

    private async Task Upgrade(UpdateZipFile updateZipFile)
    {
        await ToastService.Success(title: "正在升级中，请稍后...", content: "详细可查看日志");
        await UpgradeRpcServer.UpgradeAsync(null, updateZipFile);

    }
}

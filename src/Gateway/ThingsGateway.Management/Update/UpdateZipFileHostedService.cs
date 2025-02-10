//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;

using ThingsGateway.Upgrade;

namespace ThingsGateway.Management;


internal sealed class UpdateZipFileHostedService : BackgroundService
{
    private INoticeService NoticeService { get; set; }
    private IUpdateZipFileService UpdateZipFileService { get; set; }
    private IVerificatInfoService VerificatInfoService { get; set; }
    public UpdateZipFileHostedService(INoticeService noticeService, IUpdateZipFileService updateZipFileService, IVerificatInfoService verificatInfoService)
    {
        NoticeService = noticeService;
        UpdateZipFileService = updateZipFileService;
        VerificatInfoService = verificatInfoService;
    }
    private Version LastVersion;
    private bool error;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var upgradeServerOptions = App.GetOptions<UpgradeServerOptions>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (upgradeServerOptions.Enable && !App.HostEnvironment.IsDevelopment())
                {
                    var data = await UpdateZipFileService.GetList().ConfigureAwait(false);
                    if (data.Count != 0 && (LastVersion == null || data.FirstOrDefault().Version > LastVersion))
                    {
                        LastVersion = data.FirstOrDefault().Version;
                        var verificatInfoIds = VerificatInfoService.GetListByUserId(RoleConst.SuperAdminId);
                        await NoticeService.NavigationMesage(verificatInfoIds.Where(a => a.Online).SelectMany(a => a.ClientIds), "/gateway/system?tab=1", App.CreateLocalizerByType(typeof(UpdateZipFileHostedService))["Update"]).ConfigureAwait(false);
                    }
                }
                error = false;
                await Task.Delay(60000, stoppingToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!error)
                    UpdateZipFileService.TextLogger.LogWarning(ex);
                error = true;
            }
        }
    }
}

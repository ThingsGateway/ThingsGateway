//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace ThingsGateway.Management;

[ApiDescriptionSettings("ThingsGateway.OpenApi", Order = 200)]
[Route("openApi/autoUpdate")]
[RolePermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class AutoUpdateControler : ControllerBase
{
    private IUpdateZipFileService _updateZipFileService;
    public AutoUpdateControler(IUpdateZipFileService updateZipFileService)
    {
        _updateZipFileService = updateZipFileService;
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    [HttpPost("update")]
    public async Task Update()
    {
        var data = await _updateZipFileService.GetList().ConfigureAwait(false);
        if (data.Count != 0)
            await _updateZipFileService.Update(data.OrderByDescending(a => a.Version).FirstOrDefault()).ConfigureAwait(false);
    }
}

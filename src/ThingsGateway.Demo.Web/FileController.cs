#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ThingsGateway.Demo.Web
{
    /// <summary>
    /// 文件下载
    /// </summary>
#if DEMO
#else

    [ApiDescriptionSettings(IgnoreApi = true)]
#endif

    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        /// <summary>
        /// 下载wwwroot文件夹下的文件
        /// </summary>
        /// <param name="fileName">相对路径</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");

            return File(fileStream, "application/octet-stream", (fileName.Replace('/', '_')));
        }
    }
}
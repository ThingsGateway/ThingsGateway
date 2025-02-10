//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace ThingsGateway.Upgrade;
public class UpdateZipFileAddInput1
{
    /// <summary>
    /// zip包
    /// </summary>
    [Required]
    public IFormFile ZipFile { get; set; }
    /// <summary>
    /// json
    /// </summary>
    [Required]
    public IFormFile JsonFile { get; set; }
}

public class UpdateZipFileAddInput
{
    /// <summary>
    /// zip包
    /// </summary>
    [Required]
    public IBrowserFile ZipFile { get; set; }
    /// <summary>
    /// json
    /// </summary>
    [Required]
    public IBrowserFile JsonFile { get; set; }
}
public class UpdateZipFileInput
{
    /// <summary>
    /// APP名称
    /// </summary>
    public string AppName { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// .net版本
    /// </summary>
    public Version DotNetVersion { get; set; }

    /// <summary>
    /// 系统版本
    /// </summary>
    public string OSPlatform { get; set; }

    public Architecture Architecture { get; set; }

}
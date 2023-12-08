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

using Furion.FriendlyException;

using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="IFileService"/>
/// </summary>
public class FileService : IFileService
{
    /// <inheritdoc/>
    public void ImportVerification(IBrowserFile file, int maxSzie = 300, string[] allowTypes = null)
    {
        if (file == null) throw Oops.Bah("文件不能为空");
        if (file.Size > maxSzie * 1024 * 1024) throw Oops.Bah($"文件大小不允许超过{maxSzie}M");
        var fileSuffix = Path.GetExtension(file.Name).ToLower().Split(".")[1]; // 文件后缀
        string[] allowTypeS = allowTypes ?? new string[] { "xlsx" };//允许上传的文件类型
        if (!allowTypeS.Contains(fileSuffix)) throw Oops.Bah(errorMessage: "文件格式错误");
    }
}
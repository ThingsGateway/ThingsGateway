using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Application;

/// <summary>
/// 文件管理服务
/// </summary>
public interface IFileService : ITransient
{

    /// <summary>
    /// 验证上传文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="maxSzie">最大体积(M)</param>
    /// <param name="allowTypes">允许上传类型</param>
    void ImportVerification(IBrowserFile file, int maxSzie = 30, string[] allowTypes = null);



}

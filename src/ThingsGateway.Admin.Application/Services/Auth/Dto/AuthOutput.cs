//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

public class OpenApiLoginOutput
{
    /// <summary>
    /// 令牌Token
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 刷新Token
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// 验证ID
    /// </summary>
    public long VerificatId { get; set; }
}

public class LoginOutput
{
    /// <summary>
    /// 令牌Token
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// 默认模块
    /// </summary>
    public long? DefaultModule { get; set; }

    /// <summary>
    /// 默认主页
    /// </summary>
    public string DefaultRazor { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 模块列表
    /// </summary>
    public IEnumerable<SysResource> ModuleList { get; set; } = Enumerable.Empty<SysResource>();

    /// <summary>
    /// 刷新Token
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// 验证ID
    /// </summary>
    public long VerificatId { get; set; }
}

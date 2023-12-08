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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// Cache常量
/// </summary>
public class CacheConst
{
    /// <summary>
    /// 登录验证码缓存Key
    /// </summary>
    public const string LOGIN_CAPTCHA = "LOGIN_CAPTCHA";

    /// <summary>
    /// 配置缓存Key
    /// </summary>
    public const string SYS_CONFIGCATEGORY = "SYS_CONFIGCATEGORY";

    #region OpenApi

    /// <summary>
    /// OpenApi用户表缓存Key
    /// </summary>
    public const string CACHE_OPENAPIUSER = "CACHE_OPENAPIUSER";

    /// <summary>
    /// OpenApi关系缓存Key
    /// </summary>
    public const string CACHE_OPENAPIUSERACCOUNT = "CACHE_OPENAPIUSERACCOUNT";

    /// <summary>
    /// UserVerificat缓存Key
    /// </summary>
    public const string CACHE_OPENAPIUSERVERIFICAT = "CACHE_OPENAPIUSERVERIFICAT";

    /// <summary>
    /// UserVerificat缓存Key
    /// </summary>
    public const string CACHE_USERVERIFICAT = "CACHE_USERVERIFICAT";

    #endregion OpenApi

    /// <summary>
    /// 用户表缓存Key
    /// </summary>
    public const string CACHE_SYSUSER = "CACHE_SYSUSER";

    /// <summary>
    /// 用户表缓存Key
    /// </summary>
    public const string CAHCE_SYSUSERACCOUNT = "CAHCE_SYSUSERACCOUNT";

    /// <summary>
    /// 关系表缓存Key
    /// </summary>
    public const string CACHE_SYSRELATION = "CACHE_SYSRELATION";

    /// <summary>
    /// 资源表缓存Key
    /// </summary>
    public const string CACHE_SYSRESOURCE = "CACHE_SYSRESOURCE";

    /// <summary>
    /// 角色表缓存Key
    /// </summary>
    public const string CACHE_SYSROLE = "CACHE_SYSROLE";
}
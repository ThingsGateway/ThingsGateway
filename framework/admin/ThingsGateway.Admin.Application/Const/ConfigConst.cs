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
/// 配置常量
/// </summary>
public static class ConfigConst
{
    /// <summary>
    /// 系统固定配置
    /// </summary>
    public const string SYS_CONFIGBASEDEFAULT = "SYS_CONFIGBASEDEFAULT";

    /// <summary>
    /// 其他自定义配置
    /// </summary>
    public const string SYS_CONFIGOTHER = "SYS_CONFIGOTHER";

    #region config

    /// <summary>
    /// 版权标识
    /// </summary>
    public const string CONFIG_COPYRIGHT = "CONFIG_COPYRIGHT";

    /// <summary>
    /// 版权跳转url
    /// </summary>
    public const string CONFIG_COPYRIGHT_URL = "CONFIG_COPYRIGHT_URL";

    /// <summary>
    /// 是否启用PageTab
    /// </summary>
    public const string CONFIG_PAGETAB = "CONFIG_PAGETAB";

    /// <summary>
    /// 登录验证码开关
    /// </summary>
    public const string CONFIG_CAPTCHA_OPEN = "CONFIG_CAPTCHA_OPEN";

    /// <summary>
    /// 默认用户密码
    /// </summary>
    public const string CONFIG_PASSWORD = "CONFIG_PASSWORD";

    /// <summary>
    /// 登录界面的介绍文本
    /// </summary>
    public const string CONFIG_REMARK = "CONFIG_REMARK";

    /// <summary>
    /// 单用户登录开关
    /// </summary>
    public const string CONFIG_SINGLE_OPEN = "CONFIG_SINGLE_OPEN";

    /// <summary>
    /// swagger用户
    /// </summary>
    public const string CONFIG_SWAGGER_NAME = "CONFIG_SWAGGER_NAME";

    /// <summary>
    /// swagger密码
    /// </summary>
    public const string CONFIG_SWAGGER_PASSWORD = "CONFIG_SWAGGER_PASSWORD";

    /// <summary>
    /// 系统标题
    /// </summary>
    public const string CONFIG_TITLE = "CONFIG_TITLE";

    /// <summary>
    /// 系统登录过期时间
    /// </summary>
    public const string CONFIG_VERIFICAT_EXPIRES = "CONFIG_VERIFICAT_EXPIRES";

    /// <summary>
    /// Swagger是否需要登录
    /// </summary>
    public const string CONFIG_SWAGGERLOGIN_OPEN = "CONFIG_SWAGGERLOGIN_OPEN";

    #endregion
}
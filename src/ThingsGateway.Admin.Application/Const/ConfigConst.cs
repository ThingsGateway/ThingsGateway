//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 系统配置常量
/// </summary>
public class ConfigConst
{
    /// <summary>
    /// 系统默认工作台
    /// </summary>
    public const string SYS_DEFAULT_WORKBENCH_DATA = "SYS_DEFAULT_WORKBENCH_DATA";

    /// <summary>
    /// 系统默认主页
    /// </summary>
    public const string SYS_DEFAULT_DEFAULT_RAZOR = "SYS_DEFAULT_DEFAULT_RAZOR";

    #region 登录策略

    /// <summary>
    /// 登录验证码开关
    /// </summary>
    public const string LOGIN_CAPTCHA_OPEN = "LOGIN_CAPTCHA_OPEN";

    /// <summary>
    /// 单用户登录开关
    /// </summary>
    public const string LOGIN_SINGLE_OPEN = "LOGIN_SINGLE_OPEN";

    /// <summary>
    ///  登录错误锁定时长
    /// </summary>
    public const string LOGIN_ERROR_LOCK = "LOGIN_ERROR_LOCK";

    /// <summary>
    ///  登录错误锁定时长
    /// </summary>
    public const string LOGIN_ERROR_RESET_TIME = "LOGIN_ERROR_RESET_TIME";

    /// <summary>
    /// 登录错误次数
    /// </summary>
    public const string LOGIN_ERROR_COUNT = "LOGIN_ERROR_COUNT";

    /// <summary>
    /// Verificat过期时间(分)
    /// </summary>
    public const string LOGIN_VERIFICAT_EXPIRES = "LOGIN_VERIFICAT_EXPIRES";

    #endregion 登录策略

    #region 密码策略

    /// <summary>
    /// 默认用户密码
    /// </summary>
    public const string PWD_DEFAULT_PASSWORD = "PWD_DEFAULT_PASSWORD";

    /// <summary>
    /// 密码最小长度
    /// </summary>
    public const string PWD_MIN_LENGTH = "PWD_MIN_LENGTH";

    /// <summary>
    /// 包含数字
    /// </summary>
    public const string PWD_CONTAIN_NUM = "PWD_CONTAIN_NUM";

    /// <summary>
    /// 包含小写字母
    /// </summary>
    public const string PWD_CONTAIN_LOWER = "PWD_CONTAIN_LOWER";

    /// <summary>
    /// 包含大写字母
    /// </summary>
    public const string PWD_CONTAIN_UPPER = "PWD_CONTAIN_UPPER";

    /// <summary>
    /// 包含特殊字符
    /// </summary>
    public const string PWD_CONTAIN_CHARACTER = "PWD_CONTAIN_CHARACTER";

    #endregion 密码策略
}
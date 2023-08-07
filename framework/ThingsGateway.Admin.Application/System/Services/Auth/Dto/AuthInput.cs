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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 登录输入参数
/// </summary>
public class LoginInput : ValidCodeInput
{
    /// <summary>
    /// 账号
    ///</summary>
    [Required(ErrorMessage = "账号不能为空"), MinLength(3, ErrorMessage = "账号不能少于4个字符")]
    public string Account { get; set; }

    /// <summary>
    /// 设备类型，默认PC
    /// </summary>
    /// <example>0</example>
    public AuthDeviceTypeEnum Device { get; set; } = AuthDeviceTypeEnum.PC;

    /// <summary>
    /// 密码
    ///</summary>
    [Required(ErrorMessage = "密码不能为空"), MinLength(3, ErrorMessage = "密码不能少于3个字符")]
    public string Password { get; set; }
}
/// <summary>
/// 验证码输入
/// </summary>
public class ValidCodeInput
{
    /// <summary>
    /// 验证码
    /// </summary>
    public string ValidCode { get; set; }

    /// <summary>
    /// 请求号
    /// </summary>
    public long ValidCodeReqNo { get; set; }
}
/// <summary>
/// 登录设备类型枚举
/// </summary>
public enum AuthDeviceTypeEnum
{
    /// <summary>
    /// PC端
    /// </summary>
    [Description("PC端")]
    PC,

    /// <summary>
    /// 移动端
    /// </summary>
    [Description("移动端")]
    APP,

    /// <summary>
    /// Api
    /// </summary>
    [Description("Api")]
    Api,
}
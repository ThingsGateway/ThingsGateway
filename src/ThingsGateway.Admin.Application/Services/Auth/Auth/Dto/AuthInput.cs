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
/// 权限认证输入
/// </summary>
public class AuthInput
{
}

/// <summary>
/// 验证码
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
/// 登录输入参数
/// </summary>
public class LoginInput : ValidCodeInput
{
    /// <summary>
    /// 账号
    ///</summary>
    /// <example>apiAdmin</example>
    [Required(ErrorMessage = "账号不能为空")]
    public string Account { get; set; }

    /// <summary>
    /// 密码
    ///</summary>
    ///<example>04F75DE291D453BC1B15DF350B4763FEA20B0E0EF4F9513ADD7E1923F92441F87488A1ADBF9862808916E2DFEEF828A0E3DCE24EE73BAC2EECB05C390C4E51A2F06D13EDEBE2DB30878C5D0EF757D68C37A5E203E7C20F87D1F27979B4A53C90C08AD7AB038C02</example>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; }

    /// <summary>
    /// 设备类型，默认PC
    /// </summary>
    /// <example>0</example>
    public AuthDeviceTypeEnum Device { get; set; } = AuthDeviceTypeEnum.PC;
}

/// <summary>
/// 登出输入参数
/// </summary>
public class LoginOutIput
{
    /// <summary>
    /// verificatId
    /// </summary>
    public long VerificatId { get; set; }
}
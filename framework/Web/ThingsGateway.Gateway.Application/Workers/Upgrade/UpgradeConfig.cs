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

namespace ThingsGateway.Core;

/// <summary>
/// UpgradeConfig
/// </summary>
public class UpgradeConfig
{
    /// <summary>
    /// 软件自动更新使能
    /// </summary>
    [Description("软件自动更新使能")]
    public bool FileEnable { get; set; }
    /// <summary>
    /// 配置自动更新使能
    /// </summary>
    [Description("配置自动更新使能")]
    public bool ConfigEnable { get; set; }

    /// <summary>
    /// 更新服务器Uri
    /// </summary>
    [Description("更新服务器Uri")]
    public string UpdateServerUri { get; set; }

    /// <summary>
    /// 验证Token
    /// </summary>
    [Description("验证Token")]
    public string VerifyToken { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }


    /// <summary>
    /// StartCommand
    /// </summary>
    public string[] StartCommand { get; set; }


}

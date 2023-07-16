#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 运行日志分页DTO
/// </summary>
public class BackendLogPageInput : BasePageInput
{
    /// <summary>
    /// 日志源
    /// </summary>
    [Description("日志源")]
    public string Source { get; set; }
    /// <summary>
    /// 日志等级
    /// </summary>
    [Description("日志等级")]
    public string Level { get; set; }

}
/// <summary>
/// RPC日志分页DTO
/// </summary>
public class RpcLogPageInput : BasePageInput
{
    /// <summary>
    /// 操作源
    /// </summary>
    [Description("操作源")]
    public string Source { get; set; }
    /// <summary>
    /// 操作源
    /// </summary>
    [Description("操作对象")]
    public string Object { get; set; }
    /// <summary>
    /// 方法
    /// </summary>
    [Description("方法")]
    public string Method { get; set; }
}
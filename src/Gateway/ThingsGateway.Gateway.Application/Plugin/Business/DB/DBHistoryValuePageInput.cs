//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

public class DBHistoryValuePageInput : BasePageInput
{
    /// <summary>
    /// 结束时间
    /// </summary>
    public virtual DateTime? EndTime { get; set; } = DateTime.Now.AddDays(1);

    /// <summary>
    /// 开始时间
    /// </summary>
    public virtual DateTime? StartTime { get; set; } = DateTime.Now.AddDays(-1);

    /// <summary>
    /// 变量名称
    /// </summary>
    public virtual string? VariableName { get; set; }

    /// <summary>
    /// 变量名称组
    /// </summary>
    public virtual string[]? VariableNames { get; set; }
}

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

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class VariableExpressionProperty : CollectPropertyBase
{
    /// <summary>
    /// 离线后恢复运行的间隔时间 /s，默认300s
    /// </summary>
    public override int ReIntervalTime { get; set; } = 30;

    /// <summary>
    /// 失败重试次数，默认3
    /// </summary>
    public override int RetryCount { get; set; } = 3;

    public override int ConcurrentCount { get; set; } = 1;
}
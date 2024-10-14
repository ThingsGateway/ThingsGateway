//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Logging;

using ThingsGateway.Extension;

namespace ThingsGateway.Gateway.Application;

public class BackendLogPageInput : ITableSearchModel
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public virtual LogLevel? LogLevel { get; set; }

    /// <summary>
    /// 日志源
    /// </summary>
    public string? LogSource { get; set; }

    /// <summary>
    /// 时间区间
    /// </summary>
    public DateTimeRangeValue? SearchDate { get; set; }

    /// <inheritdoc/>
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        ret.AddIF(!string.IsNullOrEmpty(LogSource), () => new SearchFilterAction(nameof(BackendLog.LogSource), LogSource));
        ret.AddIF(LogLevel != null, () => new SearchFilterAction(nameof(BackendLog.LogLevel), LogLevel!.Value, FilterAction.Equal));
        ret.AddIF(SearchDate != null, () => new SearchFilterAction(nameof(BackendLog.LogTime), SearchDate!.Start, FilterAction.GreaterThanOrEqual));
        ret.AddIF(SearchDate != null, () => new SearchFilterAction(nameof(BackendLog.LogTime), SearchDate!.End, FilterAction.LessThanOrEqual));
        return ret;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        SearchDate = null;
        LogLevel = null;
        LogSource = null;
    }
}

public class RpcLogPageInput : ITableSearchModel
{
    /// <summary>
    /// 操作对象
    /// </summary>
    public string? OperateObject { get; set; }

    /// <summary>
    /// 操作源
    /// </summary>
    public string? OperateSource { get; set; }

    /// <summary>
    /// 时间区间
    /// </summary>
    public DateTimeRangeValue? SearchDate { get; set; }

    /// <inheritdoc/>
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        ret.AddIF(!string.IsNullOrEmpty(OperateSource), () => new SearchFilterAction(nameof(RpcLog.OperateSource), OperateSource));
        ret.AddIF(!string.IsNullOrEmpty(OperateObject), () => new SearchFilterAction(nameof(RpcLog.OperateObject), OperateObject));
        ret.AddIF(SearchDate != null, () => new SearchFilterAction(nameof(RpcLog.LogTime), SearchDate!.Start, FilterAction.GreaterThanOrEqual));
        ret.AddIF(SearchDate != null, () => new SearchFilterAction(nameof(RpcLog.LogTime), SearchDate!.End, FilterAction.LessThanOrEqual));
        return ret;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        SearchDate = null;
        OperateObject = null;
        OperateSource = null;
    }
}

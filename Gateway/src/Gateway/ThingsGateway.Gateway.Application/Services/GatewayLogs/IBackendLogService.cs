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

namespace ThingsGateway.Gateway.Application;

public interface IBackendLogService
{
    /// <summary>
    /// 删除 BackendLog 表中的所有记录
    /// </summary>
    /// <remarks>
    /// 调用此方法会删除 BackendLog 表中的所有记录。
    /// </remarks>
    Task DeleteBackendLogAsync();

    /// <summary>
    /// 获取最新的十条 BackendLog 记录
    /// </summary>
    /// <returns>最新的十条记录</returns>
    Task<List<BackendLog>> GetNewLog();

    /// <summary>
    /// 分页查询 BackendLog 数据
    /// </summary>
    /// <param name="option">查询选项</param>
    /// <returns>查询到的数据</returns>
    Task<QueryData<BackendLog>> PageAsync(QueryPageOptions option);

    /// <summary>
    /// 获取最近一段时间内每天的后端日志统计信息
    /// </summary>
    /// <param name="day">要统计的天数</param>
    /// <returns>按天统计的后端日志信息列表</returns>
    Task<List<BackendLogDayStatisticsOutput>> StatisticsByDayAsync(int day);
}

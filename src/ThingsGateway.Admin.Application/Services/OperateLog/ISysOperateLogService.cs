//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 操作日志服务接口
/// </summary>
public interface ISysOperateLogService
{
    /// <summary>
    /// 删除指定分类的操作日志
    /// </summary>
    /// <param name="category">日志分类</param>
    Task DeleteAsync(LogCateGoryEnum category);

    /// <summary>
    /// 获取最新的十条日志
    /// </summary>
    /// <param name="account">操作人账号</param>
    Task<List<OperateLogIndexOutput>> GetNewLog(string account);

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="option">查询条件</param>
    Task<QueryData<SysOperateLog>> PageAsync(QueryPageOptions option);

    /// <summary>
    /// 根据天数统计操作日志信息
    /// </summary>
    /// <param name="day">天数</param>
    /// <returns>操作日志统计信息列表</returns>
    Task<List<OperateLogDayStatisticsOutput>> StatisticsByDayAsync(int day);
}

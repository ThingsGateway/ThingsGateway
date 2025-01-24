//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
[SugarTable("his_hardwareinfo", TableDescription = "硬件信息历史表")]
[Tenant(SqlSugarConst.DB_HardwareInfo)]
public class HistoryHardwareInfo
{
    /// <inheritdoc/>
    [SugarColumn(ColumnDescription = "磁盘使用率")]
    public string DriveUsage { get; set; }

    /// <inheritdoc/>
    [SugarColumn(ColumnDescription = "内存使用率")]
    public string MemoryUsage { get; set; }

    /// <inheritdoc/>
    [SugarColumn(ColumnDescription = "CPU使用率")]
    public string CpuUsage { get; set; }

    /// <inheritdoc/>
    [SugarColumn(ColumnDescription = "温度")]
    public string Temperature { get; set; }

    /// <inheritdoc/>
    [SugarColumn(ColumnDescription = "电池")]
    public string Battery { get; set; }

    /// <inheritdoc/>
    [SugarColumn(ColumnDescription = "时间")]
    public DateTime Date { get; set; }
}

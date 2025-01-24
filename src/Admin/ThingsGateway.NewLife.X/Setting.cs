// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.ComponentModel;

using ThingsGateway.NewLife.Log;

//[assembly: InternalsVisibleTo("XUnitTest.Core")]

namespace ThingsGateway.NewLife;

/// <summary>核心设置</summary>
/// <remarks>
/// 文档 https://newlifex.com/core/setting
/// </remarks>
[DisplayName("核心设置")]
public class Setting
{
    /// <summary>当前实例。</summary>
    public static Setting Current = new();

    #region 属性
    /// <summary>是否启用全局调试。默认启用</summary>
    [Description("全局调试。XTrace.Debug")]
    public Boolean Debug { get; set; } = true;

    /// <summary>日志等级，只输出大于等于该级别的日志，All/Debug/Info/Warn/Error/Fatal，默认Info</summary>
    [Description("日志等级。只输出大于等于该级别的日志，All/Debug/Info/Warn/Error/Fatal，默认Info")]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;

    /// <summary>文件日志目录。默认Log子目录</summary>
    [Description("文件日志目录。默认Log子目录")]
    public String LogPath { get; set; } = "Logs/XLog";

    /// <summary>日志文件上限。超过上限后拆分新日志文件，默认10MB，0表示不限制大小</summary>
    [Description("日志文件上限。超过上限后拆分新日志文件，默认10MB，0表示不限制大小")]
    public Int32 LogFileMaxBytes { get; set; } = 10;

    /// <summary>日志文件备份。超过备份数后，最旧的文件将被删除，网络安全法要求至少保存6个月日志，默认200，0表示不限制个数</summary>
    [Description("日志文件备份。超过备份数后，最旧的文件将被删除，网络安全法要求至少保存6个月日志，默认200，0表示不限制个数")]
    public Int32 LogFileBackups { get; set; } = 200;

    /// <summary>日志文件格式。默认{0:yyyy_MM_dd}.log，支持日志等级如 {1}_{0:yyyy_MM_dd}.log</summary>
    [Description("日志文件格式。默认{0:yyyy_MM_dd}.log，支持日志等级如 {1}_{0:yyyy_MM_dd}.log")]
    public String LogFileFormat { get; set; } = "{0:yyyy_MM_dd}.log";

    /// <summary>日志记录时间UTC校正，单位：小时。默认0表示使用的是本地时间，使用UTC时间的系统转换成本地时间则相差8小时</summary>
    [Description("日志记录时间UTC校正，小时")]
    public Int32 UtcIntervalHours { get; set; } = 0;

    #endregion

}

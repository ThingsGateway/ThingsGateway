// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业调度器操作结果
/// </summary>
[SuppressSniffer]
public enum ScheduleResult
{
    /// <summary>
    /// 不存在
    /// </summary>
    NotFound = 0,

    /// <summary>
    /// 未指定作业 Id
    /// </summary>
    NotIdentify = 1,

    /// <summary>
    /// 已存在
    /// </summary>
    Exists = 2,

    /// <summary>
    /// 成功
    /// </summary>
    Succeed = 3,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 4
}
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

using System.Text.Json.Serialization;

namespace ThingsGateway.Schedule;

/// <summary>
/// 配置作业信息特性
/// </summary>
/// <remarks>仅限 <see cref="IJob"/> 实现类使用</remarks>
[SuppressSniffer, AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class JobDetailAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    public JobDetailAttribute(string jobId)
    {
        JobId = jobId;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="description">作业描述</param>
    public JobDetailAttribute(string jobId, string description)
        : this(jobId)
    {
        Description = description;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    /// <param name="concurrent">并行/串行</param>
    public JobDetailAttribute(string jobId, bool concurrent)
        : this(jobId)
    {
        Concurrent = concurrent;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobId">作业 Id</param>
    ///  <param name="concurrent">并行/串行</param>
    /// <param name="description">作业描述</param>
    public JobDetailAttribute(string jobId, bool concurrent, string description)
        : this(jobId, concurrent)
    {
        Description = description;
    }

    /// <summary>
    /// 作业 Id
    /// </summary>
    [JsonInclude]
    public string JobId { get; set; }

    /// <summary>
    /// 作业组名称
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 是否采用并行执行
    /// </summary>
    /// <remarks>如果设置为 false，那么使用串行执行</remarks>
    public bool Concurrent { get; set; } = true;
}
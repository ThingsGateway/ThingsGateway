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

using System.Diagnostics.CodeAnalysis;

namespace ThingsGateway.Schedule;

/// <summary>
/// 支持重复 Key 的字典比较器
/// </summary>
internal sealed class RepeatKeyEqualityComparer : IEqualityComparer<JobDetail>
{
    /// <summary>
    /// 相等比较
    /// </summary>
    /// <param name="x"><see cref="JobDetail"/></param>
    /// <param name="y"><see cref="JobDetail"/></param>
    /// <returns><see cref="bool"/></returns>
    public bool Equals(JobDetail x, JobDetail y)
    {
        return x != y;
    }

    /// <summary>
    /// 获取哈希值
    /// </summary>
    /// <param name="obj"><see cref="JobDetail"/></param>
    /// <returns><see cref="int"/></returns>
    public int GetHashCode([DisallowNull] JobDetail obj)
    {
        return obj.GetHashCode();
    }
}
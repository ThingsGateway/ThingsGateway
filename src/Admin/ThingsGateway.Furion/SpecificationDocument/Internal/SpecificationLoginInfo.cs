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

namespace ThingsGateway.SpecificationDocument;

/// <summary>
/// 规范化文档授权登录配置信息
/// </summary>
[SuppressSniffer]
public sealed class SpecificationLoginInfo
{
    /// <summary>
    /// 是否启用授权控制
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 检查登录地址
    /// </summary>
    public string CheckUrl { get; set; }

    /// <summary>
    /// 提交登录地址
    /// </summary>
    public string SubmitUrl { get; set; }

    /// <summary>
    /// 生产环境自动开启
    /// </summary>
    public bool EnableOnProduction { get; set; }
}
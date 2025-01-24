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
/// 配置规范化文档 OperationId 问题
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OperationIdAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="operationId">自定义 OperationId，可用户生成可读的前端代码</param>
    public OperationIdAttribute(string operationId)
    {
        OperationId = operationId;
    }

    /// <summary>
    /// 自定义 OperationId
    /// </summary>
    public string OperationId { get; set; }
}
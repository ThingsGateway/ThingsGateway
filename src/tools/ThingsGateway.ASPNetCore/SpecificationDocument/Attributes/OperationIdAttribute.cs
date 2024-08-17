// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://kimdiego2098.github.io/
// QQ群：605534569
// ------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

namespace ThingsGateway;

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

// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

namespace ThingsGateway;

/// <summary>
/// 解决规范化文档 SchemaId 冲突问题
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SchemaIdAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="schemaId">自定义 SchemaId，只能是字母开头，只运行下划线_连接</param>
    public SchemaIdAttribute(string schemaId)
    {
        SchemaId = schemaId;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="schemaId">自定义 SchemaId</param>
    /// <param name="replace">默认在头部叠加，设置 true 之后，将直接使用 <see cref="SchemaId"/></param>
    public SchemaIdAttribute(string schemaId, bool replace)
    {
        SchemaId = schemaId;
        Replace = replace;
    }

    /// <summary>
    /// 自定义 SchemaId
    /// </summary>
    public string SchemaId { get; set; }

    /// <summary>
    /// 完全覆盖
    /// </summary>
    /// <remarks>默认在头部叠加，设置 true 之后，将直接使用 <see cref="SchemaId"/></remarks>
    public bool Replace { get; set; } = false;
}

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
/// 用于控制 Swager 生成 Enum 类型
/// </summary>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
public sealed class EnumToNumberAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public EnumToNumberAttribute()
        : this(true)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="enabled">启用状态</param>
    public EnumToNumberAttribute(bool enabled = true)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// 启用状态
    /// </summary>
    /// <remarks>设置 false 则使用字符串类型</remarks>
    public bool Enabled { get; set; }
}

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

namespace ThingsGateway.Logging;

/// <summary>
/// LoggingMonitor 序列化属性命名规则选项
/// </summary>
public enum ContractResolverTypes
{
    /// <summary>
    /// CamelCase 小驼峰
    /// </summary>
    /// <remarks>默认值</remarks>
    CamelCase = 0,

    /// <summary>
    /// 保持原样
    /// </summary>
    Default = 1
}

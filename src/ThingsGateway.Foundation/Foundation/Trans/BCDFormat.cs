#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation;

/// <summary>
/// BCD格式化值
/// </summary>
public enum BcdFormat
{
    /// <inheritdoc/>
    C8421 = 1,
    /// <inheritdoc/>
    C5421 = 2,
    /// <inheritdoc/>
    C2421 = 3,
    /// <inheritdoc/>
    C3 = 4,
    /// <inheritdoc/>
    Gray = 5,
}
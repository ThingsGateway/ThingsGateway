//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Cache;

/// <summary>
/// 常量
/// </summary>
public class CacheStringConst
{
    /// <summary>
    /// Name为{0}的连接已存在
    /// </summary>
    public const string ConnectExistError = "The connection named {0} already exists.";

    /// <summary>
    /// Name为{0}的连接不存在
    /// </summary>
    public const string ConnectNameNullError = "The connection named {0} does not exist.";

    /// <summary>
    /// Redis服务未启用，请开启该服务
    /// </summary>
    public const string RedisNotStarted = "Redis service is not started, please start the service.";
}
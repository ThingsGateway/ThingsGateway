#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

namespace ThingsGateway.Cache;

/// <summary>
/// Redis常量
/// </summary>
public class CacheConst
{
    /// <summary>
    /// Redis Key前缀(可删除)
    /// </summary>
    public const string Cache_Prefix_Admin = "ThingsGatewayAdmin:";

    /// <summary>
    /// Redis Key前缀(需要持久化，不随系统重启删除)
    /// </summary>
    public const string Cache_Prefix = "ThingsGateway:";

    /// <summary>
    /// Redis Hash类型
    /// </summary>
    public const string Cache_Hash = "Hash";

    /// <summary>
    /// 用户Token缓存Key
    /// </summary>
    public const string Cache_UserToken = Cache_Prefix + "UserToken";
}
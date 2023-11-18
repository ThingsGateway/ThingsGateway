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

using SqlSugar;

namespace ThingsGateway.Plugin.TDengineDB;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class TDengineDBProperty : UploadPropertyWithCache
{

    public DbType DbType { get; set; } = DbType.TDengine;
    [DeviceProperty("链接字符串", "")] public string BigTextConnectStr { get; set; } = "Host=localhost;Port=6030;Username=root;Password=taosdata;Database=test";

    /// <summary>
    /// 每次发送时合并的缓存值数量
    /// </summary>
    public virtual int CacheSendCount { get; set; } = 2000;
    /// <summary>
    /// 每次添加缓存时，合并的变量值数量
    /// </summary>
    public virtual int CacheItemCount { get; set; } = 2000;

}

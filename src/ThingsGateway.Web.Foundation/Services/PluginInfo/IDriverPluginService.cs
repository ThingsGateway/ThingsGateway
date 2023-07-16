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

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 插件服务
/// </summary>
public interface IDriverPluginService : ITransient
{
    /// <summary>
    /// 添加/更新插件
    /// </summary>
    Task AddAsync(DriverPluginAddInput input);
    /// <summary>
    /// 获取缓存
    /// </summary>
    List<DriverPlugin> GetCacheList();
    /// <summary>
    /// 根据ID获取插件信息
    /// </summary>
    DriverPlugin GetDriverPluginById(long Id);
    /// <summary>
    /// 根据分类获取插件树
    /// </summary>
    List<DriverPluginCategory> GetDriverPluginChildrenList(DriverEnum? driverTypeEnum = null);

    /// <summary>
    /// 根据ID获取名称
    /// </summary>
    long? GetIdByName(string name);
    /// <summary>
    /// 根据名称获取ID
    /// </summary>
    string GetNameById(long id);
    /// <summary>
    /// 分页
    /// </summary>
    Task<SqlSugarPagedList<DriverPlugin>> PageAsync(DriverPluginPageInput input);
}
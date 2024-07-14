//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 系统字典服务接口，提供对系统字典的操作
/// </summary>
public interface ISysDictService
{
    /// <summary>
    /// 删除业务配置
    /// </summary>
    /// <param name="ids">待删除配置项的ID列表</param>
    /// <returns>是否成功删除</returns>
    Task<bool> DeleteDictAsync(IEnumerable<long> ids);

    /// <summary>
    /// 修改登录策略
    /// </summary>
    /// <param name="input">登录策略</param>
    Task EditLoginPolicyAsync(LoginPolicy input);

    /// <summary>
    /// 修改页面策略
    /// </summary>
    /// <param name="input">页面策略</param>
    Task EditPagePolicyAsync(PagePolicy input);

    /// <summary>
    /// 修改密码策略
    /// </summary>
    /// <param name="input">密码策略</param>
    Task EditPasswordPolicyAsync(PasswordPolicy input);

    /// <summary>
    /// 修改网站设置
    /// </summary>
    /// <param name="input">网站设置</param>
    Task EditWebsitePolicyAsync(WebsitePolicy input);

    /// <summary>
    /// 获取系统配置
    /// </summary>
    /// <returns>系统配置信息</returns>
    Task<AppConfig> GetAppConfigAsync();

    /// <summary>
    /// 根据分类和名称获取系统字典项
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="name">名称</param>
    /// <returns>系统字典项</returns>
    Task<SysDict> GetByKeyAsync(string category, string name);

    /// <summary>
    /// 从缓存/数据库获取系统配置列表
    /// </summary>
    /// <returns>系统配置列表</returns>
    Task<List<SysDict>> GetSystemConfigAsync();

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="option">查询选项</param>
    /// <returns>查询结果</returns>
    Task<QueryData<SysDict>> PageAsync(QueryPageOptions option);

    /// <summary>
    /// 修改业务配置
    /// </summary>
    /// <param name="input">配置项</param>
    /// <param name="type">保存类型</param>
    /// <returns>是否成功保存</returns>
    Task<bool> SaveDictAsync(SysDict input, ItemChangedType type);
}

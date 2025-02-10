//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.RulesEngine;


public interface IRulesService
{

    /// <summary>
    /// 清除所有规则
    /// </summary>
    Task ClearRulesAsync();

    /// <summary>
    /// 删除规则
    /// </summary>
    /// <param name="ids">待删除规则的ID列表</param>
    Task<bool> DeleteRulesAsync(IEnumerable<long> ids);

    /// <summary>
    /// 从缓存中删除规则
    /// </summary>
    void DeleteRulesFromCache();

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>规则列表</returns>
    List<Rules> GetAll();

    /// <summary>
    /// 通过ID获取规则
    /// </summary>
    /// <param name="id">规则ID</param>
    /// <returns>规则对象</returns>
    Rules? GetRulesById(long id);

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="filterKeyValueAction">查询条件</param>
    Task<QueryData<Rules>> PageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null);

    /// <summary>
    /// 保存规则
    /// </summary>
    /// <param name="input">规则对象</param>
    /// <param name="type">保存类型</param>
    Task<bool> SaveRulesAsync(Rules input, ItemChangedType type);
}

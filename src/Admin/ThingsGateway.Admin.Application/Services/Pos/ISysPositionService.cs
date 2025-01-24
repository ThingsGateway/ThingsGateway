// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 职位服务
/// </summary>
public interface ISysPositionService
{
    #region 查询

    /// <summary>
    /// 获取职位列表
    /// </summary>
    /// <returns>职位列表</returns>
    Task<List<SysPosition>> GetAllAsync(bool showDisabled = true);
    /// <summary>
    /// 保存岗位
    /// </summary>
    /// <param name="input">机构</param>
    /// <param name="type">保存类型</param>
    Task<bool> SavePositionAsync(SysPosition input, ItemChangedType type);

    /// <summary>
    /// 删除岗位
    /// </summary>
    /// <param name="ids">id列表</param>
    Task<bool> DeletePositionAsync(IEnumerable<long> ids);

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="queryFunc">额外条件</param>
    Task<QueryData<SysPosition>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<SysPosition>, ISugarQueryable<SysPosition>>? queryFunc = null);

    /// <summary>
    /// 获取职位信息
    /// </summary>
    /// <param name="id">职位ID</param>
    /// <returns>职位信息</returns>
    Task<SysPosition> GetSysPositionById(long id);

    /// <summary>
    /// 职位树形结构
    /// </summary>
    /// <returns></returns>
    Task<List<PositionTreeOutput>> TreeAsync();

    /// <summary>
    /// 职位选择器
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns></returns>
    Task<List<PositionSelectorOutput>> SelectorAsync(PositionSelectorInput input);



    #endregion

}

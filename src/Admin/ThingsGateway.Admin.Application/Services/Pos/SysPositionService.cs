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

using ThingsGateway.FriendlyException;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 职位服务
/// </summary>
public class SysPositionService : BaseService<SysPosition>, ISysPositionService
{
    private ISysUserService _sysUserService;
    private ISysUserService SysUserService
    {
        get
        {
            if (_sysUserService == null)
            {
                _sysUserService = App.GetService<ISysUserService>();
            }
            return _sysUserService;
        }
    }
    private readonly ISysOrgService _sysOrgService;
    private IDispatchService<SysPosition> _dispatchService;

    public SysPositionService(ISysOrgService sysOrgService, IDispatchService<SysPosition> dispatchService)
    {
        _sysOrgService = sysOrgService;
        _dispatchService = dispatchService;
    }

    public async Task<List<SysPosition>> GetAllAsync(bool showDisabled = true)
    {
        var key = $"{CacheConst.Cache_SysPosition}";//系统配置key
        var sysPositions = App.CacheService.Get<List<SysPosition>>(key);
        if (sysPositions == null)
        {
            using var db = GetDB();
            sysPositions = (await db.Queryable<SysPosition>().ToListAsync().ConfigureAwait(false));
            App.CacheService.Set(key, sysPositions);
        }
        if (!showDisabled)
        {
            sysPositions = sysPositions.Where(it => it.Status).ToList();
        }
        return sysPositions;
    }

    public async Task<SysPosition> GetSysPositionById(long id)
    {
        var list = await GetAllAsync().ConfigureAwait(false);
        return list.FirstOrDefault(x => x.Id == id);
    }

    [OperDesc("DeletePosition")]
    public async Task<bool> DeletePositionAsync(IEnumerable<long> ids)
    {
        //获取所有ID
        if (ids.Any())
        {
            using var db = GetDB();
            //如果组织下有用户则不能删除
            if (await db.Queryable<SysUser>().AnyAsync(it => ids.Contains(it.PositionId.Value)).ConfigureAwait(false))
            {
                throw Oops.Bah(Localizer["DeleteUserFirst"]);
            }

            var dels = (await GetAllAsync().ConfigureAwait(false)).Where(a => ids.Contains(a.Id));
            await SysUserService.CheckApiDataScopeAsync(dels.Select(a => a.OrgId).ToList(), dels.Select(a => a.CreateUserId).ToList()).ConfigureAwait(false);
            //删除职位
            var result = await base.DeleteAsync(ids).ConfigureAwait(false);
            if (result)
                RefreshCache();
            return result;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// 表格查询
    /// </summary>
    public async Task<QueryData<SysPosition>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<SysPosition>, ISugarQueryable<SysPosition>>? queryFunc = null)
    {
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false); //获取机构ID范围
        queryFunc += a => a
                    .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId);

        return await QueryAsync(option, queryFunc).ConfigureAwait(false);
    }

    [OperDesc("SavePosition")]
    public async Task<bool> SavePositionAsync(SysPosition input, ItemChangedType type)
    {
        await CheckInput(input).ConfigureAwait(false);//检查参数
        if (type == ItemChangedType.Update)
            await SysUserService.CheckApiDataScopeAsync(input.OrgId, input.CreateUserId).ConfigureAwait(false);

        var reuslt = await base.SaveAsync(input, type).ConfigureAwait(false);
        if (reuslt)
            RefreshCache();

        return reuslt;
    }

    /// <summary>
    /// 刷新缓存
    /// </summary>
    /// <returns></returns>
    private void RefreshCache()
    {
        App.CacheService.Remove($"{CacheConst.Cache_SysPosition}");
        _dispatchService.Dispatch(null);
    }

    /// <inheritdoc/>
    public async Task<List<PositionTreeOutput>> TreeAsync()
    {
        var result = new List<PositionTreeOutput>();//返回结果
        var sysOrgList = await _sysOrgService.GetAllAsync(false).ConfigureAwait(false);//获取所有组织
        var sysPositions = await GetAllAsync().ConfigureAwait(false);//获取所有职位
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        sysOrgList = sysOrgList
           .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.Id))
           .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
           .ToList();//在指定组织列表查询
        sysPositions = sysPositions
            .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .ToList();//在指定职位列表查询

        var posCategory = typeof(PositionCategoryEnum).GetEnumNames();//获取职位分类
        var topOrgList = sysOrgList.Where(it => it.ParentId == 0).ToList();//获取顶级组织
        //遍历顶级组织
        foreach (var org in topOrgList)
        {
            var childIds = await _sysOrgService.GetOrgChildIdsAsync(org.Id, true, sysOrgList).ConfigureAwait(false);//获取组织下的所有子级ID
            var orgPositions = sysPositions.Where(it => childIds.Contains(it.OrgId)).ToList();//获取组织下的职位
            if (orgPositions.Count == 0) continue;
            var positionTreeOutput = new PositionTreeOutput
            {
                Id = org.Id,
                Name = org.Name,
                IsPosition = false
            };//实例化组织树
            //获取组织下的职位职位分类
            foreach (var category in posCategory)
            {
                var id = CommonUtils.GetSingleId();//生成唯一ID临时用,因为前端需要ID
                var categoryTreeOutput = new PositionTreeOutput
                {
                    Id = id,
                    Name = category,
                    IsPosition = false
                };//实例化职位分类树
                var positions = orgPositions.Where(it => it.Category.ToString() == category).ToList();//获取职位分类下的职位
                //遍历职位，实例化职位树
                positions.ForEach(it =>
                {
                    categoryTreeOutput.Children.Add(new PositionTreeOutput()
                    {
                        Id = it.Id,
                        Name = it.Name,
                        IsPosition = true
                    });//添加职位
                });
                positionTreeOutput.Children.Add(categoryTreeOutput);
            }
            result.Add(positionTreeOutput);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<List<PositionSelectorOutput>> SelectorAsync(PositionSelectorInput input)
    {
        var sysOrgList = await _sysOrgService.GetAllAsync(false).ConfigureAwait(false);//获取所有组织
        var sysPositions = await GetAllAsync().ConfigureAwait(false);//获取所有职位
        var dataScope = await SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        sysOrgList = sysOrgList
           .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.Id))
           .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
           .ToList();//在指定组织列表查询
        sysPositions = sysPositions
            .WhereIF(dataScope != null && dataScope?.Count > 0, b => dataScope.Contains(b.OrgId))
            .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .ToList();//在指定职位列表查询

        var result = await ConstructPositionSelector(sysOrgList, sysPositions).ConfigureAwait(false);//构造树
        return result;
    }

    /// <summary>
    /// 构建职位选择器
    /// </summary>
    /// <param name="orgList">组织列表</param>
    /// <param name="sysPositions">职位列表</param>
    /// <param name="parentId">父Id</param>
    /// <returns></returns>
    public async Task<List<PositionSelectorOutput>> ConstructPositionSelector(List<SysOrg> orgList, List<SysPosition> sysPositions,
        long parentId = 0)
    {
        //找下级组织列表
        var orgInfos = orgList.Where(it => it.ParentId == parentId).OrderBy(it => it.SortCode).ToList();
        var data = new List<PositionSelectorOutput>();
        if (orgInfos.Count > 0)//如果数量大于0
        {
            foreach (var item in orgInfos)//遍历组织
            {
                var childIds = await _sysOrgService.GetOrgChildIdsAsync(item.Id, true, orgList).ConfigureAwait(false);//获取组织下的所有子级ID
                var orgPositions = sysPositions.Where(it => childIds.Contains(it.OrgId)).ToList();//获取组织下的职位
                if (orgPositions.Count > 0)//如果组织和组织下级有职位
                {
                    var positionSelectorOutput = new PositionSelectorOutput
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Children = await ConstructPositionSelector(orgList, sysPositions, item.Id).ConfigureAwait(false)//递归
                    };//实例化职位树
                    var positions = orgPositions.Where(it => it.OrgId == item.Id).ToList();//获取组织下的职位
                    if (positions.Count > 0)//如果数量大于0
                    {
                        foreach (var position in positions)
                        {
                            positionSelectorOutput.Children.Add(new PositionSelectorOutput
                            {
                                Id = position.Id,
                                Name = position.Name
                            });//添加职位
                        }
                    }
                    data.Add(positionSelectorOutput);//添加到列表
                }
            }
            return data;//返回结果
        }
        return new List<PositionSelectorOutput>();
    }

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="input"></param>
    private async Task CheckInput(SysPosition input)
    {
        var sysPositions = await GetAllAsync().ConfigureAwait(false);//获取全部
        if (sysPositions.Any(it => it.OrgId == input.OrgId && it.Name == input.Name && it.Id != input.Id))//判断同级是否有名称重复的
            throw Oops.Bah(Localizer["NameDup", input.Name]);
        if (input.Id > 0)//如果ID大于0表示编辑
        {
            var position = sysPositions.Where(it => it.Id == input.Id).FirstOrDefault();//获取当前职位
            if (position == null)
                throw Oops.Bah(Localizer["SysPositionNull", input.Name]);
        }
        //如果code没填
        if (string.IsNullOrEmpty(input.Code))
        {
            input.Code = RandomHelper.CreateRandomString(10);//赋值Code
        }
        else
        {
            //判断是否有相同的Code
            if (sysPositions.Any(it => it.Code == input.Code && it.Id != input.Id))
                throw Oops.Bah(Localizer["CodeDup", input.Code]);
        }
    }




}

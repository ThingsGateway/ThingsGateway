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

using Furion.DependencyInjection;
using Furion.FriendlyException;

using Mapster;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="IMenuService"/>
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class MenuService : DbRepository<SysResource>, IMenuService
{
    private readonly IRelationService _relationService;
    private readonly IResourceService _resourceService;
    private readonly IRoleService _roleService;

    /// <inheritdoc cref="IMenuService"/>
    public MenuService(IResourceService resourceService, IRelationService relationService, IRoleService roleService)
    {
        _roleService = roleService;
        _resourceService = resourceService;
        _relationService = relationService;
    }

    /// <inheritdoc />
    [OperDesc("添加菜单")]
    public async Task AddAsync(MenuAddInput input)
    {
        await CheckInputAsync(input);//检查参数
        var sysResource = input.Adapt<SysResource>();//实体转换

        if (await InsertAsync(sysResource))//插入数据
            _resourceService.RefreshCache(ResourceCategoryEnum.MENU);//刷新菜单缓存
    }

    /// <inheritdoc />
    [OperDesc("删除菜单")]
    public async Task DeleteAsync(params long[] input)
    {
        //获取所有ID
        var ids = input.ToList();
        if (ids.Count > 0)
        {
            //获取所有菜单和按钮
            var resourceList = await _resourceService.GetListByCategorysAsync(new List<ResourceCategoryEnum> { ResourceCategoryEnum.MENU, ResourceCategoryEnum.BUTTON });
            //找到要删除的菜单
            var sysResources = resourceList.Where(it => ids.Contains(it.Id)).ToList();
            //查找内置菜单
            var system = sysResources.Where(it => it.Code == ResourceConst.System).FirstOrDefault();
            if (system != null)
                throw Oops.Bah($"不可删除系统菜单:{system.Title}");
            //需要删除的资源ID列表
            var resourceIds = new List<long>();
            //遍历菜单列表
            sysResources.ForEach(it =>
            {
                //获取菜单所有子节点
                var child = _resourceService.GetChildListById(resourceList, it.Id, false);
                //将子节点ID添加到删除资源ID列表
                resourceIds.AddRange(child.Select(it => it.Id).ToList());
                resourceIds.Add(it.Id);//添加到删除资源ID列表
            });
            ids.AddRange(resourceIds);//添加到删除ID列表
                                      //事务
            var result = await itenant.UseTranAsync(async () =>
            {
                await DeleteByIdsAsync(ids.Cast<object>().ToArray());//删除菜单和按钮
                await Context.Deleteable<SysRelation>()//关系表删除对应SYS_ROLE_HAS_RESOURCE
                 .Where(it => it.Category == CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE && resourceIds.Contains(SqlFunc.ToInt64(it.TargetId))).ExecuteCommandAsync();
            });
            if (result.IsSuccess)//如果成功了
            {
                _resourceService.RefreshCache(ResourceCategoryEnum.MENU);//资源表菜单刷新缓存
                _resourceService.RefreshCache(ResourceCategoryEnum.BUTTON);//资源表按钮刷新缓存
                _relationService.RefreshCache(CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//关系表刷新缓存
            }
            else
            {
                //写日志
                throw Oops.Oh(result.ErrorMessage);
            }
        }
    }

    /// <inheritdoc />
    public async Task<SysResource> DetailAsync(BaseIdInput input)
    {
        var sysResources = await _resourceService.GetListByCategoryAsync(ResourceCategoryEnum.MENU);
        var resource = sysResources.Where(it => it.Id == input.Id).FirstOrDefault();
        return resource;
    }

    /// <inheritdoc />
    [OperDesc("编辑菜单")]
    public async Task EditAsync(MenuEditInput input)
    {
        await CheckInputAsync(input);//检查参数
        var sysResource = input.Adapt<SysResource>();//实体转换
        if (await UpdateAsync(sysResource))//更新数据
        {
            _resourceService.RefreshCache(ResourceCategoryEnum.MENU);//刷新菜单缓存
            //需要更新资源权限，因为地址可能改变，页面权限需要更改
            await _roleService.RefreshResourceAsync(input.Id);
        }
    }

    /// <inheritdoc />
    public async Task<List<SysResource>> TreeAsync(MenuPageInput input)
    {
        //获取所有菜单
        var sysResources = await _resourceService.GetListByCategoryAsync(ResourceCategoryEnum.MENU);
        sysResources = sysResources
            .Where(it => it.ParentId == input.ParentId)
            .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Title == input.SearchKey)//根据关键字查找
            .ToList();
        //构建菜单树
        var tree = _resourceService.ResourceListToTree(sysResources, input.ParentId);
        return tree;
    }

    #region 方法

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysResource"></param>
    private async Task CheckInputAsync(SysResource sysResource)
    {
        //获取所有菜单列表
        var menList = await _resourceService.GetListByCategoryAsync(ResourceCategoryEnum.MENU);
        //判断是否有同级且同名的菜单
        if (menList.Any(it => it.ParentId == sysResource.ParentId && it.Title == sysResource.Title && it.Id != sysResource.Id))
            throw Oops.Bah($"存在重复的菜单名称:{sysResource.Title}");
        if (sysResource.ParentId != 0)
        {
            //获取父级,判断父级ID正不正确
            var parent = menList.Where(it => it.Id == sysResource.ParentId).FirstOrDefault();
            if (parent != null)
            {
                if (parent.Id == sysResource.Id)
                    throw Oops.Bah($"上级菜单不能选择自己");
            }
            else
            {
                throw Oops.Bah($"上级菜单不存在:{sysResource.ParentId}");
            }
        }
    }

    #endregion 方法
}
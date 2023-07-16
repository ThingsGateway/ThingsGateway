#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Application
{
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
        public async Task Add(MenuAddInput input)
        {
            await CheckInput(input);//检查参数
            var sysResource = input.Adapt<SysResource>();//实体转换

            if (await InsertAsync(sysResource))//插入数据
                await _resourceService.RefreshCache(MenuCategoryEnum.MENU);//刷新菜单缓存
        }

        /// <inheritdoc />
        [OperDesc("删除菜单")]
        public async Task Delete(List<BaseIdInput> input)
        {
            //获取所有ID
            var ids = input.Select(it => it.Id).ToList();
            if (ids.Count > 0)
            {
                //获取所有菜单和按钮
                var resourceList = await _resourceService.GetListAsync(new List<MenuCategoryEnum> { MenuCategoryEnum.MENU, MenuCategoryEnum.BUTTON });
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
                    await _resourceService.RefreshCache(MenuCategoryEnum.MENU);//资源表菜单刷新缓存
                    await _resourceService.RefreshCache(MenuCategoryEnum.BUTTON);//资源表按钮刷新缓存
                    await _relationService.RefreshCache(CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//关系表刷新缓存
                }
                else
                {
                    //写日志
                    throw Oops.Oh(ErrorCodeEnum.A0002);
                }
            }
        }

        /// <inheritdoc />
        public async Task<SysResource> Detail(BaseIdInput input)
        {
            var sysResources = await _resourceService.GetListByCategory(MenuCategoryEnum.MENU);
            var resource = sysResources.Where(it => it.Id == input.Id).FirstOrDefault();
            return resource;
        }

        /// <inheritdoc />
        [OperDesc("编辑菜单")]
        public async Task Edit(MenuEditInput input)
        {
            await CheckInput(input);//检查参数
            var sysResource = input.Adapt<SysResource>();//实体转换
            if (await UpdateAsync(sysResource))//更新数据
            {
                await _resourceService.RefreshCache(MenuCategoryEnum.MENU);//刷新菜单缓存
                //需要更新资源权限，因为地址可能改变，页面权限需要更改
                await _roleService.RefreshResource(input.Id);
            }
        }

        /// <inheritdoc />
        public async Task<List<SysResource>> Tree(MenuPageInput input)
        {
            //获取所有菜单
            var sysResources = await _resourceService.GetListByCategory(MenuCategoryEnum.MENU);
            sysResources = sysResources
                .Where(it => it.ParentId == input.ParentId)
                .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Title == input.SearchKey)//根据关键字查找
                .ToList();
            //构建菜单树
            var tree = sysResources.ResourceListToTree(input.ParentId);
            return tree;
        }

        #region 方法

        /// <summary>
        /// 检查输入参数
        /// </summary>
        /// <param name="sysResource"></param>
        private async Task CheckInput(SysResource sysResource)
        {
            //获取所有菜单列表
            var menList = await _resourceService.GetListByCategory(MenuCategoryEnum.MENU);
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
}
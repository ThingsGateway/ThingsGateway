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
using Furion.EventBus;
using Furion.FriendlyException;

using Mapster;

using ThingsGateway.Admin.Core;

using Yitter.IdGenerator;

namespace ThingsGateway.Admin.Application
{
    /// <inheritdoc cref="IRoleService"/>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class RoleService : DbRepository<SysRole>, IRoleService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IRelationService _relationService;
        private readonly IResourceService _resourceService;

        /// <inheritdoc cref="IRoleService"/>
        public RoleService(
                           IRelationService relationService,
                           IResourceService resourceService,
                           IEventPublisher eventPublisher)
        {
            _relationService = relationService;
            _resourceService = resourceService;
            _eventPublisher = eventPublisher;
        }

        /// <inheritdoc />
        [OperDesc("添加角色")]
        public async Task AddAsync(RoleAddInput input)
        {
            await CheckInput(input);//检查参数
            var sysRole = input.Adapt<SysRole>();//实体转换
            sysRole.Code = YitIdHelper.NextId().ToString();//赋值Code
            if (await InsertAsync(sysRole))//插入数据
                RefreshCache();//刷新缓存
        }

        /// <inheritdoc />
        [OperDesc("删除角色")]
        public async Task DeleteAsync(params long[] input)
        {
            //获取所有ID
            var ids = input.ToList();
            if (ids.Count > 0)
            {
                var sysRoles = await GetListAsync();//获取所有角色
                var hasSuperAdmin = sysRoles.Any(it => it.Code == RoleConst.SuperAdmin && ids.Contains(it.Id));//判断是否有超级管理员
                if (hasSuperAdmin) throw Oops.Bah($"不可删除系统内置超管角色");

                //数据库是string所以这里转下
                var targetIds = ids.Select(it => it.ToString()).ToList();
                //定义删除的关系
                var delRelations = new List<string> { CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE, CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION };
                //事务
                var result = await itenant.UseTranAsync(async () =>
                {
                    await DeleteByIdsAsync(ids.Cast<object>().ToArray());//删除按钮

                    //删除关系表角色与资源关系，角色与权限关系
                    await Context.Deleteable<SysRelation>().Where(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category)).ExecuteCommandAsync();
                    //删除关系表角色与用户关系
                    await Context.Deleteable<SysRelation>().Where(it => targetIds.Contains(it.TargetId) && it.Category == CateGoryConst.Relation_SYS_USER_HAS_ROLE).ExecuteCommandAsync();

                });
                if (result.IsSuccess)//如果成功了
                {
                    RefreshCache();//刷新缓存
                    _relationService.RefreshCache(CateGoryConst.Relation_SYS_USER_HAS_ROLE);//关系表刷新SYS_USER_HAS_ROLE缓存
                    _relationService.RefreshCache(CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//关系表刷新SYS_ROLE_HAS_RESOURCE缓存
                    _relationService.RefreshCache(CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);//关系表刷新SYS_ROLE_HAS_PERMISSION缓存
                    await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, ids);//清除角色下用户缓存
                }
                else
                {
                    //写日志
                    throw Oops.Oh(result.ErrorMessage);
                }
            }
        }

        /// <inheritdoc />
        [OperDesc("编辑角色")]
        public async Task EditAsync(RoleEditInput input)
        {
            //判断是否超管
            if (input.Code == RoleConst.SuperAdmin)
                throw Oops.Bah($"不可编辑超管角色");
            await CheckInput(input);//检查参数
            var role = await GetFirstAsync(it => it.Id == input.Id);//获取角色
            if (role != null)
            {
                var permissions = new List<SysRelation>();

                var sysRole = input.Adapt<SysRole>();//实体转换
                                                     //事务
                var result = await itenant.UseTranAsync(async () =>
                {
                    await UpdateAsync(sysRole);//更新角色
                    if (permissions.Any())//如果有授权权限就更新
                        await Context.Updateable(permissions).ExecuteCommandAsync();
                });
                if (result.IsSuccess)//如果成功了
                {
                    RefreshCache();//刷新缓存
                    if (permissions.Any())//如果有授权
                        _relationService.RefreshCache(CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);//关系表刷新SYS_ROLE_HAS_PERMISSION缓存
                    await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, new List<long> { input.Id });//清除角色下用户缓存
                }
                else
                {
                    //写日志
                    throw Oops.Oh(result.ErrorMessage);
                }
            }
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns></returns>
        public override async Task<List<SysRole>> GetListAsync()
        {
            //先从Cache拿，需要获取新的对象，避免操作导致缓存中对象改变
            var sysRoles = CacheStatic.Cache.Get<List<SysRole>>(CacheConst.CACHE_SYSROLE, true);
            if (sysRoles == null)
            {
                //cache没有就去数据库拿
                sysRoles = await base.GetListAsync();
                if (sysRoles.Count > 0)
                {
                    //插入Cache
                    CacheStatic.Cache.Set(CacheConst.CACHE_SYSROLE, sysRoles, true);
                }
            }
            return sysRoles;
        }

        /// <inheritdoc/>
        public async Task<List<long>> GetRoleIdListByUserIdAsync(long userId)
        {
            List<SysRole> cods = new();//角色代码集合
            var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);//根据用户ID获取角色ID
            var roleIdList = roleList.Select(x => x.TargetId.ToLong()).ToList();//角色ID列表
            return roleIdList;
        }

        /// <inheritdoc/>
        public async Task<List<SysRole>> GetRoleListByUserIdAsync(long userId)
        {
            List<SysRole> cods = new();//角色代码集合
            var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);//根据用户ID获取角色ID
            var roleIdList = roleList.Select(x => x.TargetId.ToLong()).ToList();//角色ID列表
            if (roleIdList.Count > 0)
            {
                cods = await GetListAsync(it => roleIdList.Contains(it.Id));
            }
            return cods;
        }

        /// <inheritdoc />
        [OperDesc("角色授权")]
        public async Task GrantResourceAsync(GrantResourceInput input)
        {
            var menuIds = input.GrantInfoList.Select(it => it.MenuId).ToList();//菜单ID
            var extJsons = input.GrantInfoList.Select(it => it.ToJsonString()).ToList();//拓展信息
            var relationRoles = new List<SysRelation>();//要添加的角色资源和授权关系表
            var sysRole = (await GetListAsync()).Where(it => it.Id == input.Id).FirstOrDefault();//获取角色
            if (sysRole != null)
            {
                #region 角色资源处理

                //遍历角色列表
                for (int i = 0; i < menuIds.Count; i++)
                {
                    //将角色资源添加到列表
                    relationRoles.Add(new SysRelation
                    {
                        ObjectId = sysRole.Id,
                        TargetId = menuIds[i].ToString(),
                        Category = CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE,
                        ExtJson = extJsons?[i]
                    });
                }

                #endregion 角色资源处理

                #region 角色权限处理

                var relationRolePer = new List<SysRelation>();//要添加的角色有哪些权限列表

                //获取菜单信息
                var menus = await GetMenuByMenuIds(menuIds);
                if (menus.Count > 0)
                {
                    //获取权限授权树
                    var permissions = PermissionUtil.PermissionTreeSelector(menus.Select(it => it.Component).ToList());
                    permissions.ForEach(it =>
                    {
                        //新建角色权限关系
                        relationRolePer.Add(new SysRelation
                        {
                            ObjectId = sysRole.Id,
                            TargetId = it.ApiRoute,
                            Category = CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION,
                            ExtJson = new RelationRolePermission
                            {
                                ApiUrl = it.ApiRoute,
                            }.ToJsonString()
                        });
                    });
                }
                relationRoles.AddRange(relationRolePer);//合并列表

                #endregion 角色权限处理

                #region 保存数据库

                //事务
                var result = await itenant.UseTranAsync(async () =>
                {
                    //删除老的
                    await Context.Deleteable<SysRelation>().Where(it => it.ObjectId == sysRole.Id
                    &&
                    (it.Category == CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION
                    || it.Category == CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE)
                    )
                    .ExecuteCommandAsync();
                    await Context.Insertable(relationRoles).ExecuteCommandAsync();
                });
                if (result.IsSuccess)//如果成功了
                {
                    _relationService.RefreshCache(CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//刷新关系缓存
                    _relationService.RefreshCache(CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);//刷新关系缓存
                    await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, new List<long> { input.Id });//发送事件清除角色下用户缓存
                }
                else
                {
                    //写日志
                    throw Oops.Oh(result.ErrorMessage);
                }

                #endregion 保存数据库
            }
        }

        /// <inheritdoc />
        [OperDesc("用户授权")]
        public async Task GrantUserAsync(GrantUserInput input)
        {
            var sysRelations = new List<SysRelation>();//关系列表

            //遍历用户ID
            input.GrantInfoList.ForEach(it =>
            {
                sysRelations.Add(new SysRelation
                {
                    ObjectId = it,
                    TargetId = input.Id.ToString(),
                    Category = CateGoryConst.Relation_SYS_USER_HAS_ROLE
                });
            });

            //事务
            var result = await itenant.UseTranAsync(async () =>
            {
                //删除老的
                await Context.Deleteable<SysRelation>().Where(it => it.TargetId == input.Id.ToString() && it.Category == CateGoryConst.Relation_SYS_USER_HAS_ROLE).ExecuteCommandAsync();
                await Context.Insertable(sysRelations).ExecuteCommandAsync();//添加新的

            });
            if (result.IsSuccess)//如果成功了
            {
                _relationService.RefreshCache(CateGoryConst.Relation_SYS_USER_HAS_ROLE);//刷新关系表SYS_USER_HAS_ROLE缓存
                await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, new List<long> { input.Id.Value });//清除角色下用户缓存
            }
            else
            {
                //写日志
                throw Oops.Oh(result.ErrorMessage);
            }
        }

        /// <inheritdoc />
        public async Task<RoleOwnResourceOutput> OwnResourceAsync(long input)
        {
            RoleOwnResourceOutput roleOwnResource = new() { Id = input };//定义结果集
            List<RelationRoleResuorce> GrantInfoList = new();//已授权信息集合
                                                             //获取关系列表
            var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(input, CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);
            //遍历关系表
            relations.ForEach(it =>
            {
                //将扩展信息转为实体
                var relationRole = it.ExtJson.ToJsonWithT<RelationRoleResuorce>();
                GrantInfoList.Add(relationRole);//添加到已授权信息
            });
            roleOwnResource.GrantInfoList = GrantInfoList;//赋值已授权信息
            return roleOwnResource;
        }

        /// <inheritdoc />
        public async Task<List<long>> OwnUserAsync(long input)
        {
            //获取关系列表
            var relations = await _relationService.GetRelationListByTargetIdAndCategoryAsync(input.ToString(), CateGoryConst.Relation_SYS_USER_HAS_ROLE);
            return relations.Select(it => it.ObjectId).ToList();
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<SysRole>> PageAsync(RolePageInput input)
        {
            var query = Context.Queryable<SysRole>()
                             .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Name.Contains(input.SearchKey));//根据关键字查询
            for (int i = 0; i < input.SortField.Count; i++)
            {
                query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
            }
            query = query.OrderBy(it => it.SortCode);//排序

            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

        /// <inheritdoc />
        public void RefreshCache()
        {
            CacheStatic.Cache.Remove(CacheConst.CACHE_SYSROLE);//删除KEY
        }

        /// <inheritdoc />
        public async Task RefreshResourceAsync(long? menuId = null)
        {
            var data = await GetListAsync();
            foreach (var item in data)
            {
                var r1 = await OwnResourceAsync(item.Id);
                if (menuId == null || r1.GrantInfoList.Any(a => a.MenuId == menuId))
                {
                    await GrantResourceAsync(new GrantResourceInput() { Id = item.Id, GrantInfoList = r1.GrantInfoList });
                }
            }


        }

        /// <inheritdoc />
        public async Task<List<SysRole>> RoleSelectorAsync(string searchKey = null)
        {
            var result = await Context.Queryable<SysRole>()
                             .WhereIF(!string.IsNullOrEmpty(searchKey), it => it.Name.Contains(searchKey))//根据关键字查询
                             .ToListAsync();
            return result;
        }

        #region 方法

        /// <summary>
        /// 检查输入参数
        /// </summary>
        /// <param name="sysRole"></param>
        private async Task CheckInput(SysRole sysRole)
        {
            var sysRoles = await GetListAsync();//获取所有
            var repeatName = sysRoles.Any(it => it.Name == sysRole.Name && it.Id != sysRole.Id);//是否有重复角色名称
            if (repeatName)//如果有
            {
                throw Oops.Bah($"存在重复的角色:{sysRole.Name}");
            }
        }

        /// <summary>
        /// 根据菜单ID获取菜单
        /// </summary>
        /// <param name="menuIds"></param>
        /// <returns></returns>
        private async Task<List<SysResource>> GetMenuByMenuIds(List<long> menuIds)
        {
            //获取所有菜单
            var menuList = await _resourceService.GetListByCategoryAsync(ResourceCategoryEnum.MENU);
            //获取菜单信息
            var menus = menuList.Where(it => menuIds.Contains(it.Id)).ToList();

            return menus;
        }

        #endregion 方法
    }
}
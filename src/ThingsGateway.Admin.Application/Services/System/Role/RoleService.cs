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

using Furion.FriendlyException;

using Mapster;

using ThingsGateway.Core.Extension.Json;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IRoleService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class RoleService : DbRepository<SysRole>, IRoleService
{
    private readonly ISimpleCacheService _simpleCacheService;
    private readonly IRelationService _relationService;
    private readonly IResourceService _resourceService;
    private readonly IEventPublisher _eventPublisher;

    public RoleService(
        ISimpleCacheService simpleCacheService,
        IRelationService relationService,
        IResourceService resourceService,
        IEventPublisher eventPublisher)
    {
        _simpleCacheService = simpleCacheService;
        _relationService = relationService;
        _resourceService = resourceService;
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc/>
    public override async Task<List<SysRole>> GetListAsync()
    {
        //先从Redis拿
        var sysRoles = _simpleCacheService.Get<List<SysRole>>(SystemConst.Cache_SysRole);
        if (sysRoles == null)
        {
            //redis没有就去数据库拿
            sysRoles = await base.GetListAsync();
            if (sysRoles.Count > 0)
            {
                //插入Redis
                _simpleCacheService.Set(SystemConst.Cache_SysRole, sysRoles);
            }
        }
        return sysRoles;
    }

    /// <inheritdoc/>
    public async Task<List<SysRole>> GetRoleListByUserIdAsync(long userId)
    {
        var cods = new List<SysRole>();//角色代码集合
        var roleList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userId, CateGoryConst.Relation_SYS_USER_HAS_ROLE);//根据用户ID获取角色ID
        var roleIdList = roleList.Select(x => x.TargetId.ToLong()).ToList();//角色ID列表
        if (roleIdList.Count > 0)
        {
            cods = (await GetListAsync()).Where(it => roleIdList.Contains(it.Id)).ToList();
        }
        return cods;
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<SysRole>> PageAsync(RolePageInput input)
    {
        var query = Context.Queryable<SysRole>()
            .WhereIF(!string.IsNullOrEmpty(input.Category), it => it.Category == input.Category)//根据分类
            .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Name.Contains(input.SearchKey));//根据关键字查询
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }
        query = query.OrderBy(it => it.SortCode);//排序
        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        return pageInfo;
    }

    /// <inheritdoc />
    [OperDesc("添加角色")]
    public async Task AddAsync(RoleAddInput input)
    {
        await CheckInput(input);//检查参数
        var sysRole = input.Adapt<SysRole>();//实体转换
        sysRole.Code = RandomUtil.CreateRandomString(10);//赋值Code
        if (await InsertAsync(sysRole))//插入数据
            await RefreshCacheAsync();//刷新缓存
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
            var sysRole = input.Adapt<SysRole>();//实体转换
            //事务
            var result = await Context.AsTenant().UseTranAsync(async () =>
            {
                await UpdateAsync(sysRole);//更新角色
            });
            if (result.IsSuccess)//如果成功了
            {
                await RefreshCacheAsync();//刷新缓存
                await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, new List<long> { input.Id });//清除角色下用户缓存
            }
            else
            {
                //写日志
                throw new(result.ErrorMessage, result.ErrorException);
            }
        }
    }

    /// <inheritdoc />
    [OperDesc("删除角色")]
    public async Task DeleteAsync(List<BaseIdInput> input)
    {
        //获取所有ID
        var ids = input.Select(it => it.Id).ToList();
        if (ids.Count > 0)
        {
            var sysRoles = await GetListAsync();//获取所有角色
            var hasSuperAdmin = sysRoles.Any(it => it.Code == RoleConst.SuperAdmin && ids.Contains(it.Id));//判断是否有超级管理员
            if (hasSuperAdmin) throw Oops.Bah($"不可删除系统内置超管角色");

            //数据库是string所以这里转下
            var targetIds = ids.Select(it => it.ToString()).ToList();
            //定义删除的关系
            var delRelations = new List<string> { CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE, CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION, CateGoryConst.Relation_SYS_ROLE_HAS_OPENAPIPERMISSION };
            //事务
            var result = await NewContent.UseTranAsync(async () =>
            {
                await DeleteByIdsAsync(ids.Cast<object>().ToArray());//删除按钮
                var relationRep = ChangeRepository<DbRepository<SysRelation>>();//切换仓储
                relationRep.NewContent = NewContent;
                //删除关系表角色与资源关系，角色与权限关系
                await relationRep.DeleteAsync(it => ids.Contains(it.ObjectId) && delRelations.Contains(it.Category));
                //删除关系表角色与用户关系
                await relationRep.DeleteAsync(it => targetIds.Contains(it.TargetId) && it.Category == CateGoryConst.Relation_SYS_USER_HAS_ROLE);
            });
            if (result.IsSuccess)//如果成功了
            {
                await RefreshCacheAsync();//刷新缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_USER_HAS_ROLE);//关系表刷新SYS_USER_HAS_ROLE缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//关系表刷新Relation_SYS_ROLE_HAS_RESOURCE缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);//关系表刷新Relation_SYS_ROLE_HAS_PERMISSION缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_ROLE_HAS_OPENAPIPERMISSION);//关系表刷新Relation_SYS_ROLE_HAS_OPENAPIPERMISSION缓存
                await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, ids);//清除角色下用户缓存
            }
            else
            {
                //写日志
                throw new(result.ErrorMessage, result.ErrorException);
            }
        }
    }

    /// <inheritdoc />
    public async Task<RoleOwnResourceOutput> OwnResourceAsync(BaseIdInput input, string category = CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE)
    {
        var roleOwnResource = new RoleOwnResourceOutput() { Id = input.Id };//定义结果集
        var GrantInfoList = new List<RelationRoleResource>();//已授权信息集合
        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(input.Id, category);
        //遍历关系表
        relations.ForEach(it =>
        {
            //将扩展信息转为实体
            var relationRole = it.ExtJson.FromJsonString<RelationRoleResource>();
            GrantInfoList.Add(relationRole);//添加到已授权信息
        });
        roleOwnResource.GrantInfoList = GrantInfoList;//赋值已授权信息
        return roleOwnResource;
    }

    /// <inheritdoc />
    [OperDesc("角色授权资源")]
    public async Task GrantResourceAsync(GrantResourceInput input)
    {
        var menuIds = input.GrantInfoList.Select(it => it.MenuId).ToList();//菜单ID
        var extJsons = input.GrantInfoList.Select(it => it.ToJsonString()).ToList();//拓展信息
        var relationRoles = new List<SysRelation>();//要添加的角色资源和授权关系表
        var sysRole = (await GetListAsync()).Where(it => it.Id == input.Id).FirstOrDefault();//获取角色
        if (sysRole != null)
        {
            if (sysRole.Category == CateGoryConst.Role_API)
                throw Oops.Bah("API角色不允许授权资源");

            #region 角色资源处理

            //遍历菜单列表
            for (var i = 0; i < menuIds.Count; i++)
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

            #region 角色权限处理.

            var relationRolePer = new List<SysRelation>();//要添加的角色有哪些权限列表

            //获取菜单信息
            var menus = await _resourceService.GetMenuByMenuIdsAsync(menuIds);
            if (menus.Count > 0)
            {
                //获取权限授权树
                var permissions = _resourceService.PermissionTreeSelector(menus.Select(it => it.Href).ToList());
                permissions.ForEach(it =>
                {
                    //新建角色权限关系
                    relationRolePer.Add(new SysRelation
                    {
                        ObjectId = sysRole.Id,
                        TargetId = it.ApiRoute,
                        Category = CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION,
                        ExtJson = new RelationRolePermission { ApiUrl = it.ApiRoute }
                            .ToJsonString()
                    });
                });
            }
            relationRoles.AddRange(relationRolePer);//合并列表

            #endregion 角色权限处理.

            #region 保存数据库

            //事务
            var result = await NewContent.UseTranAsync(async () =>
            {
                var relationRep = ChangeRepository<DbRepository<SysRelation>>();//切换仓储
                relationRep.NewContent = NewContent;

                //如果不是代码生成,就删除老的
                await relationRep.DeleteAsync(it =>
                    it.ObjectId == sysRole.Id && (it.Category == CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION || it.Category == CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE));
                await relationRep.InsertRangeAsync(relationRoles);//添加新的
            });
            if (result.IsSuccess)//如果成功了
            {
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);//刷新关系缓存
                await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);//刷新关系缓存
                await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, new List<long> { input.Id });//发送事件清除角色下用户缓存
            }
            else
            {
                throw new(result.ErrorMessage, result.ErrorException);
            }

            #endregion 保存数据库
        }
    }

    /// <inheritdoc />
    public async Task<RoleOwnPermissionOutput> OwnPermissionAsync(BaseIdInput input)
    {
        var roleOwnPermission = new RoleOwnPermissionOutput { Id = input.Id };//定义结果集
        var GrantInfoList = new List<RelationRolePermission>();//已授权信息集合
        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(input.Id, CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION);
        //遍历关系表
        relations.ForEach(it =>
        {
            //将扩展信息转为实体
            var relationPermission = it.ExtJson.FromJsonString<RelationRolePermission>();
            GrantInfoList.Add(relationPermission);//添加到已授权信息
        });
        roleOwnPermission.GrantInfoList = GrantInfoList;//赋值已授权信息
        return roleOwnPermission;
    }

    /// <inheritdoc />
    [OperDesc("角色授权权限")]
    public async Task GrantPermissionAsync(GrantPermissionInput input)
    {
        var sysRole = (await GetListAsync()).Where(it => it.Id == input.Id).FirstOrDefault();//获取角色
        if (sysRole != null)
        {
            var apiUrls = input.GrantInfoList.Select(it => it.ApiUrl).ToList();//apiurl列表
            var extJsons = input.GrantInfoList.Select(it => it.ToJsonString()).ToList();//拓展信息
            await _relationService.SaveRelationBatchAsync(CateGoryConst.Relation_SYS_ROLE_HAS_PERMISSION, input.Id, apiUrls, extJsons, true);//添加到数据库
            await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, new List<long> { input.Id });//清除角色下用户缓存
        }
    }

    #region OPENAPI

    /// <inheritdoc />
    public async Task<RoleOwnPermissionOutput> ApiOwnPermissionAsync(BaseIdInput input)
    {
        var roleOwnPermission = new RoleOwnPermissionOutput { Id = input.Id };//定义结果集
        var GrantInfoList = new List<RelationRolePermission>();//已授权信息集合
        //获取关系列表
        var relations = await _relationService.GetRelationListByObjectIdAndCategoryAsync(input.Id, CateGoryConst.Relation_SYS_ROLE_HAS_OPENAPIPERMISSION);
        //遍历关系表
        relations.ForEach(it =>
        {
            //将扩展信息转为实体
            var relationPermission = it.ExtJson.FromJsonString<RelationRolePermission>();
            GrantInfoList.Add(relationPermission);//添加到已授权信息
        });
        roleOwnPermission.GrantInfoList = GrantInfoList;//赋值已授权信息
        return roleOwnPermission;
    }

    /// <inheritdoc />
    [OperDesc("角色授权API权限")]
    public async Task ApiGrantPermissionAsync(GrantPermissionInput input)
    {
        var sysRole = (await GetListAsync()).Where(it => it.Id == input.Id).FirstOrDefault();//获取角色
        if (sysRole != null)
        {
            var apiUrls = input.GrantInfoList.Select(it => it.ApiUrl).ToList();//apiurl列表
            var extJsons = input.GrantInfoList.Select(it => it.ToJsonString()).ToList();//拓展信息
            await _relationService.SaveRelationBatchAsync(CateGoryConst.Relation_SYS_ROLE_HAS_OPENAPIPERMISSION, input.Id, apiUrls, extJsons, true);//添加到数据库
            await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, new List<long> { input.Id });//清除角色下用户缓存
        }
    }

    #endregion OPENAPI

    /// <inheritdoc />
    public async Task<List<long>> OwnUserAsync(BaseIdInput input)
    {
        //获取关系列表
        var relations = await _relationService.GetRelationListByTargetIdAndCategoryAsync(input.Id.ToString(), CateGoryConst.Relation_SYS_USER_HAS_ROLE);
        return relations.Select(it => it.ObjectId).ToList();
    }

    /// <inheritdoc />
    [OperDesc("角色授权用户")]
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
        var result = await NewContent.UseTranAsync(async () =>
        {
            var relationRep = ChangeRepository<DbRepository<SysRelation>>();//切换仓储
            relationRep.NewContent = NewContent;
            var targetId = input.Id.ToString();//目标ID转string
            await relationRep.DeleteAsync(it => it.TargetId == targetId && it.Category == CateGoryConst.Relation_SYS_USER_HAS_ROLE);//删除老的
            await relationRep.InsertRangeAsync(sysRelations);//添加新的
        });
        if (result.IsSuccess)//如果成功了
        {
            await _relationService.RefreshCacheAsync(CateGoryConst.Relation_SYS_USER_HAS_ROLE);//刷新关系表SYS_USER_HAS_ROLE缓存
            await _eventPublisher.PublishAsync(EventSubscriberConst.ClearUserCache, new List<long> { input.Id });//清除角色下用户缓存
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<SysRole>> RoleSelectorAsync(RoleSelectorInput input)
    {
        var result = await Context.Queryable<SysRole>()
            .WhereIF(!string.IsNullOrEmpty(input.Category), it => it.Category == input.Category)//分类
            .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Name.Contains(input.SearchKey))//根据关键字查询
            .ToPagedListAsync(input.Current, input.Size);
        return result;
    }

    /// <inheritdoc />
    public async Task<List<PermissionTreeSelector>> RolePermissionTreeSelectorAsync(BaseIdInput input)
    {
        var permissionTreeSelectors = new List<PermissionTreeSelector>();//授权树结果集
        //获取角色资源关系
        var relationsRes = await _relationService.GetRelationByCategoryAsync(CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);
        var menuIds = relationsRes.Where(it => it.ObjectId == input.Id).Select(it => it.TargetId.ToLong()).ToList();
        if (menuIds.Any())
        {
            //获取菜单信息
            var menus = await _resourceService.GetMenuByMenuIdsAsync(menuIds);
            //获取权限授权树
            var permissions = _resourceService.PermissionTreeSelector(menus.Select(it => it.Href).ToList());
            return permissions;
            //if (permissions.Count > 0)
            //{
            //    permissionTreeSelectors = permissions.Select(it => it.PermissionName).ToList();//返回授权树权限名称列表
            //}
        }
        return permissionTreeSelectors;
    }

    /// <inheritdoc />
    public async Task RefreshCacheAsync()
    {
        _simpleCacheService.Remove(SystemConst.Cache_SysRole);//删除KEY
        await GetListAsync();//重新缓存
    }

    /// <inheritdoc />
    public async Task<List<SysRole>> GetRoleListByIdListAsync(IdListInput input)
    {
        var roles = await GetListAsync();
        var roleList = roles.Where(it => input.IdList.Contains(it.Id)).ToList();// 获取指定ID的岗位列表
        return roleList;
    }

    #region 方法

    /// <summary>
    /// 检查输入参数
    /// </summary>
    /// <param name="sysRole"></param>
    private async Task CheckInput(SysRole sysRole)
    {
        //判断分类
        if (sysRole.Category != CateGoryConst.Role_GLOBAL && sysRole.Category != CateGoryConst.Role_API)
            throw Oops.Bah($"角色所属分类错误:{sysRole.Category}");

        var sysRoles = await GetListAsync();//获取所有
        var repeatName = sysRoles.Any(it => it.Name == sysRole.Name && it.Id != sysRole.Id);//是否有重复角色名称
        if (repeatName)//如果有
        {
            throw Oops.Bah($"同组织下存在重复的角色:{sysRole.Name}");
        }
    }

    #endregion 方法
}
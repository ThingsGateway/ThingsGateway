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

namespace ThingsGateway.Application
{
    /// <inheritdoc cref="IResourceService"/>
    public class ResourceService : DbRepository<SysResource>, IResourceService
    {
        private readonly SysCacheService _sysCacheService;

        /// <inheritdoc cref="IResourceService"/>
        public ResourceService(SysCacheService sysCacheService)
        {
            _sysCacheService = sysCacheService;
        }

        /// <inheritdoc/>
        public async Task<List<SysResource>> GetaMenuAndSpaList()
        {
            //获取所有的菜单以及单页面
            var sysResources = await GetListAsync(new List<MenuCategoryEnum> { MenuCategoryEnum.MENU, MenuCategoryEnum.SPA });
            if (sysResources != null)
            {
                //并按分类和排序码排序
                sysResources = sysResources.OrderBy(it => it.Category).ThenBy(it => it.SortCode).ToList();
            }
            return sysResources;
        }

        /// <inheritdoc />
        public List<SysResource> GetChildListById(List<SysResource> sysResources, long resId, bool isContainOneself = true)
        {
            //查找下级
            var childLsit = GetResourceChilden(sysResources, resId);
            if (isContainOneself)//如果包含自己
            {
                //获取自己的机构信息
                var self = sysResources.Where(it => it.Id == resId).FirstOrDefault();
                if (self != null) childLsit.Insert(0, self);//如果机构不为空就插到第一个
            }
            return childLsit;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetCodeByIds(List<long> ids, MenuCategoryEnum category)
        {
            //根据分类获取所有
            var sysResources = await GetListByCategory(category);
            //条件查询
            var result = sysResources.Where(it => ids.Contains(it.Id)).Select(it => it.Code).ToList();
            return result;
        }

        /// <inheritdoc/>
        public async Task<List<SysResource>> GetListAsync(List<MenuCategoryEnum> categoryList = null)
        {
            //定义结果
            var sysResources = new List<SysResource>();

            //定义资源分类列表,如果是空的则获取全部资源
            categoryList = (categoryList != null || (categoryList.Count == 1 && categoryList.FirstOrDefault() == MenuCategoryEnum.None))
                ? categoryList
                : new List<MenuCategoryEnum> { MenuCategoryEnum.MENU, MenuCategoryEnum.BUTTON, MenuCategoryEnum.SPA };
            //遍历列表
            foreach (var category in categoryList)
            {
                //根据分类获取到资源列表
                var data = await GetListByCategory(category);
                //添加到结果集
                sysResources.AddRange(data);
            }
            return sysResources;
        }

        /// <inheritdoc />
        public async Task<List<SysResource>> GetListByCategory(MenuCategoryEnum category)
        {
            //先从Cache拿
            var sysResources = _sysCacheService.Get<List<SysResource>>(CacheConst.Cache_SysResource, category.ToString());
            if (sysResources == null)
            {
                //cache没有就去数据库拿
                sysResources = await CopyNew().GetListAsync(it => it.Category == category);
                if (sysResources.Count > 0)
                {
                    //插入Cache
                    _sysCacheService.Set(CacheConst.Cache_SysResource, category.ToString(), sysResources);
                }
            }
            return sysResources;
        }

        /// <summary>
        /// 获取资源所有下级
        /// </summary>
        /// <param name="resourceList">资源列表</param>
        /// <param name="parentId">父ID</param>
        /// <returns></returns>
        public List<SysResource> GetResourceChilden(List<SysResource> resourceList, long parentId)
        {
            //找下级资源ID列表
            var resources = resourceList.Where(it => it.ParentId == parentId).ToList();
            if (resources.Count > 0)//如果数量大于0
            {
                var data = new List<SysResource>();
                foreach (var item in resources)//遍历资源
                {
                    var res = GetResourceChilden(resourceList, item.Id);
                    data.AddRange(res);//添加子节点;
                    data.Add(item);//添加到列表
                }
                return data;//返回结果
            }
            return new List<SysResource>();
        }

        /// <summary>
        /// 获取上级
        /// </summary>
        /// <param name="resourceList"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public List<SysResource> GetResourceParent(List<SysResource> resourceList, long parentId)
        {
            //找上级资源ID列表
            var resources = resourceList.Where(it => it.Id == parentId).FirstOrDefault();
            if (resources != null)//如果数量大于0
            {
                var data = new List<SysResource>();
                var parents = GetResourceParent(resourceList, resources.ParentId);
                data.AddRange(parents);//添加子节点;
                data.Add(resources);//添加到列表
                return data;//返回结果
            }
            return new List<SysResource>();
        }

        /// <inheritdoc/>
        public async Task RefreshCache(MenuCategoryEnum category)
        {
            //如果分类是空的
            if (category == MenuCategoryEnum.None)
            {
                //删除全部key
                _sysCacheService.RemoveByPrefixKey(CacheConst.Cache_SysResource);
                await GetListAsync();
            }
            else
            {
                //否则只删除一个Key
                _sysCacheService.Remove(CacheConst.Cache_SysResource, category.ToString());
                await GetListByCategory(category);
            }
        }

        /// <inheritdoc />
        public async Task<List<RoleGrantResourceMenu>> ResourceTreeSelector()
        {
            List<RoleGrantResourceMenu> resourceTreeSelector = new List<RoleGrantResourceMenu>();//定义结果
            resourceTreeSelector = await GetRoleGrantResourceMenus();
            return resourceTreeSelector;
        }

        /// <summary>
        /// 获取授权菜单
        /// </summary>
        /// <returns></returns>
        private async Task<List<RoleGrantResourceMenu>> GetRoleGrantResourceMenus()
        {
            var roleGrantResourceMenus = new List<RoleGrantResourceMenu>();//定义结果
            List<SysResource> allMenuList = (await GetListByCategory(MenuCategoryEnum.MENU));//获取所有菜单列表
            List<SysResource> allButtonList = await GetListByCategory(MenuCategoryEnum.BUTTON);//获取所有按钮列表
            var parentMenuList = allMenuList.Where(it => it.ParentId == 0).ToList();//获取一级目录

            //遍历一级目录
            foreach (var parent in parentMenuList)
            {
                //如果是目录则去遍历下级
                if (parent.TargetType == TargetTypeEnum.CATALOG)
                {
                    //获取所有下级菜单
                    var menuList = GetChildListById(allMenuList, parent.Id, false);

                    //遍历下级菜单
                    foreach (var menu in menuList)
                    {
                        //如果菜单类型是菜单
                        if (menu.TargetType == TargetTypeEnum.SELF)
                        {
                            //获取菜单下按钮集合并转换成对应实体
                            var buttonList = allButtonList.Where(it => it.ParentId == menu.Id).ToList();
                            var buttons = buttonList.Adapt<List<RoleGrantResourceButton>>();
                            roleGrantResourceMenus.Add(new()
                            {
                                Id = menu.Id,
                                ParentId = parent.Id,
                                ParentName = parent.Title,
                                Title = GetRoleGrantResourceMenuTitle(parentMenuList, menu),//菜单名称需要特殊处理因为有二级菜单
                                Button = buttons
                            });
                        }
                        else if (menu.TargetType == TargetTypeEnum.BLANK || menu.TargetType == TargetTypeEnum.CALLBACK)//如果是内链或者外链
                        {
                            //直接加到资源列表
                            roleGrantResourceMenus.Add(new()
                            {
                                Id = menu.Id,
                                ParentId = parent.Id,
                                ParentName = parent.Title,
                                Title = menu.Title,
                            });
                        }
                    }
                }
                else
                {
                    //否则就将自己加到一级目录里面
                    roleGrantResourceMenus.Add(new()
                    {
                        Id = parent.Id,
                        ParentId = parent.Id,
                        ParentName = parent.Title,
                        Title = parent.Title,
                    });
                }
            }
            return roleGrantResourceMenus;
        }

        /// <summary>
        /// 获取授权菜单类菜单名称
        /// </summary>
        /// <param name="menuList">菜单列表</param>
        /// <param name="menu">当前菜单</param>
        /// <returns></returns>
        private string GetRoleGrantResourceMenuTitle(List<SysResource> menuList, SysResource menu)
        {
            //查找菜单上级
            var parentList = GetResourceParent(menuList, menu.ParentId);
            //如果有父级菜单
            if (parentList.Count > 0)
            {
                var titles = parentList.Select(it => it.Title).ToList();//提取出父级的name
                var title = string.Join("- ", titles) + $"-{menu.Title}";//根据-分割,转换成字符串并在最后加上菜单的title
                return title;
            }
            else
            {
                return menu.Title;//原路返回
            }
        }
    }
}
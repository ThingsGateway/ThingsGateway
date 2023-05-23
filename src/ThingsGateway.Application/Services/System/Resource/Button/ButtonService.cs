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
    /// <inheritdoc cref="IButtonService"/>
    /// </summary>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class ButtonService : DbRepository<SysResource>, IButtonService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IRelationService _relationService;
        private readonly IResourceService _resourceService;

        /// <inheritdoc cref="IButtonService"/>
        public ButtonService(IResourceService resourceService, IRelationService relationService, IEventPublisher eventPublisher)
        {
            this._resourceService = resourceService;
            this._relationService = relationService;
            this._eventPublisher = eventPublisher;
        }

        /// <inheritdoc />
        public async Task Add(ButtonAddInput input)
        {
            await CheckInput(input);//检查参数
            var sysResource = input.Adapt<SysResource>();//实体转换
            if (await InsertAsync(sysResource))//插入数据
                await _resourceService.RefreshCache(MenuCategoryEnum.BUTTON);//刷新缓存
        }

        /// <inheritdoc />
        [OperDesc("删除按钮")]
        public async Task Delete(List<BaseIdInput> input)
        {
            //获取所有ID
            var ids = input.Select(it => it.Id).ToList();
            //获取所有按钮集合
            var buttonList = await _resourceService.GetListByCategory(MenuCategoryEnum.BUTTON);

            #region 处理关系表角色资源信息

            //获取所有菜单集合
            var menuList = await _resourceService.GetListByCategory(MenuCategoryEnum.MENU);
            //获取按钮的父菜单id集合
            var parentIds = buttonList.Where(it => ids.Contains(it.Id)).Select(it => it.ParentId.ToString()).ToList();
            //获取关系表分类为SYS_ROLE_HAS_RESOURCE数据
            var roleResources = await _relationService.GetRelationByCategory(CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);
            //获取相关关系表数据
            var relationList = roleResources
                    .Where(it => parentIds.Contains(it.TargetId))//目标ID是父ID中
                    .Where(it => it.ExtJson != null).ToList();//扩展信息不为空
                                                              //遍历关系表
            relationList.ForEach(it =>
            {
                var relationRoleResuorce = it.ExtJson.ToJsonEntity<RelationRoleResuorce>();//拓展信息转实体
                var buttonInfo = relationRoleResuorce.ButtonInfo;//获取按钮信息
                if (buttonInfo.Count > 0)
                {
                    var diffArr = buttonInfo.Where(it => !buttonInfo.Contains(it)).ToList(); //找出不同的元素(即交集的补集)
                    relationRoleResuorce.ButtonInfo = diffArr;//重新赋值按钮信息
                    it.ExtJson = relationRoleResuorce.ToJson();//重新赋值拓展信息
                }
            });

            #endregion 处理关系表角色资源信息

            //事务
            var result = await itenant.UseTranAsync(async () =>
            {
                await DeleteByIdsAsync(ids.Cast<object>().ToArray());//删除按钮
                if (relationList.Count > 0)
                {
                    await Context.Updateable(relationList).UpdateColumns(it => it.ExtJson).ExecuteCommandAsync();//修改拓展信息
                }
            });
            if (result.IsSuccess)//如果成功了
            {
                await _resourceService.RefreshCache(MenuCategoryEnum.BUTTON);//资源表按钮刷新缓存

            }
            else
            {
                throw Oops.Oh(ErrorCodeEnum.A0002);
            }
        }

        /// <inheritdoc />
        [OperDesc("编辑按钮")]
        public async Task Edit(ButtonEditInput input)
        {
            await CheckInput(input);//检查参数
            var sysResource = input.Adapt<SysResource>();//实体转换
                                                         //事务
            var result = await itenant.UseTranAsync(async () =>
            {
                await UpdateAsync(sysResource); //更新按钮
            });
            if (result.IsSuccess)//如果成功了
            {
                await _resourceService.RefreshCache(MenuCategoryEnum.BUTTON);//资源表按钮刷新缓存
            }
            else
            {
                throw Oops.Oh(ErrorCodeEnum.A0002);
            }
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<SysResource>> Page(ButtonPageInput input)
        {
            var query = Context.Queryable<SysResource>()
                             .Where(it => it.ParentId == input.ParentId && it.Category == MenuCategoryEnum.BUTTON)
                             .WhereIF(!string.IsNullOrEmpty(input.SearchKey), it => it.Title.Contains(input.SearchKey) || it.Component.Contains(input.SearchKey))//根据关键字查询
                             .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
                             .OrderBy(it => it.SortCode);//排序
            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            return pageInfo;
        }

        #region 方法

        /// <summary>
        /// 检查输入参数
        /// </summary>
        /// <param name="sysResource"></param>
        private async Task CheckInput(SysResource sysResource)
        {
            //获取所有按钮和菜单
            var buttonList = await _resourceService.GetListAsync(new List<MenuCategoryEnum> { MenuCategoryEnum.BUTTON, MenuCategoryEnum.MENU });
            //判断code是否重复
            if (buttonList.Any(it => it.Code == sysResource.Code && it.Id != sysResource.Id))
                throw Oops.Bah($"存在重复的按钮编码:{sysResource.Code}");
            //判断菜单是否存在
            if (!buttonList.Any(it => it.Id == sysResource.ParentId))
                throw Oops.Bah($"不存在的父级菜单:{sysResource.ParentId}");
            sysResource.Category = MenuCategoryEnum.BUTTON;//设置分类为按钮
        }

        #endregion 方法
    }
}
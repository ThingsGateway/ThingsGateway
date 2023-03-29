namespace ThingsGateway.Application
{
    
    /// <inheritdoc cref="IUserCenterService"/>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class UserCenterService : DbRepository<SysUser>, IUserCenterService
    {
        private readonly IConfigService _configService;
        private readonly IMenuService _menuService;
        private readonly IRelationService _relationService;
        private readonly IResourceService _resourceService;
        private readonly ISysUserService _userService;
        private readonly SysCacheService _sysCacheService;

        /// <inheritdoc cref="IUserCenterService"/>
        public UserCenterService(ISysUserService userService,
                                 IRelationService relationService,
                                 IResourceService resourceService,
                                 IMenuService menuService,
                                 IConfigService configService,
                                  SysCacheService sysCacheService
                                 )
        {
            _userService = userService;
            _relationService = relationService;
            _resourceService = resourceService;
            this._menuService = menuService;
            _configService = configService;
            _sysCacheService = sysCacheService;
        }

        /// <inheritdoc />
        public async Task<List<SysResource>> GetOwnMenu(string UserAccount = null)
        {
            var result = new List<SysResource>();
            //获取用户信息
            var userInfo = await _userService.GetUserByAccount(UserAccount ?? UserManager.UserAccount);
            if (userInfo != null)
            {
                //获取所有的菜单和模块和菜单目录以及单页面列表，并按分类和排序码排序
                var allMenuAndSpaList = await _resourceService.GetaMenuAndSpaList();
                List<SysResource> allMenuList = new List<SysResource>();//菜单列表
                List<SysResource> allSpaList = new List<SysResource>();//单页列表
                //遍历菜单集合
                allMenuAndSpaList.ForEach(it =>
                {
                    switch (it.Category)
                    {
                        case MenuCategoryEnum.MENU://菜单
                            allMenuList.Add(it);//添加到菜单列表
                            break;

                        case MenuCategoryEnum.SPA://单页
                            allSpaList.Add(it);//添加到单页列表
                            break;
                    }
                });
                //输出的用户权限菜单
                List<SysResource> myMenus = new();
                //管理员拥有全部权限
                if (UserManager.SuperAdmin)
                {
                    myMenus = allMenuList;
                }
                else
                {
                    //获取角色所拥有的资源集合
                    var resourceList = await _relationService.GetRelationListByObjectIdListAndCategory(userInfo.RoleIdList, CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);
                    //定义菜单ID列表
                    HashSet<long> rolesMenuIdList = new HashSet<long>();
                    //获取拥有权限的菜单Id集合
                    rolesMenuIdList.AddRange(resourceList.Select(r => r.TargetId.ToLong()).ToList());
                    //获取我的菜单列表
                    myMenus = allMenuList.Where(it => rolesMenuIdList.Contains(it.Id)).ToList();
                }

                // 对获取到的角色对应的菜单列表进行处理，获取父列表
                var parentList = GetMyParentMenus(allMenuList, myMenus);
                myMenus.AddRange(parentList);//合并列表

                myMenus.AddRange(allSpaList.OrderBy(it => it.SortCode).FirstOrDefault());//但也添加到菜单

                //构建菜单树
                result = myMenus.ResourceListToTree();
            }
            return result;
        }

        /// <inheritdoc/>
        [OperDesc("修改密码")]
        public async Task EditPassword(PasswordInfoInput input)
        {
            var password = CryptogramUtil.Sm4Encrypt(input.ConfirmPassword);
            var user = await _userService.GetUsertById(input.Id);
            if (user.Password != input.OldPassword)
                throw Oops.Bah("旧密码不正确");

            if (await UpdateAsync(it => new SysUser { Password = password }, it => it.Id == input.Id))
            {
                //从列表中删除
                _sysCacheService.SetVerificatId(input.Id, new());
                _userService.DeleteUserFromCache(input.Id);//从cache删除用户信息
            }
        }

        /// <inheritdoc />
        public async Task<List<long>> GetLoginWorkbench()
        {
            //获取个人工作台信息
            var sysRelation = await _relationService.GetWorkbench(UserManager.UserId);
            if (sysRelation != null)
            {
                //如果有数据直接返回个人工作台
                return sysRelation.ExtJson.ToJsonEntity<List<long>>();
            }
            else
            {
                return new();
            }
        }


        /// <inheritdoc />
        public async Task UpdateWorkbench(List<long> input)
        {
            //关系表保存个人工作台
            await _relationService.SaveRelation(CateGoryConst.Relation_SYS_USER_WORKBENCH_DATA, UserManager.UserId, null, input.ToJson(), true);
        }

        /// <inheritdoc />
        [OperDesc("用户更新个人信息")]
        public async Task UpdateUserInfo(UpdateInfoInput input)
        {
            var newInput = input.Adapt<UpdateInfoInput>();
            //如果手机号不是空
            if (!string.IsNullOrEmpty(newInput.Phone))
            {
                if (!newInput.Phone.MatchPhoneNumber())//判断是否是手机号格式
                    throw Oops.Bah($"手机号码格式错误");
                newInput.Phone = CryptogramUtil.Sm4Encrypt(newInput.Phone);
                var any = await IsAnyAsync(it => it.Phone == newInput.Phone && it.Id != UserManager.UserId);//判断是否有重复的
                if (any)
                    throw Oops.Bah($"系统已存在该手机号");
            }
            if (!string.IsNullOrEmpty(newInput.Email))
            {
                var match = newInput.Email.MatchEmail();
                if (!match.isMatch)
                    throw Oops.Bah($"邮箱格式错误");
            }
            //更新指定字段
            var result = await UpdateAsync(it => new SysUser
            {
                Email = newInput.Email,
                Phone = newInput.Phone,
            }, it => it.Id == UserManager.UserId);
            if (result)
                _userService.DeleteUserFromCache(UserManager.UserId);//cache删除用户数据
        }

        #region 方法


        /// <summary>
        /// 获取父菜单集合，已过滤掉同时存在的父节点
        /// </summary>
        /// <param name="allMenuList">所有菜单列表</param>
        /// <param name="myMenus">我的菜单列表</param>
        /// <returns></returns>
        private List<SysResource> GetMyParentMenus(List<SysResource> allMenuList, List<SysResource> myMenus)
        {
            var parentList = new List<SysResource>();
            myMenus.ForEach(it =>
            {
                //找到父ID对应的菜单
                var parent = allMenuList.Where(r => r.Id == it.ParentId).FirstOrDefault();
                if (parent != null && !parentList.Contains(parent) && !myMenus.Contains(parent))//如果不为空且夫列表里没有
                {
                    parentList.Add(parent);//添加到父列表
                }
            });
            return parentList;
        }

        #endregion 方法
    }
}
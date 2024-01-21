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

using System.Text.RegularExpressions;

using ThingsGateway.Admin.Core.Utils;
using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Extension.Json;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IUserCenterService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class UserCenterService : DbRepository<SysUser>, IUserCenterService
{
    private readonly ISysUserService _userService;
    private readonly IRelationService _relationService;
    private readonly IResourceService _resourceService;
    private readonly IMenuService _menuService;
    private readonly IConfigService _configService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISimpleCacheService _simpleCacheService;

    public UserCenterService(ISysUserService userService,
        IRelationService relationService,
        IResourceService resourceService, ISimpleCacheService simpleCacheService,
        IMenuService menuService, IEventPublisher eventPublisher,
        IConfigService configService)
    {
        _userService = userService;
        _relationService = relationService;
        _resourceService = resourceService;
        _menuService = menuService;
        _configService = configService;
        _eventPublisher = eventPublisher;
        _simpleCacheService = simpleCacheService;
    }

    #region 查询

    /// <inheritdoc />
    public async Task<(List<SysResource> menuTree, List<SysResource> menu)> GetOwnMenuAsync(long userId)
    {
        var result = new List<SysResource>();
        //获取用户信息
        var userInfo = await _userService.GetUserByIdAsync(userId);
        if (userInfo != null)
        {
            //获取用户所拥有的资源集合
            var resourceList = await _relationService.GetRelationListByObjectIdAndCategoryAsync(userInfo.Id, CateGoryConst.Relation_SYS_USER_HAS_RESOURCE);
            if (resourceList.Count == 0)//如果没有就获取角色的
                //获取角色所拥有的资源集合
                resourceList = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(userInfo.RoleIdList,
                    CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);
            //定义菜单ID列表
            var menuIdList = new HashSet<long>();

            //获取菜单集合
            menuIdList.AddRange(resourceList.Select(r => r.TargetId.ToLong()).ToList());

            //获取所有的菜单和模块以及单页面列表，并按分类和排序码排序
            var allMenuAndSpaList = await _resourceService.GetMenuAndSpaListAsync();
            var allMenuList = new List<SysResource>();//菜单列表
            var allSpaList = new List<SysResource>();//单页列表
            //遍历菜单集合
            allMenuAndSpaList.ForEach(it =>
            {
                switch (it.Category)
                {
                    case CateGoryConst.Resource_MENU://菜单
                        allMenuList.Add(it);//添加到菜单列表
                        break;

                    case CateGoryConst.Resource_SPA://单页
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
                //获取我的菜单列表
                myMenus = allMenuList.Where(it => menuIdList.Contains(it.Id)).ToList();
            }
            // 对获取到的角色对应的菜单列表进行处理，获取父列表
            var parentList = GetMyParentMenus(allMenuList, myMenus);
            myMenus.AddRange(parentList);//合并列表

            myMenus.AddRange(allSpaList);//但也添加到菜单

            //构建菜单树
            result = _menuService.ConstructMenuTrees(myMenus);
            return (result, myMenus);
        }
        return (new(), new());
    }

    /// <inheritdoc />
    public async Task<RelationUserWorkBench> GetLoginWorkbenchAsync(long userId)
    {
        RelationUserWorkBench relationUserWorkBench = new();
        {
            //获取个人工作台信息
            var sysRelation = await _relationService.GetWorkbenchAsync(userId);
            if (sysRelation != null)
            {
                //如果有数据直接返回个人工作台
                relationUserWorkBench.Shortcut = sysRelation.ExtJson.ToLower().FromJsonString<List<long>>();
            }
            else
            {
                //如果没数据去系统配置里取默认的工作台
                var devConfig = await _configService.GetByConfigKeyAsync(CateGoryConst.Config_SYS_BASE, ConfigConst.SYS_DEFAULT_WORKBENCH_DATA);
                if (devConfig != null)
                {
                    relationUserWorkBench.Shortcut = devConfig.ConfigValue.ToLower().FromJsonString<List<long>>();//返回工作台信息
                }
                else
                {
                    return new();
                }
            }
        }
        {
            //获取个人主页信息
            var sysRelation = await _relationService.GetDefaultRazorAsync(userId);
            if (sysRelation != null)
            {
                //如果有数据直接返回个人主页
                relationUserWorkBench.DefaultRazpor = sysRelation.ExtJson.FromJsonString<long>();
            }
            else
            {
                //如果没数据去系统配置里取默认的主页
                var devConfig = await _configService.GetByConfigKeyAsync(CateGoryConst.Config_SYS_BASE, ConfigConst.SYS_DEFAULT_DEFAULT_RAZOR);
                if (devConfig != null)
                {
                    relationUserWorkBench.DefaultRazpor = devConfig.ConfigValue.ToLower().FromJsonString<long>();//返回主页信息
                }
                else
                {
                    return new();
                }
            }
        }
        return relationUserWorkBench;
    }

    #endregion 查询

    #region 编辑

    /// <inheritdoc />
    [OperDesc("更新个人信息")]
    public async Task UpdateUserInfoAsync(UpdateInfoInput input)
    {
        //如果手机号不是空
        if (!string.IsNullOrEmpty(input.Phone))
        {
            if (!input.Phone.MatchPhoneNumber())//判断是否是手机号格式
                throw Oops.Bah($"手机号码格式错误");
            input.Phone = CryptogramUtil.Sm2Encrypt(input.Phone);
            var any = await IsAnyAsync(it => it.Phone == input.Phone && it.Id != UserManager.UserId);//判断是否有重复的
            if (any)
                throw Oops.Bah($"系统已存在该手机号");
        }
        if (!string.IsNullOrEmpty(input.Email))
        {
            var match = input.Email.MatchEmail();
            if (!match.isMatch)
                throw Oops.Bah($"邮箱格式错误");
        }

        //更新指定字段
        var result = await UpdateSetColumnsTrueAsync(it => new SysUser
        {
            Email = input.Email,
            Phone = input.Phone,
        }, it => it.Id == UserManager.UserId);
        if (result)
            _userService.DeleteUserFromRedis(UserManager.UserId);//redis删除用户数据
    }

    /// <inheritdoc />
    [OperDesc("更新个人工作台")]
    public async Task UpdateWorkbenchAsync(UpdateWorkbenchInput input)
    {
        //关系表保存个人工作台
        await _relationService.SaveRelationAsync(CateGoryConst.Relation_SYS_USER_WORKBENCH_DATA, UserManager.UserId, null, input.WorkbenchData.ToJsonString(),
            true);
    }

    /// <inheritdoc />
    [OperDesc("更新个人主页")]
    public async Task UpdateDefaultRazorAsync(UpdateDefaultRazorInput input)
    {
        //关系表保存个人工作台
        await _relationService.SaveRelationAsync(CateGoryConst.Relation_SYS_USER_DEFAULT_RAZOR, UserManager.UserId, null, input.DefaultRazorData.ToJsonString(),
            true);
    }

    /// <inheritdoc />
    [OperDesc("修改密码")]
    public async Task UpdatePasswordAsync(UpdatePasswordInput input)
    {
        //获取用户信息
        var userInfo = await _userService.GetUserByIdAsync(UserManager.UserId);
        var password = CryptogramUtil.Sm2Decrypt(input.Password);//SM2解密
        if (userInfo.Password != password) throw Oops.Bah("原密码错误");
        var newPassword = CryptogramUtil.Sm2Decrypt(input.NewPassword);//sm2解密
        var loginPolicy = await _configService.GetListByCategoryAsync(CateGoryConst.Config_PWD_POLICY);//获取密码策略
        var containNumber = loginPolicy.First(it => it.ConfigKey == ConfigConst.PWD_CONTAIN_NUM).ConfigValue.ToBoolean();//是否包含数字
        var containLower = loginPolicy.First(it => it.ConfigKey == ConfigConst.PWD_CONTAIN_LOWER).ConfigValue.ToBoolean();//是否包含小写
        var containUpper = loginPolicy.First(it => it.ConfigKey == ConfigConst.PWD_CONTAIN_UPPER).ConfigValue.ToBoolean();//是否包含大写
        var containChar = loginPolicy.First(it => it.ConfigKey == ConfigConst.PWD_CONTAIN_CHARACTER).ConfigValue.ToBoolean();//是否包含特殊字符
        var minLength = loginPolicy.First(it => it.ConfigKey == ConfigConst.PWD_MIN_LENGTH).ConfigValue.ToInt();//最小长度
        if (minLength > newPassword.Length)
            throw Oops.Bah($"密码长度不能小于{minLength}");
        if (containNumber && !Regex.IsMatch(newPassword, "[0-9]"))
            throw Oops.Bah($"密码必须包含数字");
        if (containLower && !Regex.IsMatch(newPassword, "[a-z]"))
            throw Oops.Bah($"密码必须包含小写字母");
        if (containUpper && !Regex.IsMatch(newPassword, "[A-Z]"))
            throw Oops.Bah($"密码必须包含大写字母");
        if (containChar && !Regex.IsMatch(newPassword, "[~!@#$%^&*()_+`\\-={}|\\[\\]:\";'<>?,./]"))
            throw Oops.Bah($"密码必须包含特殊字符");
        // var similarity = PwdUtil.Similarity(password, newPassword);
        // if (similarity > 80)
        //     throw Oops.Bah($"新密码请勿与旧密码过于相似");
        newPassword = CryptogramUtil.Sm2Encrypt(newPassword);//SM4加密
        await UpdateSetColumnsTrueAsync(it => new SysUser() { Password = newPassword }, it => it.Id == userInfo.Id);
        _userService.DeleteUserFromRedis(input.Id);//redis删除用户数据

        //将这些用户踢下线，并永久注销这些用户
        var verificatInfos = UserTokenCacheUtil.HashGetOne(input.Id);
        await UserLoginOut(input.Id, verificatInfos);
        //从列表中删除
        UserTokenCacheUtil.HashDel(input.Id);
    }

    #endregion 编辑

    #region 方法

    /// <summary>
    /// 通知用户下线
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="verificatInfos">Token列表</param>
    private async Task UserLoginOut(long userId, List<VerificatInfo> verificatInfos)
    {
        await _eventPublisher.PublishAsync(EventSubscriberConst.UserLoginOut, new UserLoginOutEvent
        {
            Message = "您的账号已在别处登录!",
            VerificatInfos = verificatInfos,
            UserId = userId
        });//通知用户下线
    }

    /// <summary>
    /// 获取父菜单集合
    /// </summary>
    /// <param name="allMenuList">所有菜单列表</param>
    /// <param name="myMenus">我的菜单列表</param>
    /// <returns></returns>
    private List<SysResource> GetMyParentMenus(List<SysResource> allMenuList, List<SysResource> myMenus)
    {
        var parentList = new List<SysResource>();
        myMenus.ForEach(it =>
        {
            var parents = _resourceService.GetResourceParent(allMenuList, it.ParentId!.Value);//获取父级
            parents.ForEach(parent =>
            {
                if (parent != null && !parentList.Contains(parent) && !myMenus.Contains(parent))//如果不为空且两个列表里没有
                {
                    parentList.Add(parent);//添加到父列表
                }
            });
        });
        return parentList;
    }

    #endregion 方法
}
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

using Furion.DataEncryption;
using Furion.DependencyInjection;
using Furion.FriendlyException;

using Mapster;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IUserCenterService"/>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class UserCenterService : DbRepository<SysUser>, IUserCenterService
{
    private readonly IRelationService _relationService;
    private readonly IResourceService _resourceService;
    private readonly ISysUserService _userService;
    private readonly IVerificatService _verificatService;

    /// <inheritdoc cref="IUserCenterService"/>
    public UserCenterService(ISysUserService userService,
                             IRelationService relationService,
                             IResourceService resourceService,
                             IVerificatService verificatService
                             )
    {
        _userService = userService;
        _relationService = relationService;
        _resourceService = resourceService;
        _verificatService = verificatService;
    }

    /// <inheritdoc/>
    [OperDesc("修改密码")]
    public async Task EditPasswordAsync(PasswordInfoInput input)
    {
        var password = DESCEncryption.Encrypt(input.ConfirmPassword, DESCKeyConst.DESCKey);
        var user = await _userService.GetUserByIdAsync(input.Id);
        if (user.Password != input.OldPassword)
            throw Oops.Bah("旧密码不正确");

        if (await UpdateAsync(it => new SysUser { Password = password }, it => it.Id == input.Id))
        {
            //从列表中删除
            await _verificatService.SetVerificatIdAsync(input.Id, new());
            _userService.DeleteUserFromCache(input.Id);//从cache删除用户信息
        }
    }

    /// <inheritdoc />
    public async Task<List<long>> GetLoginWorkbenchAsync()
    {
        //获取个人工作台信息
        var sysRelation = await _relationService.GetWorkbenchAsync(UserManager.UserId);
        if (sysRelation != null)
        {
            //如果有数据直接返回个人工作台
            return sysRelation.ExtJson.FromJsonString<List<long>>();
        }
        else
        {
            return new();
        }
    }

    /// <inheritdoc />
    public async Task<long> GetLoginDefaultRazorAsync(long userId)
    {
        var sysRelations = await _relationService.GetRelationByCategoryAsync(CateGoryConst.Relation_SYS_USER_DEFAULTRAZOR);
        var result = sysRelations.FirstOrDefault(it => it.ObjectId == userId);//获取个人工作台
        if (result != null)
            return result.ExtJson.FromJsonString<long>();
        else
            return 0;
    }

    /// <inheritdoc />
    public async Task UpdateUserDefaultRazorAsync(long userId, long defaultRazor)
    {
        await _relationService.SaveRelationAsync(CateGoryConst.Relation_SYS_USER_DEFAULTRAZOR, userId, null, defaultRazor.ToJsonString(), true);
    }

    /// <inheritdoc />
    public async Task<List<SysResource>> GetOwnMenuAsync(string UserAccount = null)
    {
        var result = new List<SysResource>();
        //获取用户信息
        var userInfo = await _userService.GetUserByAccountAsync(UserAccount ?? UserManager.UserAccount);
        if (userInfo != null)
        {
            //获取所有的菜单和模块和菜单目录以及单页面列表，并按分类和排序码排序
            var allMenuAndSpaList = await _resourceService.GetaMenuAndSpaListAsync();
            List<SysResource> allMenuList = new();//菜单列表
            List<SysResource> allSpaList = new();//单页列表
            //遍历菜单集合
            allMenuAndSpaList.ForEach(it =>
            {
                switch (it.Category)
                {
                    case ResourceCategoryEnum.MENU://菜单
                        allMenuList.Add(it);//添加到菜单列表
                        break;

                    case ResourceCategoryEnum.SPA://单页
                        allSpaList.Add(it);//添加到单页列表
                        break;
                }
            });
            //输出的用户权限菜单
            List<SysResource> myMenus = new();
            //管理员拥有全部权限
            if (UserManager.IsSuperAdmin)
            {
                myMenus = allMenuList;
            }
            else
            {
                //获取角色所拥有的资源集合
                var resourceList = await _relationService.GetRelationListByObjectIdListAndCategoryAsync(userInfo.RoleIdList, CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);
                //定义菜单ID列表
                HashSet<long> rolesMenuIdList = new();
                //获取拥有权限的菜单Id集合
                rolesMenuIdList.AddRange(resourceList.Select(r => r.TargetId.ToLong()).ToList());
                //获取我的菜单列表
                myMenus = allMenuList.Where(it => rolesMenuIdList.Contains(it.Id)).ToList();
            }

            // 对获取到的角色对应的菜单列表进行处理，获取父列表
            var parentList = GetMyParentMenus(allMenuList, myMenus);
            myMenus.AddRange(parentList);//合并列表

            myMenus.Add(allSpaList.OrderBy(it => it.SortCode).FirstOrDefault());//第一个SPA会添加到菜单作为首页

            //构建菜单树
            result = _resourceService.ResourceListToTree(myMenus);
        }
        return result;
    }

    /// <inheritdoc />
    [OperDesc("用户更新个人信息")]
    public async Task UpdateUserInfoAsync(UpdateInfoInput input)
    {
        var newInput = input.Adapt<UpdateInfoInput>();
        //如果手机号不是空
        if (!string.IsNullOrEmpty(newInput.Phone))
        {
            if (!newInput.Phone.MatchPhoneNumber())//判断是否是手机号格式
                throw Oops.Bah($"手机号码格式错误");
            newInput.Phone = DESCEncryption.Encrypt(newInput.Phone, DESCKeyConst.DESCKey);
            var any = await IsAnyAsync(it => it.Phone == newInput.Phone && it.Id != UserManager.UserId);//判断是否有重复的
            if (any)
                throw Oops.Bah($"系统已存在该手机号");
        }
        if (!string.IsNullOrEmpty(newInput.Email))
        {
            var match = newInput.Email.MatchEmail();
            if (!match)
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

    /// <inheritdoc />
    [OperDesc("用户更新工作台信息")]
    public async Task UpdateWorkbenchAsync(List<long> input)
    {
        //关系表保存个人工作台
        await _relationService.SaveRelationAsync(CateGoryConst.Relation_SYS_USER_WORKBENCH_DATA, UserManager.UserId, null, input.ToJsonString(), true);
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
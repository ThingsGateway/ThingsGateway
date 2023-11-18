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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 用户表种子数据
/// </summary>
public class SysUserSeedData : ISqlSugarEntitySeedData<SysUser>
{
    /// <inheritdoc/>
    public IEnumerable<SysUser> SeedData()
    {
        List<SysUser> configList = new List<SysUser>
        {
            new SysUser
            {
                Id = 212725263002001,
                Account = RoleConst.SuperAdmin,
                Password = "111111",
                UserEnable = true,
                SortCode = 1,
            },

            new SysUser
             {
                Id = 201725263002001,
                Account = "admin",
                Password = "111111",
                UserEnable = true,
                SortCode = 2,
            },
        };
        return configList;
    }
}
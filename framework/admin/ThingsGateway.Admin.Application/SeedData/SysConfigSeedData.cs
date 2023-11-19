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
/// 系统配置种子数据
/// </summary>
public class SysConfigSeedData : ISqlSugarEntitySeedData<SysConfig>
{
    /// <inheritdoc/>
    public IEnumerable<SysConfig> SeedData()
    {
        List<SysConfig> configList = new List<SysConfig>
        {
            new SysConfig
            {
                Id = 22222222222222,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_SWAGGER_NAME,
                ConfigValue = "admin",
                Remark = "swagger账号",
                SortCode = 1,
            },

            new SysConfig
            {
                Id = 22222222222223,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_SWAGGER_PASSWORD,
                ConfigValue = "123456",
                Remark = "swagger密码",
                SortCode = 2,
            },

            new SysConfig
            {
                Id = 22222222222224,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_SWAGGERLOGIN_OPEN,
                ConfigValue = "false",
                Remark = "swagger开启登录",
                SortCode = 3,
            },

            new SysConfig
            {
                Id = 22222222222226,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_TITLE,
                ConfigValue = "ThingsGateway",
                Remark = "标题",
                SortCode = 5,
            },

            new SysConfig
            {
                Id = 22222222222228,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_COPYRIGHT,
                ConfigValue = "ThingsGateway ©2023 Diego",
                Remark = "系统版权",
                SortCode = 6,
            },

            new SysConfig
            {
                Id = 22222222222299,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_COPYRIGHT_URL,
                ConfigValue = "https://gitee.com/diego2098/ThingsGateway",
                Remark = "系统版权链接地址",
                SortCode = 7,
            },
                        new SysConfig
            {
                Id = 22222222222229,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_PAGETAB,
                ConfigValue = "true",
                Remark = "开启标签页",
                SortCode = 7,
            },
            new SysConfig
            {
                Id = 22222222222231,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_PASSWORD,
                ConfigValue = "111111",
                Remark = "默认用户密码",
                SortCode = 8,
            },

            new SysConfig
            {
                Id = 22222222222227,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_VERIFICAT_EXPIRES,
                ConfigValue = "14400",
                Remark = "Verificat过期时间(分)",
                SortCode = 9,
            },

            new SysConfig
            {
                Id = 22222222222232,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_SINGLE_OPEN,
                ConfigValue = "false",
                Remark = "单用户登录开关",
                SortCode = 10,
            },

            new SysConfig
            {
                Id = 22222222222230,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_CAPTCHA_OPEN,
                ConfigValue = "true",
                Remark = "登录验证码开关",
                SortCode = 11,
            },

            new SysConfig
            {
                Id = 22222222222225,
                Category = ConfigConst.SYS_CONFIGBASEDEFAULT,
                ConfigKey = ConfigConst.CONFIG_REMARK,
                ConfigValue = "边缘采集网关",
                Remark = "说明",
                SortCode = 12,
            }
        };
        return configList;
    }
}
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
    /// 添加配置参数
    /// </summary>
    public class ConfigAddInput : DevConfig
    {
        /// <summary>
        /// 分类
        /// </summary>
        [Required(ErrorMessage = "Category不能为空")]
        public override string Category { get; set; } = CateGoryConst.Config_CUSTOM_DEFINE;

        /// <summary>
        /// 配置键
        /// </summary>
        [Required(ErrorMessage = "configKey不能为空")]
        public override string ConfigKey { get; set; }

        /// <summary>
        /// 配置值
        /// </summary>

        [Required(ErrorMessage = "ConfigValue不能为空")]
        public override string ConfigValue { get; set; }
    }

    /// <summary>
    /// 编辑配置参数
    /// </summary>
    public class ConfigEditInput : ConfigAddInput
    {
        /// <summary>
        /// ID
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }

    /// <summary>
    /// 配置分页参数
    /// </summary>
    public class ConfigPageInput : BasePageInput
    {
        /// <summary>
        /// 分类
        /// </summary>
        [Description("分类")]
        public string Category { get; set; }
    }

    /// <summary>
    /// 删除配置参数
    /// </summary>
    public class ConfigDeleteInput : BaseIdInput
    {
        /// <summary>
        /// 分类
        /// </summary>
        [Required(ErrorMessage = "Category不能为空")]
        public string Category { get; set; } = CateGoryConst.Config_CUSTOM_DEFINE;
    }
}
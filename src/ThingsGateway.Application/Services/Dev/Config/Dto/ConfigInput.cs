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
        [Description("分类")]
        public string Category { get; set; }
    }

    /// <summary>
    /// 删除配置参数
    /// </summary>
    public class ConfigDeleteInput : BaseIdInput
    {
        [Required(ErrorMessage = "Category不能为空")]
        public string Category { get; set; } = CateGoryConst.Config_CUSTOM_DEFINE;
    }
}
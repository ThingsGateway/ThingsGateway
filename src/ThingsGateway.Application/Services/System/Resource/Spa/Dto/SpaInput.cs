namespace ThingsGateway.Application
{
    /// <summary>
    /// 单页输入参数
    /// </summary>
    public class SpaAddInput : SysResource
    {
        /// <summary>
        /// 路径
        /// </summary>
        [Required(ErrorMessage = "Component不能为空")]
        public override string Component { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Required(ErrorMessage = "Icon不能为空")]
        public override string Icon { get; set; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        public override TargetTypeEnum TargetType { get; set; } = TargetTypeEnum.SELF;

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "Title不能为空")]
        public override string Title { get; set; }
    }

    /// <summary>
    /// 单页输入参数
    /// </summary>
    public class SpaPageInput : BasePageInput
    {
        /// <summary>
        /// 菜单类型
        /// </summary>
        public TargetTypeEnum TargetType { get; set; }
    }

    /// <summary>
    /// 单页修改参数
    /// </summary>
    public class SpaEditInput : SpaAddInput
    {
        /// <summary>
        /// ID
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }
}
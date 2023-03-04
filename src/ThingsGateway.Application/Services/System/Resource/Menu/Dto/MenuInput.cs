namespace ThingsGateway.Application
{
    /// <summary>
    /// 添加菜单参数
    /// </summary>
    public class MenuAddInput : SysResource, IValidatableObject
    {
        /// <summary>
        /// 路径
        /// </summary>
        public override string Component { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [Required(ErrorMessage = "Icon不能为空")]
        public override string Icon { get; set; }

        /// <summary>
        /// 父ID
        /// </summary>
        [Required(ErrorMessage = "ParentId不能为空")]
        public override long ParentId { get; set; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        public override TargetTypeEnum TargetType { get; set; } = TargetTypeEnum.SELF;

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "Title不能为空")]
        public override string Title { get; set; }

        /// <summary>
        /// 特殊验证
        /// </summary>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //如果菜单类型是菜单
            if (TargetType == TargetTypeEnum.SELF)
            {
                if (string.IsNullOrEmpty(Component))
                    yield return new ValidationResult("Component不能为空", new[] { nameof(Component) });
            }
            //设置分类为菜单
            Category = MenuCategoryEnum.MENU;
        }
    }

    /// <summary>
    /// 编辑菜单输入参数
    /// </summary>
    public class MenuEditInput : MenuAddInput
    {
        /// <summary>
        /// ID
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }

    /// <summary>
    /// 菜单树查询参数
    /// </summary>
    public class MenuPageInput : BasePageInput
    {
        /// <summary>
        /// 父ID
        /// </summary>
        [Required(ErrorMessage = "ParentId不能为空")]
        public long ParentId { get; set; }
    }
}
namespace ThingsGateway.Application
{
    /// <summary>
    /// 添加按钮参数
    /// </summary>
    public class ButtonAddInput : SysResource
    {
        /// <summary>
        /// 编码
        /// </summary>
        [Required(ErrorMessage = "Code不能为空")]
        public override string Code { get; set; }

        /// <summary>
        /// 父ID
        /// </summary>
        [Required(ErrorMessage = "ParentId不能为空")]
        public override long ParentId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "Title不能为空")]
        public override string Title { get; set; }
    }

    public class ButtonPageInput : BasePageInput
    {
        /// <summary>
        /// 父ID
        /// </summary>
        [Required(ErrorMessage = "ParentId不能为空")]
        public long? ParentId { get; set; }
    }

    public class ButtonEditInput : ButtonAddInput
    {
        /// <summary>
        /// ID
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }
}
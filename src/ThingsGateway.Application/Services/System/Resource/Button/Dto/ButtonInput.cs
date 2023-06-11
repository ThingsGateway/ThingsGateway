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
    /// <summary>
    /// 按钮分页
    /// </summary>
    public class ButtonPageInput : BasePageInput
    {
        /// <summary>
        /// 父ID
        /// </summary>
        [Required(ErrorMessage = "ParentId不能为空")]
        public long? ParentId { get; set; }
    }
    /// <summary>
    /// 按钮编辑
    /// </summary>
    public class ButtonEditInput : ButtonAddInput
    {
        /// <summary>
        /// ID
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }
}
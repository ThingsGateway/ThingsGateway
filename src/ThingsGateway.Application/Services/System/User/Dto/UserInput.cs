namespace ThingsGateway.Application
{
    /// <summary>
    /// 添加用户参数
    /// </summary>
    public class UserAddInput : SysUser
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Required(ErrorMessage = "账号不能为空"), MinLength(3, ErrorMessage = "账号不能少于4个字符")]
        public override string Account { get; set; }
    }

    /// <summary>
    /// 编辑用户参数
    /// </summary>
    public class UserEditInput : SysUser
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Required(ErrorMessage = "账号不能为空"), MinLength(3, ErrorMessage = "账号不能少于4个字符")]
        public override string Account { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }

    /// <summary>
    /// 用户分页查询参数
    /// </summary>
    public class UserPageInput : BasePageInput
    {
        /// <summary>
        /// 动态查询条件
        /// </summary>
        public Expressionable<SysUser> Expression { get; set; }
    }

    /// <summary>
    /// 用户授权角色参数
    /// </summary>
    public class UserGrantRoleInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long? Id { get; set; }

        /// <summary>
        /// 授权权限信息
        /// </summary>
        [Required(ErrorMessage = "RoleIdList不能为空")]
        public List<long> RoleIdList { get; set; }
    }
}
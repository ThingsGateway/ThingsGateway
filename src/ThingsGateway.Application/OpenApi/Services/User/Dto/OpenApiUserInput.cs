namespace ThingsGateway.Application
{
    public class OpenApiPermissionTreeSelector : ITree<OpenApiPermissionTreeSelector>
    {
        /// <summary>
        /// 接口描述
        /// </summary>
        [Description("Api说明")]
        public string ApiName { get; set; }

        /// <summary>
        /// 路由名称
        /// </summary>
        [Description("Api路径")]
        public string ApiRoute { get; set; }

        public List<OpenApiPermissionTreeSelector> Children { get; set; } = new();

        public long Id { get; set; }

        public long ParentId { get; set; }

        /// <summary>
        /// 权限名称
        /// </summary>
        [Description("权限名称")]
        public string PermissionName { get; set; }
    }

    /// <summary>
    /// 添加用户参数
    /// </summary>
    public class OpenApiUserAddInput : OpenApiUser
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Required(ErrorMessage = "账号不能为空"), MinLength(3, ErrorMessage = "账号不能少于4个字符")]
        public override string Account { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空"), MinLength(2, ErrorMessage = "密码不能少于3个字符")]
        public override string Password { get; set; }
    }

    /// <summary>
    /// 编辑用户参数
    /// </summary>
    public class OpenApiUserEditInput : OpenApiUser
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
        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空"), MinLength(2, ErrorMessage = "密码不能少于3个字符")]
        public override string Password { get; set; }
    }

    /// <summary>
    /// 用户分页查询参数
    /// </summary>
    public class OpenApiUserPageInput : BasePageInput
    {
        /// <summary>
        /// 动态查询条件
        /// </summary>
        public Expressionable<SysUser> Expression { get; set; }
    }

    /// <summary>
    /// 用户授权参数
    /// </summary>
    public class OpenApiUserGrantPermissionInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long? Id { get; set; }

        /// <summary>
        /// 授权权限信息
        /// </summary>
        [Required(ErrorMessage = "PermissionList不能为空")]
        public List<string> PermissionList { get; set; }
    }
}
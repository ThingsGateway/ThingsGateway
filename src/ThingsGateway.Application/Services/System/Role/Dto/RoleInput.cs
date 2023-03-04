namespace ThingsGateway.Application
{
    /// <summary>
    /// 角色授权资源参数
    /// </summary>
    public class GrantResourceInput : RoleOwnResourceOutput
    {
        /// <summary>
        /// 授权资源信息
        /// </summary>
        [Required(ErrorMessage = "GrantInfoList不能为空")]
        public override List<RelationRoleResuorce> GrantInfoList { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }

    /// <summary>
    /// 角色授权用户参数
    /// </summary>
    public class GrantUserInput
    {
        /// <summary>
        /// 授权权限信息
        /// </summary>
        [Required(ErrorMessage = "GrantInfoList不能为空")]
        public List<long> GrantInfoList { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        public long? Id { get; set; }
    }

    /// <summary>
    /// 角色添加参数
    /// </summary>
    public class RoleAddInput : SysRole
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required(ErrorMessage = "Name不能为空")]
        public override string Name { get; set; }
    }

    /// <summary>
    /// 角色编辑参数
    /// </summary>
    public class RoleEditInput : RoleAddInput
    {
        /// <summary>
        /// Id
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        public override long Id { get; set; }
    }

    /// <summary>
    /// 角色查询参数
    /// </summary>
    public class RolePageInput : BasePageInput
    {
    }
}
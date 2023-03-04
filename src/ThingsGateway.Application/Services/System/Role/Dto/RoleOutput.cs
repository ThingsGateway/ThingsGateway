namespace ThingsGateway.Application
{
    /// <summary>
    /// 角色拥有的资源输出
    /// </summary>
    public class RoleOwnResourceOutput
    {
        /// <summary>
        /// 已授权资源信息
        /// </summary>
        public virtual List<RelationRoleResuorce> GrantInfoList { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public virtual long Id { get; set; }
    }
}
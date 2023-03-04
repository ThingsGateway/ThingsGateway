namespace ThingsGateway.Core
{
    /// <summary>
    /// SYS_ROLE_HAS_RESOURCE
    /// 角色有哪些资源扩展
    /// </summary>
    public class RelationRoleResuorce
    {
        /// <summary>
        /// 按钮信息
        /// </summary>
        public List<long> ButtonInfo { get; set; } = new List<long>();

        /// <summary>
        /// 菜单ID
        /// </summary>
        public long MenuId { get; set; }
    }
}
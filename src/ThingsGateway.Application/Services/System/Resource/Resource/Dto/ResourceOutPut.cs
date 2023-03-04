namespace ThingsGateway.Application
{


    public class RoleGrantResourceButton
    {
        /// <summary>
        /// 按钮id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
    }

    /// <summary>
    /// 授权菜单类
    /// </summary>
    public class RoleGrantResourceMenu
    {
        /// <summary>
        /// 菜单下按钮集合
        /// </summary>
        public List<RoleGrantResourceButton> Button { get; set; } = new List<RoleGrantResourceButton>();

        /// <summary>
        /// 菜单id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 父id
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 父名称
        /// </summary>
        public string ParentName { get; set; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        public string Title { get; set; }
    }

    /// <summary>
    /// Blazor Server的组件路由内容
    /// </summary>
    public class PermissionTreeSelector
    {
        /// <summary>
        /// 路由名称
        /// </summary>
        public string ApiRoute { get; set; }
    }
}
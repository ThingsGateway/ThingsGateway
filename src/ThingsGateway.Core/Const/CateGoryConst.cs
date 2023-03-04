namespace ThingsGateway.Core
{
    /// <summary>
    /// 分类常量
    /// </summary>
    public class CateGoryConst
    {
        /// <summary>
        /// 系统配置
        /// </summary>
        public const string Config_SYS_BASE = "SYS_BASE";
        /// <summary>
        /// 业务定义
        /// </summary>
        public const string Config_CUSTOM_DEFINE = "CUSTOM_DEFINE";
        /// <summary>
        /// ThingsGatewayOpenApi
        /// </summary>
        public const string ThingsGatewayOpenApi = "ThingsGatewayOpenApi";

        /// <summary>
        /// ThingsGatewayCore
        /// </summary>
        public const string ThingsGatewayCore = "ThingsGatewayCore";

        /// <summary>
        /// 用户工作台数据
        /// </summary>
        public const string Relation_SYS_USER_WORKBENCH_DATA = "SYS_USER_WORKBENCH_DATA";

        #region 关系表

        /// <summary>
        /// 角色有哪些权限
        /// </summary>
        public const string Relation_SYS_ROLE_HAS_PERMISSION = "SYS_ROLE_HAS_PERMISSION";

        /// <summary>
        /// 角色有哪些资源
        /// </summary>
        public const string Relation_SYS_ROLE_HAS_RESOURCE = "SYS_ROLE_HAS_RESOURCE";

        /// <summary>
        /// 用户有哪些角色
        /// </summary>
        public const string Relation_SYS_USER_HAS_ROLE = "SYS_USER_HAS_ROLE";

        #endregion 关系表

        #region 日志表

        /// <summary>
        /// 登录
        /// </summary>
        public const string Log_LOGIN = "LOGIN";

        /// <summary>
        /// 登出
        /// </summary>
        public const string Log_LOGOUT = "LOGOUT";

        /// <summary>
        /// 登录
        /// </summary>
        public const string Log_OPENAPILOGIN = "OPENAPILOGIN";

        /// <summary>
        /// 登出
        /// </summary>
        public const string Log_OPENAPILOGOUT = "OPENAPILOGOUT";

        /// <summary>
        /// 操作
        /// </summary>
        public const string Log_OPENAPIOPERATE = "OPENAPIOPERATE";

        /// <summary>
        /// 操作
        /// </summary>
        public const string Log_OPERATE = "OPERATE";

        /// <summary>
        /// 操作
        /// </summary>
        public const string Log_REQMETHOD = "BLAZORSERVER";

        #endregion 日志表
    }
}
namespace ThingsGateway.Core
{
    /// <summary>
    /// Cache常量
    /// </summary>
    public class CacheConst
    {
        /// <summary>
        /// 登录验证码缓存Key
        /// </summary>
        public const string Cache_Captcha = Cache_Prefix_Web + "Captcha";

        /// <summary>
        /// 系统配置表缓存Key
        /// </summary>
        public const string Cache_DevConfig = Cache_Prefix_Web + "DevConfig";

        /// <summary>
        /// Cache Key前缀(可删除)
        /// </summary>
        public const string Cache_Prefix_Web = "ThingsGateway:";

        /// <summary>
        /// 关系表缓存Key
        /// </summary>
        public const string Cache_SysRelation = Cache_Prefix_Web + "SysRelation:";

        /// <summary>
        /// 资源表缓存Key
        /// </summary>
        public const string Cache_SysResource = Cache_Prefix_Web + "SysResource:";

        /// <summary>
        /// 角色表缓存Key
        /// </summary>
        public const string Cache_SysRole = Cache_Prefix_Web + "SysRole";

        /// <summary>
        /// 用户表缓存Key
        /// </summary>
        public const string Cache_SysUser = Cache_Prefix_Web + "SysUser";

        /// <summary>
        /// 用户账户关系缓存Key
        /// </summary>
        public const string Cache_SysUserAccount = Cache_Prefix_Web + "SysUserAccount";

        /// <summary>
        /// UserId缓存Key
        /// </summary>
        public const string Cache_UserId = Cache_Prefix_Web + "UserId";

        #region OpenApi

        /// <summary>
        /// OpenApi用户表缓存Key
        /// </summary>
        public const string Cache_OpenApiUser = Cache_Prefix_Web + "OpenApiUser";

        /// <summary>
        /// OpenApi关系缓存Key
        /// </summary>
        public const string Cache_OpenApiUserAccount = Cache_Prefix_Web + "OpenApiUserAccount";

        /// <summary>
        /// OpenApiUserId缓存Key
        /// </summary>
        public const string Cache_OpenApiUserId = Cache_Prefix_Web + "OpenApiUserId";

        /// <summary>
        /// UserVerificat缓存Key
        /// </summary>
        public const string Cache_OpenApiUserVerificat = Cache_Prefix_Web + "OpenApiUserVerificat";

        /// <summary>
        /// UserVerificat缓存Key
        /// </summary>
        public const string Cache_UserVerificat = Cache_Prefix_Web + "UserVerificat";

        /// <summary>
        /// Swagger登录缓存Key
        /// </summary>
        public const string SwaggerLogin = Cache_Prefix_Web + "SwaggerLogin";

        #endregion OpenApi
    }
}
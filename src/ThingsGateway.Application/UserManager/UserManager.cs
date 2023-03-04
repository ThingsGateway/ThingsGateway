namespace ThingsGateway.Application
{
    /// <summary>
    /// 当前登录用户信息
    /// </summary>
    public class UserManager
    {
        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public static bool SuperAdmin => (App.User?.FindFirst(ClaimConst.IsSuperAdmin)?.Value).ToBoolean();

        /// <summary>
        /// 当前用户账号
        /// </summary>
        public static string UserAccount => App.User?.FindFirst(ClaimConst.Account)?.Value;

        /// <summary>
        /// 当前用户Id
        /// </summary>
        public static long UserId => (App.User?.FindFirst(ClaimConst.UserId)?.Value).ToLong();

        /// <summary>
        /// 当前VerificatId
        /// </summary>
        public static string VerificatId => App.User?.FindFirst(ClaimConst.VerificatId)?.Value;
    }
}
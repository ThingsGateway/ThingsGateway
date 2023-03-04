namespace ThingsGateway.Application
{
    /// <summary>
    /// 选择用户输出参数
    /// </summary>
    public class UserSelectorOutPut
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 用户信息输出
    /// </summary>
    public class UserInfoOutPut : SysUser
    {
    }
}
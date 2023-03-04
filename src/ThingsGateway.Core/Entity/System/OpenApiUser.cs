namespace ThingsGateway.Core
{
    [SugarTable("openapi_user")]
    [Tenant(SqlsugarConst.DB_Default)]
    public class OpenApiUser : BaseEntity
    {
        /// <summary>
        /// 账号
        ///</summary>
        [SugarColumn(ColumnName = "Account", ColumnDescription = "账号", Length = 200, IsNullable = false)]
        public virtual string Account { get; set; }

        /// <summary>
        /// 邮箱
        ///</summary>
        [SugarColumn(ColumnName = "Email", ColumnDescription = "邮箱", Length = 200, IsNullable = true)]
        public string Email { get; set; }

        /// <summary>
        /// 上次登录设备
        ///</summary>
        [SugarColumn(ColumnName = "LastLoginDevice", ColumnDescription = "上次登录设备", IsNullable = true)]
        public string LastLoginDevice { get; set; }

        /// <summary>
        /// 上次登录ip
        ///</summary>
        [SugarColumn(ColumnName = "LastLoginIp", ColumnDescription = "上次登录ip", Length = 200, IsNullable = true)]
        public string LastLoginIp { get; set; }

        /// <summary>
        /// 上次登录时间
        ///</summary>
        [SugarColumn(ColumnName = "LastLoginTime", ColumnDescription = "上次登录时间", IsNullable = true)]
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 最新登录设备
        ///</summary>
        [SugarColumn(ColumnName = "LatestLoginDevice", ColumnDescription = "最新登录设备", IsNullable = true)]
        public string LatestLoginDevice { get; set; }

        /// <summary>
        /// 最新登录ip
        ///</summary>
        [SugarColumn(ColumnName = "LatestLoginIp", ColumnDescription = "最新登录ip", Length = 200, IsNullable = true)]
        public string LatestLoginIp { get; set; }

        /// <summary>
        /// 最新登录时间
        ///</summary>
        [SugarColumn(ColumnName = "LatestLoginTime", ColumnDescription = "最新登录时间", IsNullable = true)]
        public DateTime? LatestLoginTime { get; set; }

        /// <summary>
        /// 密码
        ///</summary>
        [SugarColumn(ColumnName = "Password", ColumnDescription = "密码", Length = 200, IsNullable = false)]
        public virtual string Password { get; set; }

        /// <summary>
        /// 权限码集合
        /// </summary>
        [SugarColumn(ColumnName = "PermissionCodeList", ColumnDescription = "权限json", IsJson = true, IsNullable = true)]
        public List<string> PermissionCodeList { get; set; }

        /// <summary>
        /// 手机
        ///</summary>
        [SugarColumn(ColumnName = "Phone", ColumnDescription = "手机", Length = 200, IsNullable = true)]
        public string Phone { get; set; }

        /// <summary>
        /// 排序码
        ///</summary>
        [SugarColumn(ColumnName = "SortCode", ColumnDescription = "排序码", IsNullable = true)]
        public int SortCode { get; set; }

        /// <summary>
        /// 用户状态
        ///</summary>
        [SugarColumn(ColumnName = "UserStatus", ColumnDescription = "用户状态", Length = 200, IsNullable = true)]
        public bool UserStatus { get; set; }
    }
}
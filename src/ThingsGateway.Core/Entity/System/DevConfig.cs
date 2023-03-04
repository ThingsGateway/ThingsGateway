namespace ThingsGateway.Core
{
    /// <summary>
    /// 配置
    ///</summary>
    [SugarTable("dev_config", TableDescription = "配置")]
    [Tenant(SqlsugarConst.DB_CustomId)]
    public class DevConfig : BaseEntity
    {
        /// <summary>
        /// 分类
        ///</summary>
        [SugarColumn(ColumnName = "Category", ColumnDescription = "分类", Length = 200)]
        public virtual string Category { get; set; }

        /// <summary>
        /// 配置键
        ///</summary>
        [SugarColumn(ColumnName = "ConfigKey", ColumnDescription = "配置键", Length = 200)]
        public virtual string ConfigKey { get; set; }

        /// <summary>
        /// 配置值
        ///</summary>
        [SugarColumn(ColumnName = "ConfigValue", ColumnDescription = "配置值", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public virtual string ConfigValue { get; set; }

        /// <summary>
        /// 备注
        ///</summary>
        [SugarColumn(ColumnName = "Remark", ColumnDescription = "备注", Length = 200, IsNullable = true)]
        public string Remark { get; set; }

        /// <summary>
        /// 排序码
        ///</summary>
        [SugarColumn(ColumnName = "SortCode", ColumnDescription = "排序码", IsNullable = true)]
        public int SortCode { get; set; }
    }
}
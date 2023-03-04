namespace ThingsGateway.Core
{
    /// <summary>
    /// 系统关系表
    ///</summary>
    [SugarTable("sys_relation", TableDescription = "系统关系表")]
    [Tenant(SqlsugarConst.DB_Default)]
    public class SysRelation : PrimaryKeyEntity
    {
        /// <summary>
        /// 分类
        ///</summary>
        [SugarColumn(ColumnName = "Category", ColumnDescription = "分类", Length = 200, IsNullable = false)]
        public string Category { get; set; }

        /// <summary>
        /// 对象ID
        ///</summary>
        [SugarColumn(ColumnName = "ObjectId", ColumnDescription = "对象ID", IsNullable = false)]
        public long ObjectId { get; set; }

        /// <summary>
        /// 目标ID
        ///</summary>
        [SugarColumn(ColumnName = "TargetId", ColumnDescription = "目标ID", IsNullable = true)]
        public string TargetId { get; set; }
    }
}
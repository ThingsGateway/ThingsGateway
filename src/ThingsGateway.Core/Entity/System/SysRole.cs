namespace ThingsGateway.Core
{
    /// <summary>
    /// 角色
    ///</summary>
    [SugarTable("sys_role", TableDescription = "角色")]
    [Tenant(SqlsugarConst.DB_Default)]
    public class SysRole : PrimaryKeyEntity
    {
        /// <summary>
        /// 编码
        ///</summary>
        [SugarColumn(ColumnName = "Code", ColumnDescription = "编码", Length = 200, IsNullable = false)]
        public string Code { get; set; }

        /// <summary>
        /// 名称
        ///</summary>
        [SugarColumn(ColumnName = "Name", ColumnDescription = "名称", Length = 200, IsNullable = false)]
        public virtual string Name { get; set; }

        /// <summary>
        /// 排序码
        ///</summary>
        [SugarColumn(ColumnName = "SortCode", ColumnDescription = "排序码", IsNullable = true)]
        public int SortCode { get; set; }
    }
}
#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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
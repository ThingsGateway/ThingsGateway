#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Core
{
    /// <summary>
    /// 主键id基类
    /// </summary>
    public abstract class PrimaryIdEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
        public virtual long Id { get; set; }
    }

    /// <summary>
    /// 主键实体基类
    /// </summary>
    public abstract class PrimaryKeyEntity : PrimaryIdEntity
    {
        /// <summary>
        /// 拓展信息
        /// </summary>
        [SugarColumn(ColumnName = "ExtJson", ColumnDescription = "扩展信息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
        public virtual string ExtJson { get; set; }
    }

    /// <summary>
    /// 框架实体基类
    /// </summary>
    public class BaseEntity : PrimaryKeyEntity
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true, IsNullable = true)]
        public virtual DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [SugarColumn(ColumnDescription = "创建人", IsOnlyIgnoreUpdate = true, IsNullable = true)]
        public virtual string CreateUser { get; set; }

        /// <summary>
        /// 创建者Id
        /// </summary>
        [SugarColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true, IsNullable = true)]
        public virtual long? CreateUserId { get; set; }

        /// <summary>
        /// 软删除
        /// </summary>
        [SugarColumn(ColumnDescription = "软删除", IsNullable = true)]
        public virtual bool IsDelete { get; set; } = false;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "更新时间", IsOnlyIgnoreInsert = true, IsNullable = true)]
        public virtual DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [SugarColumn(ColumnDescription = "更新人", IsOnlyIgnoreInsert = true, IsNullable = true)]
        public virtual string UpdateUser { get; set; }

        /// <summary>
        /// 修改者Id
        /// </summary>
        [SugarColumn(ColumnDescription = "修改者Id", IsOnlyIgnoreInsert = true, IsNullable = true)]
        public virtual long? UpdateUserId { get; set; }
    }
}
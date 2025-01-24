// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Data;

namespace ThingsGateway.Extensions;

/// <summary>
///     <see cref="DataTable" /> 和 <see cref="DataSet" /> 拓展类
/// </summary>
internal static class DataTableAndSetExtensions
{
    /// <summary>
    ///     将 <see cref="DataTable" /> 转换为字典集合
    /// </summary>
    /// <param name="dataTable">
    ///     <see cref="DataTable" />
    /// </param>
    /// <returns>
    ///     <see cref="List{T}" />
    /// </returns>
    internal static List<Dictionary<string, object?>> ToDictionaryList(this DataTable dataTable)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(dataTable);

        return dataTable.AsEnumerable().Select(row =>
            row.Table.Columns.Cast<DataColumn>()
                .ToDictionary(col => col.ColumnName, col => row[col] != DBNull.Value ? row[col] : null)).ToList();
    }

    /// <summary>
    ///     将 <see cref="DataSet" /> 转换为字典集合
    /// </summary>
    /// <param name="dataSet">
    ///     <see cref="DataSet" />
    /// </param>
    /// <returns>
    ///     <see cref="Dictionary{TKey,TValue}" />
    /// </returns>
    internal static Dictionary<string, List<Dictionary<string, object?>>> ToDictionary(this DataSet dataSet)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(dataSet);

        return dataSet.Tables.Cast<DataTable>()
            .ToDictionary(table => table.TableName, table => table.ToDictionaryList());
    }
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

namespace ThingsGateway;

/// <summary>
/// SqlSugar配置
/// </summary>
public sealed class SqlSugarOption : ConnectionConfig
{
    /// <summary>
    /// 初始化数据
    /// </summary>
    public bool InitSeedData { get; set; } = false;

    /// <summary>
    /// 初始化表
    /// </summary>
    public bool InitTable { get; set; } = false;

    /// <summary>
    /// 是否控制台显示Sql语句
    /// </summary>
    public bool IsShowSql { get; set; }

    /// <summary>
    /// 更新数据
    /// </summary>
    public bool IsUpdateSeedData { get; set; } = false;
}

/// <summary>
/// SqlSugar配置
/// </summary>
public class SqlSugarOptions : List<SqlSugarOption>
{
}

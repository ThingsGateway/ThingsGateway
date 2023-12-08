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

using SqlSugar;
using SqlSugar.TDengine;

using System.ComponentModel;

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 历史数据表
/// </summary>
[SugarTable("historyValue")]
public class TDHistoryValue : STable
{
    /// <summary>
    /// 上传时间
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, InsertServerTime = true)]
    [Description("上传时间")]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    [Description("采集时间")]
    public DateTime CollectTime { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [Description("设备名称")]
    public string DeviceName { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [Description("变量名称")]
    public string Name { get; set; }

    /// <summary>
    /// 是否在线
    /// </summary>
    [Description("是否在线")]
    public bool IsOnline { get; set; }

    /// <summary>
    /// 变量值
    /// </summary>
    [Description("变量值")]
    [SugarColumn(Length = 18, DecimalDigits = 2)]
    public string Value { get; set; }
}
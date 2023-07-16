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

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 采集设备表
/// </summary>
[SugarTable("collectDevice", TableDescription = "采集设备表")]
[Tenant(SqlsugarConst.DB_CustomId)]
[SugarIndex("unique_collectdevice_name", nameof(CollectDevice.Name), OrderByType.Asc, true)]
public class CollectDevice : UploadDevice
{
    #region 冗余配置

    /// <summary>
    /// 是否冗余
    /// </summary>
    [SugarColumn(ColumnName = "IsRedundant", ColumnDescription = "是否冗余")]
    [OrderTable(Order = 2)]
    [Excel]
    public bool IsRedundant { get; set; }
    /// <summary>
    /// 冗余设备Id,只能选择相同驱动
    /// </summary>
    [SugarColumn(ColumnName = "RedundantDeviceId", ColumnDescription = "冗余设备Id")]
    public long RedundantDeviceId { get; set; }

    #endregion

}


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

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备通用表
/// </summary>
[SugarTable("collectdevice", TableDescription = "设备通用表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class CollectDevice : UploadDevice
{
    #region 冗余配置

    ///// <summary>
    ///// 冗余类型
    ///// </summary>
    //[SugarColumn(ColumnName = "RedundantEnum", ColumnDescription = "冗余类型")]
    //[OrderTable(Order = 2)]
    //[Excel]
    //public RedundantEnum RedundantEnum { get; set; }
    ///// <summary>
    ///// 主/冗余设备Id
    ///// </summary>
    //[SugarColumn(ColumnName = "RedundantDeviceId", ColumnDescription = "主/冗余设备Id")]
    //public long RedundantDeviceId { get; set; }

    #endregion

}


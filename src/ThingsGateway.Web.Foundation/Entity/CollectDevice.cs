using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备通用表
/// </summary>
[SugarTable("collectdevice", TableDescription = "设备通用表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class CollectDevice : UploadDevice
{


}


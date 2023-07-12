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

using ThingsGateway.Foundation;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Modbus;

public class ModbusServerProperty : UpDriverPropertyBase
{


    [DeviceProperty("IP", "")]
    public string IP { get; set; } = "";


    [DeviceProperty("端口", "")]
    public int Port { get; set; } = 502;

    [DeviceProperty("默认站号", "")]
    public byte Station { get; set; } = 1;
    [DeviceProperty("多站点", "")]
    public bool MulStation { get; set; } = true;
    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }
    [DeviceProperty("允许写入", "")]
    public bool DeviceRpcEnable { get; set; }
}

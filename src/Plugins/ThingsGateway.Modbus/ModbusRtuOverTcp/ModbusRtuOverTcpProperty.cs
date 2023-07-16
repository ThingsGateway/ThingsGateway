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

public class ModbusRtuOverTcpProperty : CollectDriverPropertyBase
{
    [DeviceProperty("IP", "")]
    public override string IP { get; set; } = "127.0.0.1";

    [DeviceProperty("端口", "")]
    public override int Port { get; set; } = 502;

    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }
    [DeviceProperty("默认站号", "")]
    public byte Station { get; set; } = 1;
    [DeviceProperty("连接超时时间", "")]
    public ushort ConnectTimeOut { get; set; } = 3000;
    [DeviceProperty("最大打包长度", "")]
    public ushort MaxPack { get; set; } = 100;
    [DeviceProperty("CRC检测", "")]
    public bool Crc16CheckEnable { get; set; } = true;
    [DeviceProperty("读写超时时间", "")]
    public ushort TimeOut { get; set; } = 3000;

    [DeviceProperty("帧前时间", "某些设备性能较弱，报文间需要间隔较长时间")]
    public int FrameTime { get; set; } = 0;
    [DeviceProperty("组包缓存超时", "某些设备性能较弱，报文间需要间隔较长时间，可以设置更长的组包缓存，默认1s")]
    public double CacheTimeout { get; set; } = 1;
    [DeviceProperty("共享链路", "")]
    public override bool IsShareChannel { get; set; } = false;
    public override ShareChannelEnum ShareChannel => ShareChannelEnum.TcpClient;
}

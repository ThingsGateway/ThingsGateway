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

namespace ThingsGateway.Plugin.Modbus;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusTcpServerProperty : UpDriverPropertyBase
{
    /// <summary>
    /// IP
    /// </summary>
    [DeviceProperty("IP", "")]
    public string IP { get; set; } = "";

    /// <summary>
    /// 端口
    /// </summary>
    [DeviceProperty("端口", "")]
    public int Port { get; set; } = 502;
    /// <summary>
    /// 默认站号
    /// </summary>
    [DeviceProperty("默认站号", "")]
    public byte Station { get; set; } = 1;
    /// <summary>
    /// 多站点
    /// </summary>
    [DeviceProperty("多站点", "")]
    public bool MulStation { get; set; } = true;
    /// <summary>
    /// 默认解析顺序
    /// </summary>
    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }
    /// <summary>
    /// 允许写入
    /// </summary>
    [DeviceProperty("允许写入", "")]
    public bool DeviceRpcEnable { get; set; }
    /// <summary>
    /// 组包缓存超时ms
    /// </summary>
    [DeviceProperty("组包缓存超时", "某些设备性能较弱，报文间需要间隔较长时间，可以设置更长的组包缓存，默认1s")]
    public int CacheTimeout { get; set; } = 1;
    /// <summary>
    /// 线程循环间隔
    /// </summary>
    [DeviceProperty("线程循环间隔", "最小10ms")]
    public int CycleInterval { get; set; } = 100;
}

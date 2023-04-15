namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 插件配置项
/// 使用<see cref="DevicePropertyAttribute"/>特性标识
/// <para></para>
/// 约定：
/// 如果需要密码输入，属性名称中需包含Password字符串
/// </summary>
public abstract class DriverPropertyBase
{
    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; }
}

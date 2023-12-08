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

namespace ThingsGateway.Gateway.Application;

public static class DriverBaseExtension
{
    /// <summary>
    /// 创建插件实例，并且根据设备属性设置实例
    /// </summary>
    /// <returns></returns>
    public static DriverBase CreatDriver(this DeviceRunTime deviceRunTime)
    {
        var driverPluginService = App.GetService<DriverPluginService>();
        var driver = driverPluginService.GetDriver(deviceRunTime.PluginName);
        //设置插件配置项
        driverPluginService.SetDriverProperties(driver, deviceRunTime.DevicePropertys);
        return driver;
    }

    /// <summary>
    /// 通道标识
    /// </summary>
    public static string GetChannelID(this DriverPropertyBase driverProperty)
    {
        var config = driverProperty;
        if (config.IsShareChannel)
        {
            switch (config.ShareChannel)
            {
                case ChannelEnum.SerialPortClient:
                    return config.PortName;

                case ChannelEnum.TcpClient:
                case ChannelEnum.UdpSession:
                    var a = new IPHost($"{config.IP}:{config.Port}");
                    return $"{config.ShareChannel}{a}";
            }
        }
        return null;
    }

    /// <summary>
    /// 获取设备的属性值
    /// </summary>
    public static DependencyProperty GetDevicePropertyValue(this DeviceRunTime collectDeviceRunTime, string propertyName)
    {
        if (collectDeviceRunTime == null)
            return null;
        return collectDeviceRunTime.DevicePropertys.FirstOrDefault(a => a.PropertyName == propertyName);
    }

    /// <summary>
    /// 获取变量的上传属性值
    /// </summary>
    /// <param name="variableRunTime">当前变量</param>
    /// <param name="deviceRunTime">对应上传设备Id</param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static DependencyProperty GetPropertyValue(this DeviceVariableRunTime variableRunTime, long id, string propertyName)
    {
        if (variableRunTime == null)
            return null;
        if (variableRunTime.VariablePropertys.ContainsKey(id))
        {
            var data = variableRunTime.VariablePropertys[id]
                .FirstOrDefault(a => a.PropertyName == propertyName);
            return data;
        }
        return null;
    }

    /// <summary>
    /// 共享通道类型
    /// </summary>
    public static ISenderClient GetShareChannel(this DriverPropertyBase driverProperty, TouchSocketConfig touchSocketConfig)
    {
        var config = driverProperty;
        if (config.IsShareChannel)
        {
            switch (config.ShareChannel)
            {
                case ChannelEnum.None:
                    break;

                case ChannelEnum.SerialPortClient:
                    var data = new SerialPortOption()
                    {
                        PortName = config.PortName,
                        BaudRate = config.BaudRate,
                        DataBits = config.DataBits,
                        Parity = config.Parity,
                        StopBits = config.StopBits,
                    };
                    touchSocketConfig.SetSerialPortOption(data);
                    var serialPortClient = new SerialPortClient();
                    (serialPortClient).Setup(touchSocketConfig);
                    return serialPortClient;

                case ChannelEnum.TcpClient:
                    touchSocketConfig.SetRemoteIPHost(new IPHost($"{config.IP}:{config.Port}"));
                    var tcpClient = new TcpClient();
                    (tcpClient).Setup(touchSocketConfig);
                    return tcpClient;

                case ChannelEnum.UdpSession:
                    touchSocketConfig.SetRemoteIPHost(new IPHost($"{config.IP}:{config.Port}"));
                    var udpSession = new UdpSession();
                    (udpSession).Setup(touchSocketConfig);
                    return udpSession;
            }
        }
        return null;
    }
}
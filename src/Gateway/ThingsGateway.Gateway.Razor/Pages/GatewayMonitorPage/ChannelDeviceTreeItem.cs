//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------




using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;
public enum ChannelDevicePluginTypeEnum
{
    PluginType,
    PluginName,
    Channel,
    Device
}
public class ChannelDeviceTreeItem : IEqualityComparer<ChannelDeviceTreeItem>
{
    public ChannelDevicePluginTypeEnum ChannelDevicePluginType { get; set; }

    public DeviceRuntime DeviceRuntime { get; set; }
    public ChannelRuntime ChannelRuntime { get; set; }
    public string PluginName { get; set; }
    public PluginTypeEnum? PluginType { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is ChannelDeviceTreeItem item)
        {
            if (ChannelDevicePluginType == item.ChannelDevicePluginType)
            {
                if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Device)
                {
                    return DeviceRuntime == item.DeviceRuntime;
                }
                else if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.PluginType)
                {
                    return PluginType == item.PluginType;

                }
                else if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Channel)
                {
                    return ChannelRuntime == item.ChannelRuntime;
                }
                else if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.PluginName)
                {
                    return PluginName == item.PluginName;
                }
            }

        }
        return false;

    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ChannelDevicePluginType, DeviceRuntime, ChannelRuntime, PluginName, PluginType);
    }
    public bool TryGetDeviceRuntime(out DeviceRuntime deviceRuntime)
    {
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Device && DeviceRuntime?.Id > 0)
        {
            deviceRuntime = DeviceRuntime;
            return true;
        }
        else
        {
            deviceRuntime = null;
            return false;
        }
    }
    public bool TryGetPluginName(out string pluginName)
    {
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.PluginName)
        {
            pluginName = PluginName;
            return true;
        }
        else
        {
            pluginName = default;
            return false;
        }
    }

    public bool TryGetPluginType(out PluginTypeEnum? pluginType)
    {
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.PluginType)
        {
            pluginType = PluginType;
            return true;
        }
        else
        {
            pluginType = default;
            return false;
        }
    }
    public bool TryGetChannelRuntime(out ChannelRuntime channelRuntime)
    {
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Channel && ChannelRuntime?.Id > 0)
        {
            channelRuntime = ChannelRuntime;
            return true;
        }
        else
        {
            channelRuntime = null;
            return false;
        }
    }


    public override string ToString()
    {
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Device)
        {
            return DeviceRuntime?.ToString();
        }
        else if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Channel)
        {
            return ChannelRuntime?.ToString();
        }
        else if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.PluginName)
        {
            return PluginName;
        }
        else if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.PluginType)
        {
            return PluginType.ToString();
        }
        return base.ToString();
    }

    public bool Equals(ChannelDeviceTreeItem? x, ChannelDeviceTreeItem? y)
    {
        return y.Equals(x);
    }

    public int GetHashCode([DisallowNull] ChannelDeviceTreeItem obj)
    {
        return HashCode.Combine(obj.ChannelDevicePluginType, obj.DeviceRuntime, obj.ChannelRuntime, obj.PluginName, obj.PluginType);
    }
}

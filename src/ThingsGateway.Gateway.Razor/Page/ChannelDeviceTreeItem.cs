//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

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
    public long Id { get; set; }
    public ChannelDevicePluginTypeEnum ChannelDevicePluginType { get; set; }

    public long DeviceRuntimeId { get; set; }

    public long ChannelRuntimeId { get; set; }
    public string PluginName { get; set; }
    public PluginTypeEnum? PluginType { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is ChannelDeviceTreeItem item)
        {
            return Equals(this, item);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ChannelDevicePluginType, DeviceRuntimeId, ChannelRuntimeId, PluginName, PluginType, Id);
    }

    public bool TryGetDeviceRuntime(out DeviceRuntime deviceRuntime)
    {
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Device && DeviceRuntimeId > 0)
        {
            if (GlobalData.TryGetDeviceRuntime(DeviceRuntimeId, out deviceRuntime))
            {
                return true;
            }
            if (DeviceRuntimeId == MemoryConst.MemoryDeviceId)
            {
                deviceRuntime = GlobalData.MemoryDeviceRuntime;
                return true;
            }
        }
        deviceRuntime = null;
        return false;
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
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Channel && ChannelRuntimeId > 0)
        {
            if (GlobalData.ReadOnlyIdChannels.TryGetValue(ChannelRuntimeId, out channelRuntime))
            {
                return true;
            }
        }
        channelRuntime = null;
        return false;
    }

    public override string ToString()
    {
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Device)
        {
            return TryGetDeviceRuntime(out var deviceRuntime) ? deviceRuntime?.ToString() : string.Empty;
        }
        else if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Channel)
        {
            return TryGetChannelRuntime(out var channelRuntime) ? channelRuntime?.ToString() : string.Empty;
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

    public bool Equals(ChannelDeviceTreeItem? x, ChannelDeviceTreeItem? item)
    {
        if (x.ChannelDevicePluginType != item.ChannelDevicePluginType)
            return false;

        switch (x.ChannelDevicePluginType)
        {
            case ChannelDevicePluginTypeEnum.Device:
                return x.DeviceRuntimeId == item.DeviceRuntimeId;
            case ChannelDevicePluginTypeEnum.PluginType:
                return x.PluginType == item.PluginType;
            case ChannelDevicePluginTypeEnum.Channel:
                return x.ChannelRuntimeId == item.ChannelRuntimeId;
            case ChannelDevicePluginTypeEnum.PluginName:
                return x.PluginName == item.PluginName;
            default:
                return false;
        }
    }

    public int GetHashCode([DisallowNull] ChannelDeviceTreeItem obj)
    {
        return HashCode.Combine(obj.ChannelDevicePluginType, obj.DeviceRuntimeId, obj.ChannelRuntimeId, obj.PluginName, obj.PluginType, obj.Id);
    }



    public static string ToJSString(ChannelDeviceTreeItem channelDeviceTreeItem)
    {
        return $"{channelDeviceTreeItem.ChannelDevicePluginType}.{channelDeviceTreeItem.DeviceRuntimeId}.{channelDeviceTreeItem.ChannelRuntimeId}.{channelDeviceTreeItem.PluginName}.{channelDeviceTreeItem.PluginType}";
    }


}

public struct ChannelDeviceTreeItemStruct
{
    public long Id { get; set; }
    public ChannelDevicePluginTypeEnum ChannelDevicePluginType { get; set; }

    public long DeviceRuntimeId { get; set; }

    public long ChannelRuntimeId { get; set; }
    public string PluginName { get; set; }
    public PluginTypeEnum? PluginType { get; set; }


    public bool TryGetDeviceRuntime(out DeviceRuntime deviceRuntime)
    {
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Device && DeviceRuntimeId > 0)
        {
            if (GlobalData.TryGetDeviceRuntime(DeviceRuntimeId, out deviceRuntime))
            {
                return true;
            }
        }
        deviceRuntime = null;
        return false;
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
        if (ChannelDevicePluginType == ChannelDevicePluginTypeEnum.Channel && ChannelRuntimeId > 0)
        {
            if (GlobalData.ReadOnlyIdChannels.TryGetValue(ChannelRuntimeId, out channelRuntime))
            {
                return true;
            }
        }
        channelRuntime = null;
        return false;
    }

    public static ChannelDeviceTreeItemStruct FromJSString(string jsString)
    {
        if (string.IsNullOrWhiteSpace(jsString))
            throw new ArgumentNullException(nameof(jsString));

        ReadOnlySpan<char> span = jsString.AsSpan();
        Span<Range> ranges = stackalloc Range[5];

        // 手动分割
        int partIndex = 0;
        int start = 0;
        while (partIndex < 4) // 只找前4个分隔符
        {
            int idx = span[start..].IndexOf('.');
            if (idx == -1)
                throw new FormatException($"Invalid format: expected 5 parts, got {partIndex + 1}");

            ranges[partIndex] = new Range(start, start + idx);
            start += idx + 1;
            partIndex++;
        }

        // 最后一段
        ranges[partIndex] = new Range(start, span.Length);

        // 校验段数
        if (partIndex != 4)
            throw new FormatException($"Invalid format: expected 5 parts, got {partIndex + 1}");

        var part0 = span[ranges[0]];
        var part1 = span[ranges[1]];
        var part2 = span[ranges[2]];
        var part3 = span[ranges[3]];
        var part4 = span[ranges[4]];

        // 解析 Enum 和 long
        if (!Enum.TryParse(part0, out ChannelDevicePluginTypeEnum pluginType))
            throw new FormatException($"Invalid {nameof(ChannelDevicePluginTypeEnum)}: {part0.ToString()}");

        if (!long.TryParse(part1, out long deviceRuntimeId))
            throw new FormatException($"Invalid DeviceRuntimeId: {part1.ToString()}");

        if (!long.TryParse(part2, out long channelRuntimeId))
            throw new FormatException($"Invalid ChannelRuntimeId: {part2.ToString()}");

        string pluginName = part3.ToString();

        PluginTypeEnum? parsedPluginType = null;
        if (!part4.IsEmpty)
        {
            if (!Enum.TryParse(part4, out PluginTypeEnum tmp))
                throw new FormatException($"Invalid {nameof(PluginTypeEnum)}: {part4.ToString()}");
            parsedPluginType = tmp;
        }

        return new ChannelDeviceTreeItemStruct
        {
            ChannelDevicePluginType = pluginType,
            DeviceRuntimeId = deviceRuntimeId,
            ChannelRuntimeId = channelRuntimeId,
            PluginName = pluginName,
            PluginType = parsedPluginType
        };
    }

}


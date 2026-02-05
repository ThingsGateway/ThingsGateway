//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Management.Razor;

public class ManagementTreeItem : IEquatable<ManagementTreeItem>
{
    public string Name { get; set; }

    public string ParentName { get; set; }
    public string Uri { get; set; }

    public bool Enable { get; set; }
    public bool IsServer { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is ManagementTreeItem item)
        {
            return Name == item.Name && ParentName == item.ParentName && IsServer == item.IsServer;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, ParentName, IsServer);
    }


    public override string ToString()
    {
        return $"{Name}[{Uri}]"; ;
    }

    public bool Equals(ManagementTreeItem? x, ManagementTreeItem? y)
    {
        if ((x == null && y != null) || (x != null && y == null))
        {
            return false;
        }
        return y.Name == x.Name && y.ParentName == x.ParentName && x.IsServer == y.IsServer;
    }

    public int GetHashCode([DisallowNull] ManagementTreeItem obj)
    {
        return HashCode.Combine(obj.Name, obj.ParentName, obj.IsServer);
    }

    public bool Equals(ManagementTreeItem? other)
    {
        return Equals(this, other);
    }
}

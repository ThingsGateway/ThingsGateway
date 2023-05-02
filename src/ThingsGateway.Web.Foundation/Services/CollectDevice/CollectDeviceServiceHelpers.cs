using System.Linq;

using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

/// <summary>
/// 采集设备帮助类
/// </summary>
public static class CollectDeviceServiceHelpers
{
    /// <summary>
    /// 获取设备树
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static List<DeviceTree> GetTree(this List<CollectDevice> data)
    {
        Dictionary<string, DeviceTree> trees = new();
        foreach (var item in data)
        {
            if (item.DeviceGroup.IsNullOrEmpty())
            {
                trees.Add(item.Name, new() { Name = item.Name, Childrens = null });
            }
            else
            {
                if (trees.ContainsKey(item.DeviceGroup))
                {
                    trees[item.DeviceGroup].Childrens.Add(new() { Name = item.Name, Childrens = null });
                }
                else
                {
                    trees.Add(item.DeviceGroup, new()
                    {
                        Name = item.DeviceGroup,
                        Childrens = new() { new() { Name = item.Name, Childrens = null } }
                    });
                }
            }
        }
        return trees.Values?.ToList();
    }
}
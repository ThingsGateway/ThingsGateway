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

/// <summary>
/// 采集设备帮助类
/// </summary>
public static class DeviceServiceExtensions
{
    /// <summary>
    /// 获取设备树
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static List<DeviceTree> GetTree(this IEnumerable<Device> data)
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
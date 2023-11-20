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

public static class DriverPluginServiceExtensions
{

    public static (string, string) GetFileNameAndTypeName(this string pluginName)
    {
        int lastIndex = pluginName.LastIndexOf('.'); // 查找最后一个 '.' 的索引
        if (lastIndex != -1 && lastIndex < pluginName.Length - 1)
        {
            string part1 = pluginName.Substring(0, lastIndex); // 获取子串直到最后一个 '.'
            string part2 = pluginName.Substring(lastIndex + 1); // 获取最后一个 '.' 后面的部分
            return (part1, part2);
        }
        else
        {
            return (DriverPluginService.DefaultKey, pluginName);
        }
    }
}
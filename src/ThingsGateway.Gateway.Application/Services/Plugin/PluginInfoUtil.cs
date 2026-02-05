//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class PluginInfoUtil
{
    public const string TempDirName = "TempGatewayPlugins";
    private static WaitLock _locker = new(nameof(PluginInfoUtil));
    /// <summary>
    /// 异步保存驱动程序信息。
    /// </summary>
    public static async Task SavePlugin(PluginAddInput plugin, IPluginPageService pluginPageService)
    {
        try
        {
            // 等待锁可用
            await _locker.WaitAsync().ConfigureAwait(false);
            var maxFileSize = 100 * 1024 * 1024; // 最大100MB
            string tempDir = TempDirName;
            // 获取主程序集文件名
            var mainFileName = Path.GetFileNameWithoutExtension(plugin.MainFile.Name);

            // 构建插件文件夹绝对路径
            string fullDir = PathHelper.CombinePathReplace(AppContext.BaseDirectory, tempDir, mainFileName);

            Directory.CreateDirectory(fullDir);

            PluginAddPathInput pluginAddPathInput = new();

            try
            {

                // 构建主程序集绝对路径
                var fullPath = PathHelper.CombinePathReplace(fullDir, plugin.MainFile.Name);

                // 获取主程序集文件流
                using (var stream = plugin.MainFile.OpenReadStream(maxFileSize))
                {
#pragma warning disable CA2000 // 丢失范围之前释放对象
                    FileStream fs = new(fullPath, FileMode.Create);
#pragma warning restore CA2000 // 丢失范围之前释放对象

                    await stream.CopyToAsync(fs).ConfigureAwait(false);
                    await fs.SafeDisposeAsync().ConfigureAwait(false);
                }
                pluginAddPathInput.MainFilePath = fullPath;

                foreach (var item in plugin.OtherFiles ?? new())
                {
                    // 获取附属文件流
                    using (var otherStream = item.OpenReadStream(maxFileSize))
                    {

                        var otherFullPath = $"{PathHelper.CombinePathReplace(fullDir, item.Name)}";
#pragma warning disable CA2000 // 丢失范围之前释放对象
                        FileStream otherFs = new(otherFullPath, FileMode.Create);
#pragma warning restore CA2000 // 丢失范围之前释放对象

                        await otherStream.CopyToAsync(otherFs).ConfigureAwait(false);
                        await otherFs.SafeDisposeAsync().ConfigureAwait(false);

                        pluginAddPathInput.OtherFilePaths.Add(otherFullPath);
                    }

                }


                await pluginPageService.SavePluginByPathAsync(pluginAddPathInput).ConfigureAwait(false);


            }
            finally
            {
                if (File.Exists(pluginAddPathInput.MainFilePath))
                {
                    File.Delete(pluginAddPathInput.MainFilePath);
                }
                foreach (var item in pluginAddPathInput.OtherFilePaths)
                {
                    if (File.Exists(item))
                    {
                        File.Delete(item);
                    }
                }
            }
        }
        finally
        {
            // 释放锁资源
            _locker.Release();
        }
    }

    /// <summary>
    /// 根据插件FullName获取插件主程序集名称和插件类型名称
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public static (string FileName, string TypeName) GetFileNameAndTypeName(string pluginName)
    {
        if (pluginName.IsNullOrWhiteSpace())
            return (string.Empty, string.Empty);
        // 查找最后一个 '.' 的索引
        int lastIndex = pluginName.LastIndexOf('.');

        // 如果找到了最后一个 '.'，并且它不是最后一个字符
        if (lastIndex != -1 && lastIndex < pluginName.Length - 1)
        {
            // 获取子串直到最后一个 '.'
            string part1 = pluginName.Substring(0, lastIndex);
            // 获取最后一个 '.' 后面的部分
            string part2 = pluginName.Substring(lastIndex + 1);
            return (part1, part2);
        }
        else
        {
            // 如果没有找到 '.'，或者 '.' 是最后一个字符，则返回默认的键和插件名称
            return (nameof(ThingsGateway), pluginName);
        }
    }

    /// <summary>
    /// 根据插件主程序集名称和插件类型名称获取插件FullName
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetFullName(string fileName, string name)
    {
        return string.IsNullOrEmpty(fileName) ? name : $"{fileName}.{name}";
    }


}

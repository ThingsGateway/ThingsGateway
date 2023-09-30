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

using System.Diagnostics;

using ThingsGateway.Foundation;

namespace ThingsGateway.Upgrade
{
    /// <summary>
    /// StartCommand.txt 中配置启动主程序的指令，比如
    /// 如果是直接启动：ThingsGateway.Web.Entry.exe
    /// 如果是WindowsService：Net Start ThingsGateway
    /// 如果是其他部署(linux，pm2等等)，填写对应的启动命令
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            //Thread.Sleep(15000);
            Thread.Sleep(5000);
            var path = args[0];
            if (Directory.Exists(path))
            {
                var data = DirectoryUtility.GetDirectories(path);
                if (data.Length == 0)
                {
                    ConsleError($"目录{path}下不存在文件");
                }
                else
                {
                    var oldFolder = $"{AppContext.BaseDirectory}FileTemp/ThingsGatewayOld/{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}";
                    try
                    {

                        //复制原文件，错误时可恢复
                        ConsleInfo($"正在备份原文件");
                        CopyDirectory(AppContext.BaseDirectory, oldFolder, new[] { "logs", "FileTemp" }, new[] { "ThingsGateway.Upgrade" });
                        ConsleInfo($"备份原文件成功");

                        //停止主程序进程

                        string filePath = $"{AppContext.BaseDirectory}ThingsGateway.Web.Entry";
                        // 查找正在运行的进程
                        var process = Process.GetProcesses().Where(p =>
                        {
                            try
                            {
                                if (p.ProcessName == "ThingsGateway.Web.Entry")
                                {
                                    return (Path.GetDirectoryName(p.MainModule.FileName) + Path.DirectorySeparatorChar).Equals(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase);
                                    //return true;
                                }
                                return false;
                            }
                            catch (Exception)
                            {
                                return false; // 进程访问被拒绝或没有主模块
                            }
                        })?.FirstOrDefault();
                        if (process != null)
                        {
                            var exit = process.WaitForExit(300000);
                            if (!exit)
                            {
                                ConsleError("无法终止主程序，更新过程停止");

                                Recovery(oldFolder);

                                Directory.Delete(oldFolder, true);
                                Directory.Delete(path, true);
                                return;
                            }
                        }
                        //程序已退出

                        //复制文件到主程序文件夹
                        ConsleInfo($"正在复制文件到主程序文件夹");
                        CopyDirectory(path, AppContext.BaseDirectory, new[] { "logs", "FileTemp" }, new[] { "ThingsGateway.Upgrade" });
                        ConsleInfo($"复制文件到主程序文件夹成功");

                        //尝试启动
                        var dataString = FileUtil.ReadFile($"{AppContext.BaseDirectory}StartCommand.txt");//读取文件
                        var startCommand = dataString;
                        StartCommand(startCommand);


                        Thread.Sleep(5000);
                        // 查找正在运行的进程
                        var process1 = Process.GetProcesses().Where(p =>
                        {
                            try
                            {
                                if (p.ProcessName == "ThingsGateway.Web.Entry")
                                {
                                    return (Path.GetDirectoryName(p.MainModule.FileName) + Path.DirectorySeparatorChar).Equals(AppContext.BaseDirectory, StringComparison.OrdinalIgnoreCase);
                                    //return true;
                                }
                                return false;
                            }
                            catch (Exception)
                            {
                                return false; // 进程访问被拒绝或没有主模块
                            }
                        })?.FirstOrDefault();
                        if (process1 != null)
                        {
                            //代表启动正常
                            ConsleInfo("更新成功");
                            Directory.Delete(oldFolder, true);
                            Directory.Delete(path, true);
                            return;
                        }
                        else
                        {
                            //恢复原文件
                            Recovery(oldFolder);
                            Directory.Delete(oldFolder, true);

                            Directory.Delete(path, true);

                        }

                    }
                    catch (Exception ex)
                    {
                        ConsleError(ex.ToString());

                        //恢复原文件
                        Recovery(oldFolder);
                        Directory.Delete(oldFolder, true);
                        Directory.Delete(path, true);

                    }


                }
            }
            else
            {
                ConsleError($"不存在目录{path}");
                //尝试启动
                var dataString1 = FileUtil.ReadFile($"{AppContext.BaseDirectory}StartCommand.txt");//读取文件
                var startCommand1 = dataString1;
                StartCommand(startCommand1);

            }

            Thread.Sleep(10000);
            Console.ReadLine();
        }

        private static void Recovery(string oldFolder)
        {
            ConsleInfo("更新失败，恢复原文件");
            CopyDirectory(oldFolder, AppContext.BaseDirectory, new[] { "logs", "FileTemp" }, new[] { "ThingsGateway.Upgrade" });
            //尝试启动
            var dataString1 = FileUtil.ReadFile($"{AppContext.BaseDirectory}StartCommand.txt");//读取文件
            var startCommand1 = dataString1;
            StartCommand(startCommand1);
        }

        /// <summary>
        /// 复制文件夹及文件
        /// </summary>
        /// <param name="sourceFolder">原文件路径</param>
        /// <param name="destFolder">目标文件路径</param>
        /// <param name="ignoreFolder">忽略文件夹</param>
        /// <param name="ignoreFiles">忽略文件</param>
        /// <returns></returns>
        public static void CopyDirectory(string sourceFolder, string destFolder, string[] ignoreFolder, string[] ignoreFiles)
        {
            //如果目标路径不存在,则创建目标路径
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            //得到原文件根目录下的所有文件
            var files = Directory.GetFiles(sourceFolder);
            foreach (var file in files)
            {
                if (ignoreFiles.Contains(Path.GetFileNameWithoutExtension(file)))
                { continue; }
                var name = Path.GetFileName(file);
                var dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);//复制文件
            }
            //得到原文件根目录下的所有文件夹
            var folders = Directory.GetDirectories(sourceFolder);
            foreach (var folder in folders)
            {
                if (ignoreFolder.Contains(Path.GetFileName(folder)))
                { continue; }
                var name = Path.GetFileName(folder);
                var dest = Path.Combine(destFolder, name);
                CopyDirectory(folder, dest, ignoreFolder, ignoreFiles);//构建目标路径,递归复制文件
            }
        }


        public static void ConsleInfo(string str)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(str);
        }

        public static void ConsleError(string str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
        }

        public static void StartCommand(string command)
        {
            // 创建进程对象
            Process process = new Process();
            // 设置进程启动信息
            process.StartInfo.FileName = GetCommandInterpreter();  // 获取当前平台的命令解释器
            process.StartInfo.Arguments = GetCommandArguments(command);  // 获取命令行参数
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WorkingDirectory = AppContext.BaseDirectory;
            // 启动进程
            process.Start();
        }

        // 获取当前平台的命令解释器
        static string GetCommandInterpreter()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                return "cmd.exe";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                return "/bin/bash";
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                return "/bin/bash";
            }

            throw new PlatformNotSupportedException("该平台不支持执行命令行。");
        }

        // 获取命令行参数
        static string GetCommandArguments(string command)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                return $"/c {command}";
            }
            else  // Linux 或 macOS
            {
                return $"-c \"{command}\"";
            }
        }

    }


}
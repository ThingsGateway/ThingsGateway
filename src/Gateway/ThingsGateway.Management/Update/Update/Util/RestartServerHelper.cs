//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Threading;
using ThingsGateway.Upgrade;

namespace ThingsGateway;

public static class RestartServerHelper
{
    public static void DeleteAndBackup()
    {
        //删除不必要的文件
        DeleteDelEx();
        //删除备份
        Delete(FileConst.BackupPath);
        Delete(FileConst.BackupDirPath);
        Delete(FileConst.UpgradePath);

        //备份原数据
        Backup();
    }

    public static bool ExtractUpdate()
    {
        var file = FileConst.UpgradePath;
        if (file.IsNullOrEmpty() || !File.Exists(file)) return false;
        // 解压更新程序包
        if (!file.EndsWithIgnoreCase(".zip", ".7z")) return false;

        var tmp = Path.GetTempPath().CombinePath(Path.GetFileNameWithoutExtension(file));
        file.AsFile().Extract(tmp, true);

        CopyAndReplace(tmp, AppContext.BaseDirectory);
        RestartServer();
        return true;
    }

    /// <summary>
    /// 重启服务
    /// </summary>
    public static void RestartServer()
    {
        var life = App.GetService<IHostApplicationLifetime>();
        life.StopApplication();
        var fileName = App.Configuration.GetSection("StartWay")?.Get<string>();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //重启
            WindowsRestart(fileName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            LinuxRestart(fileName);
        }
        Environment.Exit(Environment.ExitCode);
    }

    private static void LinuxRestart(string fileName)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (fileName.Equals("DOTNET", StringComparison.OrdinalIgnoreCase))
        {
            var process = Process.GetCurrentProcess();
            stringBuilder.AppendLine(ProcessHelper.GetCommandLine(process.Id));
        }
        else if (!fileName.IsNullOrEmpty() && File.Exists(fileName))
        {
            var data = File.ReadAllLines(fileName);
            foreach (var item in data)
            {
                stringBuilder.AppendLine(item);
            }
        }


        var cmd = $"#!/bin/bash{Environment.NewLine}{stringBuilder}";
        Process.Start(new ProcessStartInfo()
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{cmd.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = false
        });
    }

    private static void WindowsRestart(string fileName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            string restartBat = Path.Combine(AppContext.BaseDirectory, "Restart.bat");
            StringBuilder stringBuilder = new StringBuilder();
            if (fileName.Equals("DOTNET", StringComparison.OrdinalIgnoreCase))
            {
                var process = Process.GetCurrentProcess();
                stringBuilder.AppendLine(ProcessHelper.GetCommandLine(process.Id));
            }
            else if (!fileName.IsNullOrEmpty() && File.Exists(fileName))
            {
                var data = File.ReadAllLines(fileName);
                foreach (var item in data)
                {
                    stringBuilder.AppendLine(item);
                }
            }

            File.WriteAllText(restartBat, stringBuilder.ToString(), Encoding.Default);
            /*
            * 当前用户是管理员的时候，直接启动应用程序
            * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行
            */
            //获得当前登录的Windows用户标示
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = restartBat,
                //设置启动动作,确保以管理员身份运行
                Verb = "runas"
            };
            Process.Start(startInfo);
        }
    }

    private static void DeleteDelEx()
    {
        // 删除备份
        var di = new DirectoryInfo(AppContext.BaseDirectory);
        var fs = di.GetAllFiles("*.del", true);
        foreach (var item in fs)
        {
            try
            {
                item.Delete();
            }
            catch { }
        }
    }
    private static void Delete(string path)
    {
        try
        {
            // 删除备份
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch
        {

        }
    }

    /// <summary>拷贝并替换。正在使用锁定的文件不可删除，但可以改名</summary>
    /// <param name="source">源目录</param>
    /// <param name="dest">目标目录</param>
    private static void CopyAndReplace(String source, String dest)
    {
        var di = source.AsDirectory();
        var now = TimerX.Now.ToFileTime();

        // 来源目录根，用于截断
        var root = di.FullName.EnsureEnd(Path.DirectorySeparatorChar.ToString());
        foreach (var item in di.GetAllFiles(null, true))
        {
            var name = item.FullName.TrimStart(root);
            var dst = dest.CombinePath(name).GetBasePath();

            var configDirs = App.Configuration.GetSection("ConfigurationScanDirectories").Get<string[]>();
            if (configDirs.Any(a => a == item.DirectoryName))
            {
                //配置json文件不覆盖
                continue;
            }
            if (dst.EqualIgnoreCase("appsettings.json") || dst.EqualIgnoreCase("pm2.json"))
            {
                //配置json文件不覆盖
                continue;
            }

            // 拷贝覆盖
            try
            {
                item.CopyTo(dst.EnsureDirectory(true), true);
            }
            catch
            {
                try
                {
                    // 如果是exe/dll，则先改名，因为可能无法覆盖
                    if (File.Exists(dst))
                    {
                        var del = $"{dst}.{now}.del";
                        File.Move(dst, del);
                        item.CopyTo(dst, true);
                    }
                }
                catch
                {

                }
            }
        }

        // 删除临时目录
        di.Delete(true);
    }


    private static void Backup()
    {
        try
        {

            //备份原数据
            var backupDir = new DirectoryInfo(AppContext.BaseDirectory);
            backupDir.CopyTo(FileConst.BackupDirPath, allSub: true);
            FileConst.BackupDirPath.AsDirectory().Compress(FileConst.BackupPath);

        }
        catch
        {

        }
    }

}

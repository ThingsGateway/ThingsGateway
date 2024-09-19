//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using System.Diagnostics;
using System.Text.RegularExpressions;

using ThingsGateway.Extension;
using ThingsGateway.Foundation;
using ThingsGateway.Razor;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class LogConsole : IDisposable
{
    private bool IsPause;

    public bool Disposed { get; set; }

    [Parameter, EditorRequired]
    public ILog Logger { get; set; }

    [Parameter]
    public string HeaderText { get; set; } = "Log";

    [Parameter]
    public string HeightText { get; set; } = "400px";

    [Parameter, EditorRequired]
    public string LogPath { get; set; }

    /// <summary>
    /// 日志
    /// </summary>
    public ICollection<LogMessage> Messages { get; set; } = new List<LogMessage>();

    private ICollection<LogMessage> CurrentMessages => IsPause ? PauseMessagesText : Messages;

    [Inject]
    private DownloadService DownloadService { get; set; }

    /// <summary>
    /// 暂停缓存
    /// </summary>
    private ICollection<LogMessage> PauseMessagesText { get; set; } = new List<LogMessage>();

    [Inject]
    private IPlatformService PlatformService { get; set; }

    [Inject]
    private ToastService ToastService { get; set; }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    protected async Task ExecuteAsync()
    {
        try
        {
            if (LogPath != null)
            {
                var files = TextFileReader.GetFiles(LogPath);
                if (files == null || files.FirstOrDefault() == null || !files.FirstOrDefault().IsSuccess)
                {
                    Messages = new List<LogMessage>();
                    await Task.Delay(1000);
                }
                else
                {
                    await Task.Run(async () =>
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        var result = TextFileReader.LastLog(files.FirstOrDefault().FullName, 0);
                        if (result.IsSuccess)
                        {
                            Messages = result.Content.Where(a => a.LogLevel >= Logger?.LogLevel).Select(a => new LogMessage((int)a.LogLevel, $"{a.LogTime} - {a.Message}{(a.ExceptionString.IsNullOrWhiteSpace() ? null : $"{Environment.NewLine}{a.ExceptionString}")}")).ToList();
                        }
                        else
                        {
                            Messages = new List<LogMessage>();
                        }
                        sw.Stop();
                        if (sw.ElapsedMilliseconds > 500)
                        {
                            await Task.Delay(1000);
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
        }
    }

    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private async Task Delete()
    {
        if (LogPath != null)
        {
            var files = TextFileReader.GetFiles(LogPath);
            if (files == null || files.FirstOrDefault() == null || !files.FirstOrDefault().IsSuccess)
            {
            }
            else
            {
                foreach (var item in files)
                {
                    if (File.Exists(item.FullName))
                    {
                        int error = 0;
                        while (error < 3)
                        {
                            try
                            {
                                File.SetAttributes(item.FullName, FileAttributes.Normal);
                                File.Delete(item.FullName);
                                break;
                            }
                            catch
                            {
                                await Task.Delay(3000);
                                error++;
                            }
                        }
                    }
                }
            }
        }
    }

    private async Task HandleOnExportClick(MouseEventArgs args)
    {
        try
        {
            if (IsPause)
            {
                using var memoryStream = new MemoryStream();
                using StreamWriter writer = new(memoryStream);
                foreach (var item in PauseMessagesText)
                {
                    writer.WriteLine(item.Message);
                }
                writer.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);

                // 定义文件名称规则的正则表达式模式
                string pattern = @"[\\/:*?""<>|]";
                // 使用正则表达式将不符合规则的部分替换为下划线
                string sanitizedFileName = Regex.Replace(HeaderText, pattern, "_");
                await DownloadService.DownloadFromStreamAsync(sanitizedFileName + DateTime.Now.ToFileDateTimeFormat(), memoryStream);
            }
            else
            {
                if (PlatformService != null)
                    await PlatformService.OnLogExport(LogPath);
            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    private Task Pause()
    {
        IsPause = !IsPause;
        if (IsPause)
            PauseMessagesText = Messages.ToList();
        return Task.CompletedTask;
    }

    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                await ExecuteAsync();
                await InvokeAsync(StateHasChanged);
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }
}

public class LogMessage
{
    public LogMessage(int level, string message)
    {
        Level = level;
        Message = message;
    }

    public int Level { get; set; }
    public string Message { get; set; }
}

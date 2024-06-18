//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using NewLife.Extension;

using System.Diagnostics;
using System.Text.RegularExpressions;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation;
using ThingsGateway.Razor;

namespace ThingsGateway.Debug;

public partial class LogConsole : IDisposable
{
    [Parameter]
    [EditorRequired]
    public string HeaderText { get; set; }

    [Parameter]
    public string HeightText { get; set; } = "400px";

    private bool IsPause;

    private Task Pause()
    {
        IsPause = !IsPause;
        if (IsPause)
            PauseMessagesText = Messages.ToList();
        return Task.CompletedTask;
    }

    [Inject]
    private DownloadService DownloadService { get; set; }

    /// <summary>
    /// 日志
    /// </summary>
    public ICollection<LogMessage> Messages { get; set; } = new List<LogMessage>();

    /// <summary>
    /// 暂停缓存
    /// </summary>
    private ICollection<LogMessage> PauseMessagesText { get; set; } = new List<LogMessage>();

    private ICollection<LogMessage> CurrentMessages => IsPause ? PauseMessagesText : Messages;

    [Inject]
    private IPlatformService PlatformService { get; set; }

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

    [Inject]
    private ToastService ToastService { get; set; }

    [Parameter, EditorRequired]
    public string LogPath { get; set; }

    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    public bool Disposed { get; set; }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
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

    protected async Task ExecuteAsync()
    {
        try
        {
            if (LogPath != null)
            {
                var files = TextFileReader.GetFiles(LogPath);
                if (files == null || files.FirstOrDefault() == null || !files.FirstOrDefault().IsSuccess)
                {
                }
                else
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        var result = TextFileReader.LastLog(files.FirstOrDefault().FullName, 0);
                        if (result.IsSuccess)
                        {
                            Messages = result.Content.Select(a => new LogMessage((int)a.LogLevel, $"{a.LogTime} - {a.Message}{(a.ExceptionString.IsNullOrWhiteSpace() ? null : $"-{a.ExceptionString}")}")).ToList();
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
}

public class LogMessage
{
    public LogMessage(int level, string message)
    {
        this.Level = level;
        this.Message = message;
    }

    public int Level { get; set; }
    public string Message { get; set; }
}

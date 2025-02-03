//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Web;

using System.Diagnostics;
using System.Text.RegularExpressions;

using ThingsGateway.Extension;
using ThingsGateway.Foundation;
using ThingsGateway.NewLife;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class LogConsole : IDisposable
{
    private bool Pause;

    public bool Disposed { get; set; }

    [Parameter, EditorRequired]
    public LogLevel LogLevel { get; set; }

    [Parameter]
    public EventCallback<LogLevel> LogLevelChanged { get; set; }

    [Parameter, EditorRequired]
    public bool Enable { get; set; }
    [Parameter]
    public EventCallback<bool> EnableChanged { get; set; }
    [Parameter]
    public string CardStyle { get; set; } = "height: 100%;";
    [Parameter]
    public string HeaderText { get; set; } = "Log";

    [Parameter]
    public string HeightString { get; set; } = "calc(100% - 50px)";

    [Parameter, EditorRequired]
    public string LogPath { get; set; }

    /// <summary>
    /// 日志
    /// </summary>
    public ICollection<LogMessage> Messages { get; set; } = new List<LogMessage>();

    private ICollection<LogMessage> CurrentMessages => Pause ? PauseMessagesText : Messages;

    [Inject]
    private DownloadService DownloadService { get; set; }
    [Inject]
    private IStringLocalizer<ThingsGateway.Razor._Imports> RazorLocalizer { get; set; }

    /// <summary>
    /// 暂停缓存
    /// </summary>
    private ICollection<LogMessage> PauseMessagesText { get; set; } = new List<LogMessage>();

    [Inject]
    private IPlatformService PlatformService { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        Messages = new List<LogMessage>();
        await ExecuteAsync();
        await base.OnParametersSetAsync();
    }

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
                if (!files.IsSuccess)
                {
                    Messages = new List<LogMessage>();
                    await Task.Delay(1000);
                }
                else
                {
                    await Task.Run(async () =>
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        var result = TextFileReader.LastLog(files.Content.FirstOrDefault());
                        if (result.IsSuccess)
                        {
                            Messages = result.Content.Where(a => a.LogLevel >= LogLevel).Select(a => new LogMessage((int)a.LogLevel, $"{a.LogTime} - {a.Message}{(a.ExceptionString.IsNullOrWhiteSpace() ? null : $"{Environment.NewLine}{a.ExceptionString}")}")).ToList();
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
            NewLife.Log.XTrace.WriteException(ex);
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
            if (files.IsSuccess)
            {
                foreach (var item in files.Content)
                {
                    if (File.Exists(item))
                    {
                        int error = 0;
                        while (error < 3)
                        {
                            try
                            {
                                FileUtil.DeleteFile(item);
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
            if (Pause)
            {
                using var memoryStream = new MemoryStream();
                using StreamWriter writer = new(memoryStream);
                foreach (var item in PauseMessagesText)
                {
                    await writer.WriteLineAsync(item.Message);
                }
                await writer.FlushAsync();
                memoryStream.Seek(0, SeekOrigin.Begin);

                // 定义文件名称规则的正则表达式模式
                string pattern = @"[\\/:*?""<>|]";
                // 使用正则表达式将不符合规则的部分替换为下划线
                string sanitizedFileName = Regex.Replace(HeaderText, pattern, "_");
                await DownloadService.DownloadFromStreamAsync($"{sanitizedFileName}{DateTime.Now.ToFileDateTimeFormat()}.txt", memoryStream);
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
    private async Task OnEnable()
    {
        if (EnableChanged.HasDelegate)
        {
            Enable = !Enable;
            await EnableChanged.InvokeAsync(Enable);
        }
    }
    private Task OnPause()
    {
        Pause = !Pause;
        if (Pause)
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
                NewLife.Log.XTrace.WriteException(ex);
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

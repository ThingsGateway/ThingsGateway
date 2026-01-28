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

using System.Text;
using System.Text.RegularExpressions;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Common.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Debug;

public partial class LocalLogConsole : IDisposable
{
    private bool Pause;

    public bool Disposed { get; set; }

    [Parameter, EditorRequired]
    public LogLevel LogLevel { get; set; }

    [Parameter]
    public EventCallback<LogLevel> LogLevelChanged { get; set; }

    [Parameter]
    public string HeaderText { get; set; } = "Log";

    [Parameter]
    public string HeightString { get; set; } = "calc(100% - 300px)";

    [Parameter, EditorRequired]
    public string LogPath { get; set; }

    /// <summary>
    /// 日志
    /// </summary>
    public ICollection<LogData> Messages { get; set; } = new List<LogData>();

    private ICollection<LogData> CurrentMessages => Pause ? PauseMessagesText : Messages;

    [Inject]
    private DownloadService DownloadService { get; set; }
    [Inject]
    private IStringLocalizer<ThingsGateway.Razor._Imports> RazorLocalizer { get; set; }

    /// <summary>
    /// 暂停缓存
    /// </summary>
    private ICollection<LogData> PauseMessagesText { get; set; } = new List<LogData>();
    private string GetStringMessage(LogData itemMessage, bool slice = true)
    {
        using ValueStringBuilder valueStringBuilder = new();
        valueStringBuilder.Append(itemMessage.LogTime);
        valueStringBuilder.Append(' ');
        valueStringBuilder.Append('-');
        valueStringBuilder.Append(' ');
        valueStringBuilder.Append(itemMessage.Message);
        if (!string.IsNullOrWhiteSpace(itemMessage.ExceptionString))
        {
            valueStringBuilder.Append(Environment.NewLine);
            valueStringBuilder.Append(itemMessage.ExceptionString);
        }
        if (slice)
            return valueStringBuilder.AsSpan().Slice(0, Math.Min(valueStringBuilder.Length, 150)).ToString();
        else
            return valueStringBuilder.ToString();
    }
    [Inject]
    private PlatformService PlatformService { get; set; }

    [Inject]
    private IDownloadPlatformService DownloadPlatformService { get; set; }
    private string logPath;
    protected override async Task OnParametersSetAsync()
    {
        if (LogPath != logPath)
        {
            logPath = LogPath;
            Messages = new List<LogData>();
            await ExecuteAsync();
        }

        await base.OnParametersSetAsync();
    }

    [Inject]
    private ToastService ToastService { get; set; }
    [Inject]
    TextFileReadService TextFileReadService { get; set; }
    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
    private WaitLock WaitLock = new(nameof(LogConsole));
    protected async Task ExecuteAsync()
    {
        if (WaitLock.Waited) return;
        try
        {
            await WaitLock.WaitAsync();
            await Task.Delay(1000);

            if (LogPath != null)
            {
                var files = await TextFileReadService.GetLogFilesAsync(LogPath);
                if (!files.IsSuccess)
                {
                    Messages = new List<LogData>();
                    await Task.Delay(1000);
                }
                else
                {
                    await Task.Run(async () =>
                    {
                        var sw = ValueStopwatch.StartNew();
                        var result = await TextFileReadService.LastLogDataAsync(files.Content.FirstOrDefault(), LogLevel);
                        if (result.IsSuccess)
                        {
                            Messages = result.Content;
                        }
                        else
                        {
                            Messages = new List<LogData>();
                        }
                        if (sw.GetElapsedTime().TotalMilliseconds > 500)
                        {
                            await Task.Delay(1000);
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Foundation.Common.Log.XTrace.WriteException(ex);
        }
        finally
        {
            WaitLock.Release();
        }
    }

    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private async Task Delete()
    {
        await TextFileReadService.DeleteLogDataAsync(LogPath);
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
                    await writer.WriteLineAsync(GetStringMessage(item, false));
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
                if (DownloadPlatformService is HybridDownloadPlatformService)
                {
                    await DownloadPlatformService.DownloadFile([LogPath]);
                }
                else
                {
                    if (PlatformService != null)
                        await PlatformService.OnLogExport(LogPath);
                }

            }
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
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
            }
            catch (Exception ex)
            {
                Foundation.Common.Log.XTrace.WriteException(ex);
            }
        }
    }
}

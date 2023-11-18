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

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Foundation.Demo;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
/// <summary>
/// 调试UI
/// </summary>
public abstract class DriverDebugUIBase : ComponentBase, IDisposable
{
    /// <summary>
    /// 日志缓存
    /// </summary>
    public ConcurrentLinkedList<(LogLevel level, string message)> Messages = new();

    private PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));

    /// <inheritdoc/>
    ~DriverDebugUIBase()
    {
        this.SafeDispose();
    }

    /// <summary>
    /// 变量地址
    /// </summary>
    public virtual string Address { get; set; } = "40001";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [Inject]
    public InitTimezone InitTimezone { get; set; }

    /// <summary>
    /// 长度
    /// </summary>
    public virtual int Length { get; set; } = 1;

    /// <summary>
    /// 默认读写设备
    /// </summary>
    public virtual IReadWrite Plc { get; set; }
    /// <summary>
    /// 写入值
    /// </summary>
    public virtual string WriteValue { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    protected virtual DataTypeEnum DataTypeEnum { get; set; } = DataTypeEnum.Int16;
    /// <inheritdoc/>
    public virtual void Dispose()
    {
        _periodicTimer?.Dispose();
    }
    /// <inheritdoc/>
    public void LogOut(ThingsGateway.Foundation.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        Messages.Add(((LogLevel)logLevel,
            $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)} - {message} {exception}"));
        if (Messages.Count > 2500)
        {
            Messages.Clear();
        }
    }

    /// <inheritdoc/>
    public virtual async Task ReadAsync()
    {
        try
        {
            var data = await Plc.ReadAsync(Address, Length, DataTypeEnum);
            if (data.IsSuccess)
            {
                Messages.Add((LogLevel.Information,
            $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)} - 对应类型值：{Environment.NewLine}{data.Content.ToJsonString(true)} "));
            }
            else
            {
                Messages.Add((LogLevel.Warning,
            $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)} - {data.Message}"));
            }
        }
        catch (Exception ex)
        {
            Messages.Add((LogLevel.Error,
            $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)} - 错误：{ex}"));
        }

    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync()
    {
        try
        {
            var data = await Plc.WriteAsync(Address, WriteValue, Length, DataTypeEnum);
            if (data.IsSuccess)
            {
                Messages.Add((LogLevel.Information,
            $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)} - {data.Message}"));
            }
            else
            {
                Messages.Add((LogLevel.Warning,
            $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)} - {data.Message}"));
            }
        }
        catch (Exception ex)
        {
            Messages.Add((LogLevel.Error,
            $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(InitTimezone.TimezoneOffset)} - 写入前失败：{ex}"));
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }
    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(StateHasChanged);
        }
    }


}

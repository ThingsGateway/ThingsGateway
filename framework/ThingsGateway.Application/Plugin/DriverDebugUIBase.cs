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
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using ThingsGateway.Application.Extensions;
using ThingsGateway.Foundation;


namespace ThingsGateway.Application;

/// <summary>
/// 调试UI
/// </summary>
public abstract class DriverDebugUIBase : ComponentBase, IDisposable
{
    /// <summary>
    /// 导出提示
    /// </summary>
    public bool isDownExport;

    /// <summary>
    /// 日志缓存
    /// </summary>
    public ConcurrentLinkedList<(LogLevel level, string message)> Messages = new();

    IJSObjectReference _helper;
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));

    /// <summary>
    /// 默认读写设备
    /// </summary>
    public virtual IReadWriteDevice Plc { get; set; }


    /// <summary>
    /// 变量地址
    /// </summary>
    public virtual string Address { get; set; } = "40001";
    /// <summary>
    /// 数据类型
    /// </summary>
    protected virtual DataTypeEnum DataTypeEnum { get; set; } = DataTypeEnum.Int16;
    /// <inheritdoc/>
    [Inject]
    protected IJSRuntime JS { get; set; }

    /// <summary>
    /// 写入值
    /// </summary>
    public virtual string WriteValue { get; set; }
    [Inject]
    private ICollectDeviceService CollectDeviceService { get; set; }

    [Inject]
    private IVariableService VariableService { get; set; }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        _periodicTimer?.Dispose();
    }

    /// <inheritdoc/>
    public virtual async Task ReadAsync()
    {
        try
        {
            var data = await Plc.ReadAsync(Address, DataTypeEnum.GetSystemType());
            if (data.IsSuccess)
            {
                Messages.Add((LogLevel.Information, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - 对应类型值：" + data.Content));

            }
            else
            {
                Messages.Add((LogLevel.Error, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + data.Message));

            }
        }
        catch (Exception ex)
        {
            Messages.Add((LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + "错误：" + ex.Message));
        }

    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync()
    {
        try
        {
            var data = await Plc.WriteAsync(Address, DataTypeEnum.GetSystemType(), WriteValue);
            if (data.IsSuccess)
            {
                Messages.Add((LogLevel.Information, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + data.Message));
            }
            else
            {
                Messages.Add((LogLevel.Warning, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + data.Message));
            }
        }
        catch (Exception ex)
        {
            Messages.Add((LogLevel.Error, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + "写入前失败：" + ex.Message));
        }
    }
    /// <summary>
    /// 导入设备
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceExportAsync(CollectDevice data)
    {
        try
        {
            isDownExport = true;
            StateHasChanged();
            await CollectDeviceService.AddAsync(data);
        }
        finally
        {
            isDownExport = false;
        }
    }

    /// <summary>
    /// 导入变量
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceExportAsync(List<DeviceVariable> data)
    {
        try
        {
            isDownExport = true;
            StateHasChanged();
            await VariableService.AddBatchAsync(data);
        }
        finally
        {
            isDownExport = false;
        }
    }

    /// <summary>
    /// 导出
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public async Task DownDeviceMessageExportAsync(IEnumerable<string> values)
    {
        try
        {
            isDownExport = true;
            StateHasChanged();
            using var memoryStream = new MemoryStream();
            StreamWriter writer = new(memoryStream);
            foreach (var item in values)
            {
                writer.WriteLine(item);
            }
            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var streamRef = new DotNetStreamReference(stream: memoryStream);
            _helper ??= await JS.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Admin.Blazor.Core/js/downloadFileFromStream.js");
            await _helper.InvokeVoidAsync("downloadFileFromStream", $"报文导出{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.txt", streamRef);
        }
        finally
        {
            isDownExport = false;
        }
    }
    /// <inheritdoc/>
    public void LogOut(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        Messages.Add(((LogLevel)logLevel, SysDateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + message + (exception != null ? exception.Message : "")));
        if (Messages.Count > 2500)
        {
            Messages.Clear();
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

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

using System.IO;
using System.Timers;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 调试UI
/// </summary>
public abstract class DriverDebugUIBase : ComponentBase, IDisposable
{
    /// <summary>
    /// 变量地址
    /// </summary>
    protected string address = "40001";

    /// <summary>
    /// 数据类型
    /// </summary>
    protected DataTypeEnum dataTypeEnum = DataTypeEnum.Int16;

    /// <summary>
    /// 导出提示
    /// </summary>
    protected bool isDownExport;

    /// <summary>
    /// 读取长度
    /// </summary>
    protected int length = 1;

    /// <summary>
    /// 日志缓存
    /// </summary>
    protected ConcurrentLinkedList<(LogLevel level, string message)> Messages = new();

    /// <summary>
    /// 默认读写设备
    /// </summary>
    protected IReadWriteDevice plc;

    /// <summary>
    /// 写入值
    /// </summary>
    protected string writeValue = "1";

    /// <summary>
    /// 刷新Timer
    /// </summary>
    private System.Timers.Timer DelayTimer;

    /// <inheritdoc/>
    [Inject]
    protected IJSRuntime JS { get; set; }

    [Inject]
    ICollectDeviceService CollectDeviceService { get; set; }

    [Inject]
    IVariableService VariableService { get; set; }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        DelayTimer?.SafeDispose();
    }

    /// <inheritdoc/>
    public virtual async Task Read()
    {
        var data = await plc.ReadAsync(address, length);
        if (data.IsSuccess)
        {
            try
            {
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, plc.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var value = plc.ThingsGatewayBitConverter.GetDynamicData(dataTypeEnum.GetNetType(), data.Content).ToString();
                Messages.Add((LogLevel.Information, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - 对应类型值：" + value + " - 原始字节：" + data.Content.ToHexString(" ")));
            }
            catch (Exception ex)
            {
                Messages.Add((LogLevel.Warning, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - 操作成功，但转换数据类型失败 - 原因：" + ex.Message + " - 原始字节：" + data.Content.ToHexString(" ")));
            }
        }
        else
        {
            Messages.Add((LogLevel.Error, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - " + data.Message));
        }
    }

    /// <inheritdoc/>
    public virtual async Task Write()
    {
        try
        {
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, plc.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var data = await plc.WriteAsync(dataTypeEnum.GetNetType(), address, writeValue, dataTypeEnum == DataTypeEnum.Bcd);
            if (data.IsSuccess)
            {
                Messages.Add((LogLevel.Information, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - " + data.Message));
            }
            else
            {
                Messages.Add((LogLevel.Warning, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - " + data.Message));
            }
        }
        catch (Exception ex)
        {
            Messages.Add((LogLevel.Error, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - " + "写入前失败：" + ex.Message));
        }
    }

    /// <summary>
    /// 导入变量导出到excel
    /// </summary>
    /// <returns></returns>
    protected async Task DownDeviceExport(CollectDevice data)
    {
        try
        {
            isDownExport = true;
            StateHasChanged();
            using var memoryStream = await CollectDeviceService.ExportFileAsync(new() { data });
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var streamRef = new DotNetStreamReference(stream: memoryStream);
            await JS.InvokeVoidAsync("downloadFileFromStream", $"设备导出{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}.xlsx", streamRef);
        }
        finally
        {
            isDownExport = false;
        }
    }

    /// <summary>
    /// 导入变量导出到excel
    /// </summary>
    /// <returns></returns>
    protected async Task DownDeviceExport(List<CollectDeviceVariable> data)
    {
        try
        {
            isDownExport = true;
            StateHasChanged();
            using var memoryStream = await VariableService.ExportFileAsync(data);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var streamRef = new DotNetStreamReference(stream: memoryStream);
            await JS.InvokeVoidAsync("downloadFileFromStream", $"变量导出{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}.xlsx", streamRef);
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
    protected async Task DownDeviceMessageExport(IEnumerable<string> values)
    {
        try
        {
            isDownExport = true;
            StateHasChanged();
            using var memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);
            foreach (var item in values)
            {
                writer.WriteLine(item);
            }
            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var streamRef = new DotNetStreamReference(stream: memoryStream);
            await JS.InvokeVoidAsync("downloadFileFromStream", $"导出{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}.txt", streamRef);
        }
        finally
        {
            isDownExport = false;
        }
    }
    /// <inheritdoc/>
    protected void LogOut(string str)
    {
        Messages.Add((LogLevel.Debug, str));
        if (Messages.Count > 2500)
        {
            Messages.Clear();
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        DelayTimer = new System.Timers.Timer(1000);
        DelayTimer.Elapsed += timer_Elapsed;
        DelayTimer.AutoReset = true;
        DelayTimer.Start();
        base.OnInitialized();
    }

    private async void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch
        {

        }
    }
}

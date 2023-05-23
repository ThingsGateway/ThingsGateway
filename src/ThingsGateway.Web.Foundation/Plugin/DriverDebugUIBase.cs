#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.Components;
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
    /// <inheritdoc/>
    protected IReadWriteDevice plc;

    /// <inheritdoc/>
    protected bool isDownExport;
    /// <inheritdoc/>
    protected string address = "40001";
    /// <inheritdoc/>
    protected DataTypeEnum dataTypeEnum = DataTypeEnum.Int16;
    /// <inheritdoc/>
    protected int length = 1;
    /// <inheritdoc/>
    protected string writeValue = "1";
    /// <inheritdoc/>
    protected ConcurrentList<(long id, bool? isRed, string message)> Messages = new();
    /// <inheritdoc/>
    [Inject]
    protected IJSRuntime JS { get; set; }
    private System.Timers.Timer DelayTimer;
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
        await InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        DelayTimer?.Dispose();

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
            var memoryStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memoryStream);
            foreach (var item in values)
            {
                writer.WriteLine(item);
            }

            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var streamRef = new DotNetStreamReference(stream: memoryStream);
            await JS.InvokeVoidAsync("downloadFileFromStream", $"报文导出{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}.txt", streamRef);
        }
        finally
        {
            isDownExport = false;
        }
    }

    /// <inheritdoc/>
    protected void LogOut(string str)
    {
        Messages.Add((Yitter.IdGenerator.YitIdHelper.NextId(), null, str));
        if (Messages.Count > 2500)
        {
            Messages.RemoveRange(0, 2000);
        }
    }
    /// <inheritdoc/>
    public async Task Read()
    {
        var data = await plc.ReadAsync(address, length);
        if (data.IsSuccess)
        {
            try
            {
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, plc.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var value = plc.ThingsGatewayBitConverter.GetDynamicData(dataTypeEnum.GetNetType(), data.Content).ToString();
                Messages.Add((Yitter.IdGenerator.YitIdHelper.NextId(), false, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - 对应类型值：" + value + " - 原始字节：" + data.Content.ToHexString(" ")));
            }
            catch (Exception ex)
            {
                Messages.Add((Yitter.IdGenerator.YitIdHelper.NextId(), true, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - 操作成功，但转换数据类型失败 - 原因：" + ex.Message + " - 原始字节：" + data.Content.ToHexString(" ")));
            }
        }
        else
        {
            Messages.Add((Yitter.IdGenerator.YitIdHelper.NextId(), true, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - " + data.Message));
        }
    }

    /// <inheritdoc/>
    public async Task Write()
    {
        try
        {
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, plc.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var data = await plc.WriteAsync(dataTypeEnum.GetNetType(), address, writeValue, dataTypeEnum == DataTypeEnum.Bcd);
            if (data.IsSuccess)
            {
                Messages.Add((Yitter.IdGenerator.YitIdHelper.NextId(), false, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - " + data.Message));
            }
            else
            {
                Messages.Add((Yitter.IdGenerator.YitIdHelper.NextId(), true, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - " + data.Message));
            }
        }
        catch (Exception ex)
        {
            Messages.Add((Yitter.IdGenerator.YitIdHelper.NextId(), true, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + " - " + "写入前失败：" + ex.Message));
        }
    }

}

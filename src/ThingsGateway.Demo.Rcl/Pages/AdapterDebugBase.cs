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

namespace ThingsGateway.Demo;

using ThingsGateway.Components;
using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Extension.Json;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.String;

using TouchSocket.Core;

/// <summary>
/// 调试UI
/// </summary>
public abstract class AdapterDebugBase : BaseComponentBase, IDisposable
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));

    /// <inheritdoc/>
    ~AdapterDebugBase()
    {
        this.SafeDispose();
    }

    /// <summary>
    /// 变量地址
    /// </summary>
    public virtual string RegisterAddress { get; set; } = "40001";

    /// <summary>
    /// 长度
    /// </summary>
    public virtual int ArrayLength { get; set; } = 1;

    /// <summary>
    /// 默认读写设备
    /// </summary>
    public virtual IProtocol Plc { get; set; }

    /// <summary>
    /// 写入值
    /// </summary>
    public virtual string WriteValue { get; set; }

    /// <summary>
    /// 发送数据，utf-8格式
    /// </summary>
    public virtual string SendValue { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    protected virtual DataTypeEnum DataType { get; set; } = DataTypeEnum.Int16;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _periodicTimer?.Dispose();
        Plc?.SafeDispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public virtual async Task ReadAsync()
    {
        if (Plc != null)
        {
            try
            {
                var data = await Plc.ReadAsync(RegisterAddress, ArrayLength, DataType);
                if (data.IsSuccess)
                {
                    Plc.Logger.Debug(data.Content.ToJsonString(true));
                }
                else
                {
                    Plc.Logger.Warning(data.ToString());
                }
            }
            catch (Exception ex)
            {
                Plc.Logger.Exception(ex);
            }
        }
    }

    public virtual void Send()
    {
        if (Plc != null && !SendValue.IsNullOrEmpty())
        {
            try
            {
                Plc.DefaultSend(SendValue.HexStringToBytes());
                Plc.Logger?.Trace($"{Plc.Channel}- Send:{SendValue.HexStringToBytes().ToHexString(' ')}");
            }
            catch (Exception ex)
            {
                Plc.Logger.Exception(ex);
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync()
    {
        if (Plc != null)
        {
            try
            {
                var data = await Plc.WriteAsync(RegisterAddress, WriteValue.GetJTokenFromString(), DataType);
                if (data.IsSuccess)
                {
                    Plc.Logger.Debug(AppService.I18n.T("success"));
                }
                else
                {
                    Plc.Logger.Warning(data.ToString());
                }
            }
            catch (Exception ex)
            {
                Plc.Logger.Exception(ex);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    protected virtual Task ExecuteAsync()
    {
        return Task.CompletedTask;
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            await ExecuteAsync();
            await InvokeAsync(StateHasChanged);
        }
    }
}
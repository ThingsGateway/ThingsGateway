//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <para></para>
/// 采集插件，继承实现不同PLC通讯
/// <para></para>
/// </summary>
public abstract class CollectReceivedFoundationBase : CollectBase, IReceivedFoundationDevice
{
    /// <summary>
    /// 底层驱动，有可能为null
    /// </summary>
    public abstract IReceivedDevice? ReceivedFoundationDevice { get; }
    public override ChannelTypeEnum[] SupportedChannelTypes()
    {
        return ReceivedFoundationDevice?.SupportedChannelTypes();
    }
    public override string ToString()
    {
        return ReceivedFoundationDevice?.ToString() ?? base.ToString();
    }

    /// <inheritdoc/>
    protected override async Task DisposeAsync(bool disposing)
    {
        if (ReceivedFoundationDevice != null)
            await ReceivedFoundationDevice.SafeDisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
    protected override Task<List<VariableSourceRead>> ProtectedLoadSourceReadAsync(List<VariableRuntime> deviceVariables)
    {
        return Task.FromResult(new List<VariableSourceRead>());
    }


    /// <summary>
    /// 是否连接成功
    /// </summary>
    public override bool IsConnected()
    {
        return ReceivedFoundationDevice?.OnLine == true;
    }

    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        if (ReceivedFoundationDevice != null)
        {
            await ReceivedFoundationDevice.ConnectAsync(cancellationToken).ConfigureAwait(false);
        }
    }


    protected override ValueTask TestOnline(object? state, CancellationToken cancellationToken)
    {
        return TestOnline(this, cancellationToken);


        static async PooledValueTask TestOnline(CollectReceivedFoundationBase @this, CancellationToken cancellationToken)
        {
            if (@this.ReceivedFoundationDevice != null)
            {
                if (!@this.ReceivedFoundationDevice.OnLine)
                {
                    if (@this.ReceivedFoundationDevice.DisposedValue || @this.ReceivedFoundationDevice.Channel?.DisposedValue != false) return;
                    Exception exception = null;
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (@this.ReceivedFoundationDevice.DisposedValue || @this.ReceivedFoundationDevice.Channel?.DisposedValue != false) return;

                            await @this.ReceivedFoundationDevice.ConnectAsync(cancellationToken).ConfigureAwait(false);

                            if (@this.CurrentDevice.DeviceStatusChangeTime < TimerX.Now.AddMinutes(-1))
                            {
                                await Task.Delay(30000, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    if (@this.ReceivedFoundationDevice.OnLine == false && exception != null)
                    {
                        foreach (var item in @this.CurrentDevice.VariableSourceReads)
                        {
                            if (item.LastErrorMessage != exception.Message)
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                    @this.LogMessage?.LogWarning(exception, string.Format(AppResource.CollectFail, @this.DeviceName, item?.RegisterAddress, item?.Length, exception.Message));
                            }
                            item.LastErrorMessage = exception.Message;
                            @this.CurrentDevice.SetDeviceStatus(TimerX.Now, null, exception.Message);
                            var time = DateTime.Now;
                            item.Variables.ForEach(a => a.SetValue(null, time, isOnline: false));
                        }
                        foreach (var item in @this.CurrentDevice.ReadVariableMethods)
                        {
                            if (item.LastErrorMessage != exception.Message)
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                    @this.LogMessage?.LogWarning(exception, string.Format(AppResource.MethodFail, @this.DeviceName, item.MethodInfo.Name, exception.Message));
                            }
                            item.LastErrorMessage = exception.Message;
                            @this.CurrentDevice.SetDeviceStatus(TimerX.Now, null, exception.Message);
                            var time = DateTime.Now;
                            item.Variable.SetValue(null, time, isOnline: false);
                        }

                    }
                }
            }

        }

    }

}

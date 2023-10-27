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

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件基类,注意继承的插件的命名空间需要符合<see cref="ExportHelpers.PluginLeftName"/>前置名称
/// </summary>
public abstract class DriverBase : DisposableObject
{
    /// <summary>
    /// <inheritdoc cref="ThingsGateway.Foundation.Core.TouchSocketConfig"/>
    /// </summary>
    public TouchSocketConfig FoundataionConfig;

    /// <summary>
    /// 日志
    /// </summary>
    internal ILogger _logger;

    /// <summary>
    /// 存储日志队列
    /// </summary>
    protected ConcurrentQueue<BackendLog> _logQueues = new();

    /// <inheritdoc cref="DriverBase"/>
    public DriverBase()
    {
        FoundataionConfig = new TouchSocketConfig();
        LogMessage = new LoggerGroup() { LogLevel = ThingsGateway.Foundation.Core.LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(Log_Out) { LogLevel = ThingsGateway.Foundation.Core.LogLevel.Trace });
        FoundataionConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        _ = Task.Factory.StartNew(LogInsertAsync);
    }


    /// <summary>
    /// 调试UI Type，如果不存在，返回null
    /// </summary>
    public abstract Type DriverDebugUIType { get; }

    /// <summary>
    /// 当前插件描述
    /// </summary>
    public DriverPlugin DriverPlugin { get; internal set; }

    /// <summary>
    /// 插件配置项 ，继承实现<see cref="DriverPropertyBase"/>后，返回继承类，如果不存在，返回null
    /// </summary>
    public abstract DriverPropertyBase DriverPropertys { get; }

    /// <summary>
    /// 是否输出日志
    /// </summary>
    public bool IsLogOut { get; set; }

    /// <summary>
    /// 是否存储报文
    /// </summary>
    public bool IsSaveLog { get; set; }

    /// <summary>
    /// 报文信息
    /// </summary>
    public ConcurrentLinkedList<string> Messages { get; set; } = new();

    /// <summary>
    /// 底层日志,如果需要在Blazor界面中显示报文日志，需要输出字符串头部为<see cref="FoundationConst.LogMessageHeader"/>的日志
    /// </summary>
    protected internal LoggerGroup LogMessage { get; private set; }

    /// <summary>
    /// 是否连接成功，如果是上传设备，会直接影响到上传设备的运行状态，如果是采集设备并且不支持读取，需要自更新在线状态
    /// </summary>
    /// <returns></returns>
    public abstract bool IsConnected();

    /// <summary>
    /// 设备报文
    /// </summary>
    internal virtual void NewMessage(ThingsGateway.Foundation.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (IsLogOut)
        {
            if (arg3.StartsWith(FoundationConst.LogMessageHeader))
            {
                Messages.Add(DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + arg3);

                if (Messages.Count > 2500)
                {
                    Messages.Clear();
                }
            }
        }

    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        FoundataionConfig.Dispose();
        base.Dispose(disposing);
    }
    /// <summary>
    /// 底层日志输出
    /// </summary>
    protected virtual void Log_Out(ThingsGateway.Foundation.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (IsLogOut || arg1 >= ThingsGateway.Foundation.Core.LogLevel.Warning)
        {
            _logger.Log_Out(arg1, arg2, arg3, arg4);
        }
    }

    private async Task LogInsertAsync()
    {
        var db = DbContext.Db.CopyNew();
        while (!DisposedValue)
        {
            if (_logQueues.Count > 0)
            {
                try
                {
                    var data = _logQueues.ToListWithDequeue();
                    await db.InsertableWithAttr(data).ExecuteCommandAsync();//入库
                }
                catch
                {
                }

            }

            await Task.Delay(5000);
        }
    }
}
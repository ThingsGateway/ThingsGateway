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

using Microsoft.Extensions.Logging;

using ThingsGateway.Core;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 插件基类,注意继承的插件的命名空间需要符合<see cref="ExportHelpers.PluginLeftName"/>前置名称
/// </summary>
public abstract class DriverBase : DisposableObject
{
    /// <summary>
    /// 显示报文标识
    /// </summary>
    public const string LogMessageHeader = "报文-";
    /// <summary>
    /// <inheritdoc cref="TouchSocket.Core.TouchSocketConfig"/>
    /// </summary>
    public TouchSocketConfig TouchSocketConfig;
    /// <summary>
    /// 底层日志,如果需要在Blazor界面中显示报文日志，需要输出字符串头部为<see cref="LogMessageHeader"/>的日志
    /// </summary>
    protected LoggerGroup logMessage;
    /// <inheritdoc cref="DriverBase"/>
    public DriverBase(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        TouchSocketConfig = new TouchSocketConfig();
        logMessage = new TouchSocket.Core.LoggerGroup();
        logMessage.AddLogger(new TGEasyLogger(Log_Out));
        TouchSocketConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
    }
    /// <summary>
    /// 当前插件描述
    /// </summary>
    public DriverPlugin DriverPlugin { get; set; }
    /// <summary>
    /// 日志
    /// </summary>
    protected ILogger _logger;
    /// <summary>
    /// 服务工厂
    /// </summary>
    protected IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// 插件配置项 ，继承实现<see cref="DriverPropertyBase"/>后，返回继承类，如果不存在，返回null
    /// </summary>
    public abstract DriverPropertyBase DriverPropertys { get; }

    /// <summary>
    /// 调试UI Type，继承实现<see cref="DriverDebugUIBase"/>后，返回继承类的Type，如果不存在，返回null
    /// </summary>
    public abstract Type DriverDebugUIType { get; }

    /// <summary>
    /// 是否输出日志
    /// </summary>
    public bool IsLogOut { get; set; }

    /// <summary>
    /// 报文信息
    /// </summary>
    public ConcurrentLinkedList<string> Messages { get; set; } = new();

    /// <summary>
    /// 是否连接成功
    /// </summary>
    /// <returns></returns>
    public abstract OperResult IsConnected();

    /// <summary>
    /// 底层日志输出
    /// </summary>
    protected void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
    {
        if (IsLogOut)
            _logger.Log_Out(arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// 设备报文
    /// </summary>
    public void NewMessage(LogType arg1, object arg2, string arg3, Exception arg4)
    {
        if (IsLogOut)
        {
            if (arg3.StartsWith(LogMessageHeader))
            {
                Messages.Add(DateTime.Now.ToDateTimeF() + "-" + arg3.Substring(0, Math.Min(arg3.Length, 200)));

                if (Messages.Count > 2500)
                {
                    Messages.Clear();
                }
            }
        }

    }
}
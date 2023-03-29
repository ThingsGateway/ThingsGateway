﻿using Microsoft.Extensions.Logging;

using System.Threading;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;


/// <summary>
/// 上传插件
/// 属性暴露使用<see cref="DevicePropertyAttribute"/>特性标识
/// 如果设备属性需要密码输入，属性名称中需包含Password字符串
/// </summary>
public abstract class UpLoadBase : IDisposable
{
    /// <summary>
    /// <see cref="TouchSocketConfig"/> 
    /// </summary>
    public TouchSocketConfig TouchSocketConfig=new();
    /// <summary>
    /// 日志
    /// </summary>
    protected ILogger _logger;
    private bool isLogOut;
    private ILogger privateLogger;
    /// <summary>
    /// 服务工厂
    /// </summary>
    protected IServiceScopeFactory _scopeFactory;
    /// <inheritdoc cref="UpLoadBase"/>
    public UpLoadBase(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    /// <summary>
    /// 是否输出日志
    /// </summary>
    public bool IsLogOut
    {
        get => isLogOut;
        set
        {
            isLogOut = value;
            if (value)
            {
                _logger = privateLogger;
            }
            else
            {
                _logger = null;
            }
        }
    }


    /// <summary>
    /// 开始执行的方法
    /// </summary>
    /// <returns></returns>
    public abstract Task BeforStart();
    /// <summary>
    /// 返回是否已经在线/成功启动
    /// </summary>
    public abstract OperResult Success();
    /// <inheritdoc/>
    public abstract void Dispose();
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(ILogger logger, UploadDevice device)
    {
        privateLogger = logger;
        if (IsLogOut)
            _logger = privateLogger;
        Init(device);
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device">设备</param>
    protected abstract void Init(UploadDevice device);

    /// <summary>
    /// 循环执行
    /// </summary>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// <see cref="TouchSocket"/> 日志输出
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    protected void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
    {
        switch (arg1)
        {
            case LogType.None:
                _logger?.Log(LogLevel.None, 0, arg4, arg3);
                break;
            case LogType.Trace:
                _logger?.Log(LogLevel.Trace, 0, arg4, arg3);
                break;
            case LogType.Debug:
                _logger?.Log(LogLevel.Debug, 0, arg4, arg3);
                break;
            case LogType.Info:
                _logger?.Log(LogLevel.Information, 0, arg4, arg3);
                break;
            case LogType.Warning:
                privateLogger?.Log(LogLevel.Warning, 0, arg4, arg3);
                break;
            case LogType.Error:
                privateLogger?.Log(LogLevel.Error, 0, arg4, arg3);
                break;
            case LogType.Critical:
                privateLogger?.Log(LogLevel.Critical, 0, arg4, arg3);
                break;
            default:
                break;
        }
    }

}

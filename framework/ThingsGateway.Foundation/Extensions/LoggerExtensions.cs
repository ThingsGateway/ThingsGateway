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


namespace ThingsGateway.Foundation;

/// <summary>
/// <inheritdoc/>
/// </summary>
public static class LoggerExtensions
{

    #region 日志


    /// <summary>
    /// 输出错误日志
    /// </summary>
    public static void LogError(this ILog logger, Exception ex, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Error, null, msg, ex);
    }


    /// <summary>
    /// 输出警示日志
    /// </summary>
    public static void LogWarning(this ILog logger, Exception ex, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Warning, null, msg, ex);
    }


    /// <summary>
    /// 输出警示日志
    /// </summary>
    public static void LogWarning(this ILog logger, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Warning, null, msg, null);
    }
    /// <summary>
    /// 输出提示日志
    /// </summary>
    public static void LogInformation(this ILog logger, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Info, null, msg, null);
    }


    #endregion 日志
}
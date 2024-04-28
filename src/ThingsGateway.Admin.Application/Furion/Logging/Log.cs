
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Logging;

/// <summary>
/// 全局日志静态类
/// </summary>
public static class Log
{
    /// <summary>
    /// 创建日志记录器
    /// </summary>
    /// <returns></returns>
    public static ILogger CreateLogger<T>()
    {
        return App.RootServices!.GetRequiredService<ILogger<T>>();
    }

    /// <summary>
    /// 创建日志工厂
    /// </summary>
    /// <param name="configure">日志构建器</param>
    /// <remarks><see cref="ILoggerFactory"/> 实现了 <see cref="IDisposable"/> 接口，注意使用 `using` 控制</remarks>
    /// <returns></returns>
    public static ILoggerFactory CreateLoggerFactory(Action<ILoggingBuilder> configure = default)
    {
        return LoggerFactory.Create(builder =>
        {
            // 添加默认控制台输出
            builder.AddConsoleFormatter();

            configure?.Invoke(builder);
        });
    }
}
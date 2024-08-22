//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;

namespace ThingsGateway.Admin.NetCore;

public sealed class HostedServiceExecutor
{
    private readonly IEnumerable<IHostedService> _services;

    public HostedServiceExecutor(IEnumerable<IHostedService> services)
    {
        _services = services;
    }

    public async Task StartAsync(CancellationToken token)
    {
        foreach (var service in _services)
        {
            await service.StartAsync(token).ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken token)
    {
        List<Exception>? exceptions = null;

        foreach (var service in _services)
        {
            try
            {
                await service.StopAsync(token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }

        // Throw an aggregate exception if there were any exceptions
        if (exceptions != null)
        {
            throw new AggregateException(exceptions);
        }
    }
}

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public static class OperResultUtil
{
    /// <summary>
    /// 等待读取到指定值，超时返回错误
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="func">执行回调</param>
    /// <param name="value">比较值</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public static async ValueTask<OperResult> WaitAsync<T>(Func<CancellationToken, ValueTask<OperResult<T>>> func, T value, int timeout, CancellationToken cancellationToken = default) where T : IEquatable<T>
    {
        using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout));
        using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await func(stoppingToken.Token).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                if (result.Content.Equals(value))
                {
                    return OperResult.Success;
                }
            }
            else
            {
                return result;
            }
        }
        return new OperResult("Timeout");
    }
}

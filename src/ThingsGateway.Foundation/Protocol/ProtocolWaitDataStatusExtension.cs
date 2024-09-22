//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Resources;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public static partial class ProtocolWaitDataStatusExtension
{
    /// <summary>
    /// 当状态不是<see cref="WaitDataStatus.SetRunning"/>时返回异常。
    /// </summary>
    /// <param name="status"></param>
    public static OperResult Check(this WaitDataStatus status)
    {
        switch (status)
        {
            case WaitDataStatus.SetRunning:
                return new();

            case WaitDataStatus.Canceled: return new(new OperationCanceledException());
            case WaitDataStatus.Overtime: return new(new TimeoutException());
            case WaitDataStatus.Disposed:
            case WaitDataStatus.Default:
            default:
                {
                    return new(new Exception(TouchSocketCoreResource.UnknownError));
                }
        }
    }
}

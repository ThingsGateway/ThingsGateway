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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// IWaitData
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWaitData<T> : IDisposable
    {
        /// <summary>
        /// 等待对象的状态
        /// </summary>
        WaitDataStatus Status { get; }

        /// <summary>
        /// 等待结果
        /// </summary>
        T WaitResult { get; }

        /// <summary>
        /// 取消等待
        /// </summary>
        void Cancel();

        /// <summary>
        /// Reset。
        /// 设置<see cref="WaitResult"/>为null。然后重置状态为<see cref="WaitDataStatus.Default"/>
        /// </summary>
        void Reset();

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        bool Set();

        /// <summary>
        /// 使等待的线程继续执行
        /// </summary>
        /// <param name="waitResult">等待结果</param>
        bool Set(T waitResult);

        /// <summary>
        /// 加载取消令箭
        /// </summary>
        /// <param name="cancellationToken"></param>
        void SetCancellationToken(CancellationToken cancellationToken);

        /// <summary>
        /// 载入结果
        /// </summary>
        void SetResult(T result);
    }
}

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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using System.Runtime.CompilerServices;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// TaskExtension
    /// </summary>
    public static class TaskExtension
    {
        /// <summary>
        /// 同步获取配置ConfigureAwait为false时的结果。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static T GetFalseAwaitResult<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 同步配置ConfigureAwait为false时的执行。
        /// </summary>
        /// <param name="task"></param>
        public static void GetFalseAwaitResult(this Task task)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 配置ConfigureAwait为false。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static ConfiguredTaskAwaitable<T> ConfigureFalseAwait<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// 配置ConfigureAwait为false。
        /// </summary>
        /// <param name="task"></param>
        public static ConfiguredTaskAwaitable ConfigureFalseAwait(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// 异步等待指定最大时间
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var delayTask = Task.Delay(timeout, timeoutCancellationTokenSource.Token);
                if (await Task.WhenAny(task, delayTask) == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;
                }
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// 异步等待指定最大时间
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task WaitAsync(this Task task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var delayTask = Task.Delay(timeout, timeoutCancellationTokenSource.Token);
                if (await Task.WhenAny(task, delayTask) == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await task;
                    return;
                }
                throw new TimeoutException();
            }
        }
    }
}
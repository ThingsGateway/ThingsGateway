//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Runtime.CompilerServices;

namespace ThingsGateway.Foundation;

/// <summary>
/// SystemThreadingExtension
/// </summary>
public static class SystemThreadingExtension
{
    #region ValueTask

    /// <summary>
    /// 同步获取配置ConfigureAwait为false时的结果。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetFalseAwaitResult<T>(this ValueTask<T> task)
    {
        return task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 同步配置ConfigureAwait为false时的执行。
    /// </summary>
    /// <param name="task"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetFalseAwaitResult(this ValueTask task)
    {
        task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    #endregion ValueTask
}

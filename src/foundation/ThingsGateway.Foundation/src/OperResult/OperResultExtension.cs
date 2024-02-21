//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// OperResultExtensions
/// </summary>
public static class OperResultExtension
{
    /// <summary>
    /// 操作成功则继续，并返回对应结果值
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="result"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static OperResult<T1> OperResultFrom<T1>(this OperResult result, Func<T1> func)
    {
        if (result.IsSuccess)
            return new OperResult<T1>() { Content = func() };
        else
            return new OperResult<T1>(result);
    }

    /// <summary>
    /// 操作成功则继续
    /// </summary>
    /// <param name="result"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static OperResult Then(this OperResult result, Func<OperResult> func)
    {
        return !result.IsSuccess ? result : func();
    }

    /// <inheritdoc cref="Then(OperResult, Func{OperResult})"/>
    public static OperResult<T> Then<T>(this OperResult result, Func<OperResult<T>> func)
    {
        return !result.IsSuccess ? new(result) : func();
    }

    /// <inheritdoc cref="Then(OperResult, Func{OperResult})"/>
    public static OperResult<T2> Then<T, T2>(this OperResult<T> result, Func<T?, OperResult<T2>> func)
    {
        return !result.IsSuccess ? new(result) : func(result.Content);
    }

    /// <inheritdoc cref="Then(OperResult, Func{OperResult})"/>
    public static OperResult<T, T2> Then<T, T2>(this OperResult result, Func<OperResult<T, T2>> func)
    {
        return !result.IsSuccess ? new(result) : func();
    }
}
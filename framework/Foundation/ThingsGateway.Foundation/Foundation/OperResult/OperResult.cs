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

using Newtonsoft.Json;

namespace ThingsGateway.Foundation.Core;

/// <inheritdoc/>
public class OperResult<T> : OperResult, IOperResult<T>
{
    /// <inheritdoc/>
    public OperResult()
    {
        ErrorCode = 999;
        Message = "未知错误";
    }
    /// <inheritdoc/>
    public OperResult(OperResult operResult) : base(operResult)
    {
    }
    /// <inheritdoc/>
    public OperResult(string msg) : base(msg)
    {
    }

    /// <inheritdoc/>
    public OperResult(Exception ex) : base(ex)
    {
    }

    /// <inheritdoc/>
    public OperResult(int code, string msg = "成功") : base(code, msg)
    {
    }

    /// <inheritdoc/>
    public T Content { get; set; }

}

/// <inheritdoc/>
public class OperResult<T, T2> : OperResult<T>, IOperResult<T, T2>
{
    /// <inheritdoc/>
    public OperResult()
    {
        ErrorCode = 999;
        Message = "未知错误";
    }
    /// <inheritdoc/>
    public OperResult(OperResult operResult) : base(operResult)
    {
    }
    /// <inheritdoc/>
    public OperResult(string msg) : base(msg)
    {
    }

    /// <inheritdoc/>
    public OperResult(Exception ex) : base(ex)
    {
    }

    /// <inheritdoc/>
    public OperResult(int code, string msg = "成功") : base(code, msg)
    {
    }

    /// <inheritdoc/>
    public T2 Content2 { get; set; }
}

/// <inheritdoc/>
public class OperResult<T, T2, T3> : OperResult<T, T2>, IOperResult<T, T2, T3>
{
    /// <inheritdoc/>
    public OperResult()
    {
        ErrorCode = 999;
        Message = "未知错误";
    }
    /// <inheritdoc/>
    public OperResult(OperResult operResult) : base(operResult)
    {
    }
    /// <inheritdoc/>
    public OperResult(string msg) : base(msg)
    {
    }

    /// <inheritdoc/>
    public OperResult(Exception ex) : base(ex)
    {
    }

    /// <inheritdoc/>
    public OperResult(int code, string msg = "成功") : base(code, msg)
    {
    }

    /// <inheritdoc/>
    public T3 Content3 { get; set; }
}

/// <inheritdoc cref="OperResult"/>
public class OperResult : IOperResult
{
    /// <inheritdoc/>
    public OperResult()
    {
        ErrorCode = 999;
        Message = "未知错误";
    }
    /// <inheritdoc/>
    public OperResult(OperResult operResult)
    {
        ErrorCode = operResult.ErrorCode;
        Message = operResult.Message;
        Exception = operResult.Exception;
    }
    /// <inheritdoc/>
    public OperResult(int code, string msg = "成功")
    {
        ErrorCode = code;
        Message = msg;
    }

    /// <inheritdoc/>
    public OperResult(string msg)
    {
        ErrorCode = 999;
        Message = msg;
    }

    /// <inheritdoc/>
    public OperResult(Exception ex)
    {
        ErrorCode = 999;
        Message = ex.Message;
        Exception = ex;
    }

    /// <inheritdoc/>
    public int ErrorCode { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public Exception Exception;

    /// <inheritdoc/>
    public string ExceptionString => Exception?.ToString();

    /// <inheritdoc/>
    public bool IsSuccess => ErrorCode == 0;

    /// <inheritdoc/>
    public string Message { get; set; } = "成功";

    /// <inheritdoc/>
    public override string ToString()
    {
        return Exception != null ? ExceptionString : Message;
    }

    /// <inheritdoc/>
    public static OperResult<T, T2> CreateSuccessResult<T, T2>(T content, T2 content2)
    {
        return new OperResult<T, T2>(0) { Content = content, Content2 = content2 };
    }
    /// <inheritdoc/>
    public static OperResult<T, T2, T3> CreateSuccessResult<T, T2, T3>(T content, T2 content2, T3 content3)
    {
        return new OperResult<T, T2, T3>(0) { Content = content, Content2 = content2, Content3 = content3 };
    }
    /// <inheritdoc/>
    public static OperResult<T> CreateSuccessResult<T>(T content)
    {
        return new OperResult<T>(0) { Content = content };
    }
    /// <inheritdoc/>
    public static OperResult CreateSuccessResult()
    {
        return new OperResult(0);
    }

}


/// <summary>
/// 操作接口
/// </summary>
public interface IOperResult : IRequestInfo
{
    /// <summary>
    /// 是否成功
    /// </summary>
    bool IsSuccess { get; }
    /// <summary>
    /// 返回消息
    /// </summary>
    string Message { get; set; }
    /// <summary>
    /// 错误代码
    /// </summary>
    int ErrorCode { get; set; }
    /// <summary>
    /// 错误消息
    /// </summary>
    public string ExceptionString { get; }
}

/// <summary>
/// 操作接口
/// </summary>
public interface IOperResult<out T> : IOperResult
{
    /// <inheritdoc/>
    T Content { get; }
}
/// <summary>
/// 操作接口
/// </summary>
public interface IOperResult<out T, out T2> : IOperResult<T>
{
    /// <inheritdoc/>
    T2 Content2 { get; }
}

/// <summary>
/// 操作接口
/// </summary>
public interface IOperResult<out T, out T2, out T3> : IOperResult<T, T2>
{
    /// <inheritdoc/>
    T3 Content3 { get; }
}

/// <summary>
/// OperResultExtensions
/// </summary>
public static class OperResultExtensions
{
    /// <inheritdoc/>
    public static OperResult<T1> OperResultFrom<T1>(this OperResult result, Func<T1> func)
    {
        if (result.IsSuccess)
            return OperResult.CreateSuccessResult(func());
        else
            return new OperResult<T1>(result.ErrorCode, result.Message);
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
    public static OperResult<T1, T2> Then<T1, T2>(this OperResult result, Func<OperResult<T1, T2>> func)
    {
        return !result.IsSuccess ? new(result) : func();
    }

    /// <inheritdoc cref="Then(OperResult, Func{OperResult})"/>
    public static OperResult<T2> Then<T, T2>(this OperResult<T> result, Func<T, OperResult<T2>> func)
    {
        return !result.IsSuccess ? new(result) : func(result.Content);
    }



}
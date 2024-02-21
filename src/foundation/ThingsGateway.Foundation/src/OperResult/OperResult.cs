//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public class OperResult<T> : OperResult, IOperResult<T>
{
    /// <inheritdoc/>
    public OperResult() : base()
    {
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
    public OperResult(string msg, Exception ex) : base(msg, ex)
    {
    }

    /// <inheritdoc/>
    public T Content { get; set; }
}

/// <inheritdoc/>
public class OperResult<T, T2> : OperResult<T>, IOperResult<T, T2>
{
    /// <inheritdoc/>
    public OperResult() : base()
    {
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
    public OperResult(string msg, Exception ex) : base(msg, ex)
    {
    }

    /// <inheritdoc/>
    public T2 Content2 { get; set; }
}

/// <inheritdoc/>
public class OperResult<T, T2, T3> : OperResult<T, T2>, IOperResult<T, T2, T3>
{
    /// <inheritdoc/>
    public OperResult() : base()
    {
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
    public OperResult(string msg, Exception ex) : base(msg, ex)
    {
    }

    /// <inheritdoc/>
    public T3 Content3 { get; set; }
}

/// <inheritdoc cref="IOperResult"/>
public class OperResult : IOperResult
{
    /// <summary>
    /// 异常堆栈
    /// </summary>
    [JsonIgnore]
    public Exception? Exception;

    /// <summary>
    /// 默认构造，操作结果会是成功
    /// </summary>
    public OperResult()
    {
    }

    /// <summary>
    /// 从另一个操作对象中赋值信息
    /// </summary>
    public OperResult(OperResult operResult)
    {
        OperCode = operResult.OperCode;
        ErrorMessage = operResult.ErrorMessage;
        Exception = operResult.Exception;
        ErrorCode = operResult.ErrorCode;
    }

    /// <summary>
    /// 传入错误信息
    /// </summary>
    /// <param name="msg"></param>
    public OperResult(string msg)
    {
        OperCode = 500;
        ErrorMessage = msg;
    }

    /// <summary>
    /// 传入异常堆栈
    /// </summary>
    public OperResult(Exception ex)
    {
        OperCode = 500;
        Exception = ex;
        ErrorMessage = ex.Message;
        //指定Timeout或OperationCanceled为超时取消
        if (ex is TimeoutException || ex is OperationCanceledException)
        {
            ErrorCode = ErrorCodeEnum.Canceled;
        }
        else if (ex is ReturnErrorException)
        {
            ErrorCode = ErrorCodeEnum.RetuenError;
        }
        else
        {
            ErrorCode = ErrorCodeEnum.InvokeFail;
        }
    }

    /// <summary>
    /// 传入错误信息与异常堆栈
    /// </summary>
    public OperResult(string msg, Exception ex) : this(ex)
    {
        ErrorMessage = msg;
    }

    /// <inheritdoc/>
    public int? OperCode { get; set; }

    /// <inheritdoc/>
    public bool IsSuccess => OperCode == null || OperCode == 0;

    /// <inheritdoc/>
    public string? ErrorMessage { get; set; }

    /// <inheritdoc/>
    public ErrorCodeEnum? ErrorCode { get; private set; } = ErrorCodeEnum.RetuenError;

    /// <summary>
    /// 返回一个成功结果，并带有结果值
    /// </summary>
    public static OperResult<T> CreateSuccessResult<T>(T content)
    {
        return new() { Content = content };
    }

    /// <summary>
    /// 返回错误信息与异常堆栈等信息
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string messageString = ErrorMessage == null ? string.Empty : $"{FoundationConst.ErrorMessage}:{ErrorMessage}";
        string exceptionString = Exception == null ? string.Empty : ErrorMessage == null ? $"{FoundationConst.Exception}:{Exception}" : $"{Environment.NewLine}{FoundationConst.Exception}:{Exception}";

        return $"{messageString}{exceptionString}";
    }
}
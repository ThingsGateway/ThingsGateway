#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using TouchSocket.Resources;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public class OperResult<T> : OperResult
{
    /// <inheritdoc/>
    public OperResult() { }
    /// <inheritdoc/>
    public OperResult(ResultCode resultCode, string msg) : base(resultCode, msg)
    {
    }

    /// <inheritdoc/>
    public OperResult(T content) : base(ResultCode.Fail)
    {
        Content = content;
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
    public T Content { get; set; }

    /// <inheritdoc/>
    public OperResult<TResult> Then<TResult>(Func<T, OperResult<TResult>> func)
    {
        return !IsSuccess ? OperResult.CreateFailedResult<TResult>(this) : func(Content);
    }
}

/// <inheritdoc/>
public class OperResult<T1, T2> : OperResult
{
    /// <inheritdoc/>
    public OperResult() : base()
    {
    }
    /// <inheritdoc/>
    public OperResult(ResultCode resultCode, string msg) : base(resultCode, msg)
    {
    }

    /// <inheritdoc/>
    public OperResult(T1 content1, T2 content2) : base(ResultCode.Fail)
    {
        Content1 = content1; Content2 = content2;
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
    public T1 Content1 { get; set; }
    /// <inheritdoc/>
    public T2 Content2 { get; set; }
}

/// <inheritdoc/>
public class OperResult<T1, T2, T3> : OperResult
{
    /// <inheritdoc/>
    public OperResult(ResultCode resultCode, string msg) : base(resultCode, msg)
    {
    }

    /// <inheritdoc/>
    public OperResult(T1 content1, T2 content2, T3 content3) : base(ResultCode.Fail)
    {
        Content1 = content1; Content2 = content2; Content3 = content3;
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
    public T1 Content1 { get; set; }
    /// <inheritdoc/>
    public T2 Content2 { get; set; }
    /// <inheritdoc/>
    public T3 Content3 { get; set; }
}

/// <inheritdoc cref="IOperResult"/>
public class OperResult : IRequestInfo, IOperResult
{
    /// <summary>
    /// 业务错误代码
    /// </summary>
    public int Code;

    /// <inheritdoc/>
    public OperResult()
    {
    }

    /// <inheritdoc/>
    public OperResult(ResultCode resultCode, string msg)
    {
        ResultCode = resultCode;
        Message = msg;
    }

    /// <inheritdoc/>
    public OperResult(ResultCode resultCode)
    {
        ResultCode = resultCode;
        Message = ResultCode.GetDescription();
    }

    /// <inheritdoc/>
    public OperResult(int code)
    {
        ResultCode = ResultCode.Fail;
        Code = code;
    }

    /// <inheritdoc/>
    public OperResult(string msg)
    {
        ResultCode = ResultCode.Fail;
        Message = msg;
    }
    /// <inheritdoc/>
    public OperResult(Exception ex)
    {
        Message = ex.Message;
        Exception = ex.StackTrace;
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Exception { get; set; }
    /// <inheritdoc/>
    public bool IsSuccess => ResultCode.HasFlag(ResultCode.Success);
    /// <inheritdoc/>
    public string Message { get; set; }
    /// <inheritdoc/>
    public ResultCode ResultCode { get; set; }

    /// <inheritdoc/>
    public static OperResult<T1> CreateFailedResult<T1>(OperResult result)
    {
        OperResult<T1> failedResult = new OperResult<T1>(result.Message)
        {
            Code = result.Code
        };
        return failedResult;
    }

    /// <inheritdoc/>
    public static OperResult<T1, T2> CreateFailedResult<T1, T2>(OperResult result)
    {
        OperResult<T1, T2> failedResult = new OperResult<T1, T2>(result.Message)
        {
            Code = result.Code
        };
        return failedResult;
    }

    /// <inheritdoc/>
    public static OperResult<T1, T2, T3> CreateFailedResult<T1, T2, T3>(OperResult result)
    {
        OperResult<T1, T2, T3> failedResult = new OperResult<T1, T2, T3>(result.Message)
        {
            Code = result.Code
        };
        return failedResult;
    }

    /// <inheritdoc/>
    public static OperResult CreateSuccessResult()
    {
        return new OperResult(ResultCode.Success);
    }

    /// <inheritdoc/>
    public static OperResult<T> CreateSuccessResult<T>(T value)
    {
        return new OperResult<T>(value)
        {
            ResultCode = ResultCode.Success,
            Message = TouchSocketStatus.Success.GetDescription(),
        };
    }

    /// <inheritdoc/>
    public static OperResult<T1, T2> CreateSuccessResult<T1, T2>(T1 value1, T2 value2)
    {
        return new OperResult<T1, T2>(value1, value2)
        {
            ResultCode = ResultCode.Success,
            Message = TouchSocketStatus.Success.GetDescription(),
        };
    }

    /// <inheritdoc/>
    public static OperResult<T1, T2, T3> CreateSuccessResult<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        return new OperResult<T1, T2, T3>(value1, value2, value3)
        {
            ResultCode = ResultCode.Success,
            Message = TouchSocketStatus.Success.GetDescription(),
        };
    }
}
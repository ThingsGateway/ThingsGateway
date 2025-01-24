//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ThingsGateway.Foundation;

/// <inheritdoc cref="IOperResult"/>
public class OperResultClass : IOperResult
{
    /// <summary>
    /// 异常堆栈
    /// </summary>
#if NET6_0_OR_GREATER
    [System.Text.Json.Serialization.JsonIgnore]
#endif

    [JsonIgnore]
    public Exception? Exception { get; set; }

    /// <summary>
    /// 从另一个操作对象中赋值信息
    /// </summary>
    public OperResultClass(IOperResult operResult)
    {
        OperCode = operResult.OperCode;
        ErrorMessage = operResult.ErrorMessage;
        Exception = operResult.Exception;
        ErrorType = operResult.ErrorType;
    }

    /// <summary>
    /// 传入错误信息
    /// </summary>
    /// <param name="msg"></param>
    public OperResultClass(string msg)
    {
        OperCode = 500;
        ErrorMessage = msg;
        ErrorType = ErrorTypeEnum.InvokeFail;
    }

    /// <summary>
    /// 传入异常堆栈
    /// </summary>
    public OperResultClass(Exception ex)
    {
        OperCode = 500;
        Exception = ex;
        ErrorMessage = ex.Message;
        //指定Timeout或OperationCanceled为超时取消
        if (ex is TimeoutException || ex is OperationCanceledException)
        {
            ErrorType = ErrorTypeEnum.Canceled;
        }
        else if (ex is ReturnErrorException)
        {
            ErrorType = ErrorTypeEnum.DeviceError;
        }
        else
        {
            ErrorType = ErrorTypeEnum.InvokeFail;
        }
    }

    /// <summary>
    /// 传入错误信息与异常堆栈
    /// </summary>
    public OperResultClass(string msg, Exception ex) : this(ex)
    {
        ErrorMessage = msg;
    }

    /// <summary>
    /// 默认构造，操作结果会是成功
    /// </summary>
    public OperResultClass()
    {
    }

    /// <inheritdoc/>
    public int? OperCode { get; set; }

    /// <inheritdoc/>
    public bool IsSuccess => OperCode == null || OperCode == 0;

    /// <inheritdoc/>
    public string? ErrorMessage { get; set; }

    /// <inheritdoc/>
#if NET6_0_OR_GREATER
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
#endif

    [JsonConverter(typeof(StringEnumConverter))]
    public ErrorTypeEnum? ErrorType { get; set; }

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
        string messageString = ErrorMessage == null ? string.Empty : $"{DefaultResource.Localizer["ErrorMessage"]}:{ErrorMessage}";
        string exceptionString = Exception == null ? string.Empty : ErrorMessage == null ? $"{DefaultResource.Localizer["Exception"]}:{Exception}" : $"{Environment.NewLine}{DefaultResource.Localizer["Exception"]}:{Exception}";

        return $"{messageString}{exceptionString}";
    }
}
/// <inheritdoc/>
public class OperResultClass<T> : OperResultClass, IOperResult<T>
{
    /// <inheritdoc/>
    public OperResultClass() : base()
    {
    }

    /// <inheritdoc/>
    public OperResultClass(IOperResult operResult) : base(operResult)
    {
    }

    /// <inheritdoc/>
    public OperResultClass(string msg) : base(msg)
    {
    }

    /// <inheritdoc/>
    public OperResultClass(Exception ex) : base(ex)
    {
    }

    /// <inheritdoc/>
    public OperResultClass(string msg, Exception ex) : base(msg, ex)
    {
    }

    /// <inheritdoc/>
    public T Content { get; set; }
}

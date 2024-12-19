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

/// <summary>
/// 操作接口，继承<see cref="IRequestInfo"/>后可直接用于适配器
/// </summary>
public interface IOperResult : IRequestInfo
{
    /// <summary>
    /// 执行错误返回类型
    /// </summary>
    ErrorCodeEnum? ErrorCode { get; }

    /// <summary>
    /// 返回消息
    /// </summary>
    string? ErrorMessage { get; set; }

    /// <summary>
    /// 异常堆栈
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// 错误代码，不为0时判定为失败
    /// </summary>
    int? OperCode { get; set; }
}

/// <summary>
/// 操作接口
/// </summary>
public interface IOperResult<out T> : IOperResult
{
    /// <summary>
    /// 返回对象
    /// </summary>
    T Content { get; }
}

/// <summary>
/// 操作接口
/// </summary>
public interface IOperResult<out T, out T2> : IOperResult<T>
{
    /// <summary>
    /// 返回对象
    /// </summary>
    T2 Content2 { get; }
}

/// <summary>
/// 操作接口
/// </summary>
public interface IOperResult<out T, out T2, out T3> : IOperResult<T, T2>
{
    /// <summary>
    /// 返回对象
    /// </summary>
    T3 Content3 { get; }
}

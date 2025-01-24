// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace ThingsGateway.FriendlyException;

/// <summary>
/// 异常拓展
/// </summary>
[SuppressSniffer]
public static class AppFriendlyExceptionExtensions
{
    /// <summary>
    /// 设置异常状态码
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static AppFriendlyException StatusCode(this AppFriendlyException exception, int statusCode = StatusCodes.Status500InternalServerError)
    {
        exception.StatusCode = statusCode;
        return exception;
    }

    /// <summary>
    /// 设置额外数据
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static AppFriendlyException WithData(this AppFriendlyException exception, object data)
    {
        exception.Data = data;
        return exception;
    }
}
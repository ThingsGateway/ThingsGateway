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

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="ObjectContentConverter{TResult}" /> 工厂
/// </summary>
public interface IObjectContentConverterFactory
{
    /// <summary>
    ///     获取 <see cref="ObjectContentConverter{TResult}" /> 实例
    /// </summary>
    /// <typeparam name="TResult">转换的目标类型</typeparam>
    /// <returns>
    ///     <see cref="ObjectContentConverter{TResult}" />
    /// </returns>
    ObjectContentConverter<TResult> GetConverter<TResult>();

    /// <summary>
    ///     获取 <see cref="ObjectContentConverter" /> 实例
    /// </summary>
    /// <param name="resultType">转换的目标类型</param>
    /// <returns>
    ///     <see cref="ObjectContentConverter" />
    /// </returns>
    ObjectContentConverter GetConverter(Type resultType);
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

namespace ThingsGateway.JsonSerialization;

/// <summary>
/// Json 序列化提供器
/// </summary>
public interface IJsonSerializerProvider
{
    /// <summary>
    /// 序列化对象
    /// </summary>
    /// <param name="value"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    string Serialize(object value, object jsonSerializerOptions = default);

    /// <summary>
    /// 反序列化字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    T Deserialize<T>(string json, object jsonSerializerOptions = default);

    /// <summary>
    /// 反序列化字符串
    /// </summary>
    /// <param name="json"></param>
    /// <param name="returnType"></param>
    /// <param name="jsonSerializerOptions"></param>
    /// <returns></returns>
    object Deserialize(string json, Type returnType, object jsonSerializerOptions = default);

    /// <summary>
    /// 返回读取全局配置的 JSON 选项
    /// </summary>
    /// <returns></returns>
    object GetSerializerOptions();
}
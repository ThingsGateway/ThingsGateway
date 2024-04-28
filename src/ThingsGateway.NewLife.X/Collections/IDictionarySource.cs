
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace NewLife.Collections;

/// <summary>
/// 字典数据源接口。定义该模型类支持输出名值字典，便于序列化传输
/// </summary>
public interface IDictionarySource
{
    /// <summary>
    /// 把对象转为名值字典，便于序列化传输
    /// </summary>
    /// <returns></returns>
    IDictionary<String, Object?> ToDictionary();
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class BusinessPropertyWithCache : BusinessPropertyBase
{
    /// <summary>
    /// 启用缓存
    /// </summary>
    [DynamicProperty("启用缓存", "")]
    public virtual bool CacheEnable { get; set; } = true;

    /// <summary>
    /// 上传列表最大数量
    /// </summary>
    [DynamicProperty("上传列表最大数量", "默认1千条")]
    public virtual int SplitSize { get; set; } = 1000;

    /// <summary>
    /// 内存队列的最大数量，超出时转入文件缓存，根据数据量设定适当值
    /// </summary>
    [DynamicProperty("内存队列最大数量", "内存队列的最大数量，超出或失败时转入文件缓存，根据数据量设定适当值")]
    public virtual int QueueMaxCount { get; set; } = 10000;
}
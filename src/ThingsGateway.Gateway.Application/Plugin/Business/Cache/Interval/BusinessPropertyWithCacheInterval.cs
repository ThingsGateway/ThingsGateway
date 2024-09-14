﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class BusinessPropertyWithCacheInterval : BusinessPropertyWithCache
{
    [DynamicProperty]
    public virtual bool IsAllVariable { get; set; } = false;

    /// <summary>
    /// 是否间隔上传
    /// </summary>
    [DynamicProperty]
    public virtual bool IsInterval { get; set; } = false;

    /// <summary>
    /// 间隔上传时间
    /// </summary>
    [DynamicProperty]
    public virtual int BusinessInterval { get; set; } = 1000;
}

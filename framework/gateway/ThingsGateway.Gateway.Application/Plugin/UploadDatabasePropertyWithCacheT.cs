#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Mapster;

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Gateway.Application;


/// <summary>
/// 上传数据库插件
/// </summary>
public abstract class UploadDatabasePropertyWithCacheT: UpDriverDatabasePropertyBase
{
    /// <summary>
    /// 线程循环间隔
    /// </summary>
    [DeviceProperty("线程循环间隔", "最小50ms")]
    public virtual int CycleInterval { get; set; } = 1000;
    /// <summary>
    /// 内存队列最大条数
    /// </summary>
    [DeviceProperty("内存队列最大条数", "默认2w条")]
    public virtual int QueueMaxCount { get; set; } = 20000;
    /// <summary>
    /// 离线缓存大小限制
    /// </summary>
    [DeviceProperty("离线缓存文件大小限制", "默认1024mb")]
    public virtual int CahceMaxLength { get; set; } = 1024;

    /// <summary>
    /// 列表分割大小
    /// </summary>
    [DeviceProperty("列表分割大小", "默认1千条")]
    public virtual int SplitSize { get; set; } = 1000;

    /// <summary>
    /// 是否间隔上传
    /// </summary>
    [DeviceProperty("是否间隔上传", "False时为变化检测上传")]
    public virtual bool IsInterval { get; set; } = false;
    /// <summary>
    /// 上传间隔时间
    /// </summary>
    [DeviceProperty("上传间隔时间", "最小100ms")]
    public virtual int UploadInterval { get; set; } = 1000;

    [DeviceProperty("是否选择全部变量", "")] public bool IsAllVariable { get; set; } = false;
}



﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using SqlSugar;

using System.Reflection;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 附加属性
/// </summary>
public class DriverMethodInfo
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 属性描述
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Description { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public string? Remark { get; set; }

    /// <summary>
    /// 方法
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AdaptIgnore]
    public MethodInfo? MethodInfo { get; set; }
}

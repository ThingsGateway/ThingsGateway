//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 文件导入通用输出
/// </summary>
public class ImportPreviewOutputBase
{
    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// 导入数据数量
    /// </summary>
    public int DataCount { get => Results.Count; }

    /// <summary>
    /// 返回状态
    /// </summary>
    public ConcurrentList<(int Row, bool Success, string? ErrorMessage)> Results { get; set; } = new();
}

/// <summary>
/// 导入预览
/// </summary>
/// <typeparam name="T"></typeparam>
public class ImportPreviewOutput<T> : ImportPreviewOutputBase where T : class
{
    /// <summary>
    /// 数据
    /// </summary>
    public Dictionary<string, T> Data { get; set; } = new();
}
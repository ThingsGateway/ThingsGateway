
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Text.Json.Serialization;

namespace ThingsGateway.Core;

public class WebhookPostBody
{
    /// <summary>
    /// 提交分支信息
    /// </summary>
    public string? Ref { get; set; }

    public string? Password { get; set; }

    public string? Id { get; set; }

    public string? Sign { get; set; }

    public string GetBranchName() => Ref?.Replace("refs/heads/", "") ?? "";
}

/// <summary>
/// Gitee 提交事件参数实体类
/// </summary>
public class GiteePostBody : WebhookPostBody
{
    /// <summary>
    /// 提交信息集合
    /// </summary>
    public ICollection<GiteeCommit>? Commits { get; set; }

    [JsonPropertyName("head_commit")]
    public GiteeCommit? HeadCommit { get; set; }
}

/// <summary>
/// 提交信息实体类
/// </summary>
public class GiteeCommit
{
    /// <summary>
    /// 提交消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 提交时间戳
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// 提交地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 提交作者
    /// </summary>
    public GiteeAuthor Author { get; set; } = new GiteeAuthor();
}

/// <summary>
/// 提交作者信息
/// </summary>
public class GiteeAuthor
{
    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// 提交人 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 提交人名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 提交人邮件地址
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// 提交人名称
    /// </summary>
    public string UserName { get; set; } = "";

    /// <summary>
    /// 提交人 Gitee 地址
    /// </summary>
    public string Url { get; set; } = "";
}
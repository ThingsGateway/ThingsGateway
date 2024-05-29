//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Core;

namespace ThingsGateway.Razor;

public partial class CommitItem
{
    [Parameter]
    [NotNull]
    [EditorRequired]
    public GiteePostBody? Item { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<CommitItem>? Localizer { get; set; }

    private string? Author { get; set; }

    private string? Timestamp { get; set; }

    private string? Message { get; set; }

    private string? Url { get; set; }

    private string? Branch { get; set; }

    [NotNull]
    private string? TotalCount { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var commit = Item.HeadCommit;
        TotalCount = Item.Commits?.Count.ToString() ?? "1";
        if (commit != null)
        {
            Timestamp = commit.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            Author = commit.Author.Name;
            Message = commit.Message;
            Url = commit.Url;
            Branch = Item.GetBranchName();
        }
    }
}

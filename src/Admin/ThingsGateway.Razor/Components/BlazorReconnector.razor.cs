//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Razor;

/// <inheritdoc/>
public partial class BlazorReconnector
{
    [Inject]
    [NotNull]
    private IStringLocalizer<BlazorReconnector>? Localizer { get; set; }

    [NotNull]
    private string? ReconnectFailed1 { get; set; }

    [NotNull]
    private string? ReconnectFailed2 { get; set; }

    [NotNull]
    private string? ReconnectFailed3 { get; set; }

    [NotNull]
    private string? ReconnectFailed4 { get; set; }

    [NotNull]
    private string? ReconnectFailed5 { get; set; }

    [NotNull]
    private string? Reconnecting1 { get; set; }

    [NotNull]
    private string? Reconnecting2 { get; set; }

    [NotNull]
    private string? Reconnecting3 { get; set; }

    [NotNull]
    private string? ReconnectRejected1 { get; set; }

    [NotNull]
    private string? ReconnectRejected2 { get; set; }

    [NotNull]
    private string? ReconnectRejected3 { get; set; }

    [NotNull]
    private string? ReconnectRejected4 { get; set; }

    [NotNull]
    private string? RenderThingsGateway1 { get; set; }

    [NotNull]
    private string? RenderThingsGateway2 { get; set; }

    [NotNull]
    private string? RenderThingsGateway3 { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        Reconnecting1 = Localizer[nameof(Reconnecting1)];
        Reconnecting2 = Localizer[nameof(Reconnecting2)];
        Reconnecting3 = Localizer[nameof(Reconnecting3)];
        ReconnectFailed1 = Localizer[nameof(ReconnectFailed1)];
        ReconnectFailed2 = Localizer[nameof(ReconnectFailed2)];
        ReconnectFailed3 = Localizer[nameof(ReconnectFailed3)];
        ReconnectFailed4 = Localizer[nameof(ReconnectFailed4)];
        ReconnectFailed5 = Localizer[nameof(ReconnectFailed5)];
        ReconnectRejected1 = Localizer[nameof(ReconnectRejected1)];
        ReconnectRejected2 = Localizer[nameof(ReconnectRejected2)];
        ReconnectRejected3 = Localizer[nameof(ReconnectRejected3)];
        ReconnectRejected4 = Localizer[nameof(ReconnectRejected4)];
        RenderThingsGateway1 = Localizer[nameof(RenderThingsGateway1)];
        RenderThingsGateway2 = Localizer[nameof(RenderThingsGateway2)];
        RenderThingsGateway3 = Localizer[nameof(RenderThingsGateway3)];
    }
}

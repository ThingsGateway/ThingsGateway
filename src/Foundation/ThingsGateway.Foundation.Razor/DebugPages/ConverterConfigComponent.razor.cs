//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait

using System.Text;

using ThingsGateway.Foundation;

using LocalizerUtil = ThingsGateway.Razor.LocalizerUtil;

namespace ThingsGateway.Debug;

public partial class ConverterConfigComponent : ComponentBase
{
    [Parameter, EditorRequired]
    public ConverterConfig Model { get; set; }
    [Inject]
    IStringLocalizer<ConverterConfigComponent> Localizer { get; set; }

    private List<SelectedItem> BoolItems;
    private List<SelectedItem> EncodingItems;

    protected override void OnInitialized()
    {
        BoolItems = LocalizerUtil.GetBoolItems(Model.GetType(), nameof(Model.VariableStringLength), true);
        EncodingItems = new List<SelectedItem>() { new SelectedItem("", "none") }.Concat(Encoding.GetEncodings().Select(a => new SelectedItem(a.CodePage.ToString(), a.DisplayName))).ToList();
    }
}

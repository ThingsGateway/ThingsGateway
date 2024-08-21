//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Json.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class ScriptCheck
{
    private string Input { get; set; }
    private string Output { get; set; }

    [Parameter, EditorRequired]
    public IEnumerable<object> Data { get; set; }
    [Parameter, EditorRequired]
    public string Script { get; set; }
    [Parameter, EditorRequired]
    public EventCallback<string> ScriptChanged { get; set; }

    protected override void OnInitialized()
    {
        Input = Data.ToJsonNetString();
        base.OnInitialized();
    }

    private void CheckScript()
    {
        try
        {
            Data = Input.FromJsonNetString<IEnumerable<object>>();
            var value = Data.GetDynamicModel(Script);
            Output = value.ToJsonNetString();
        }

        catch (Exception ex)
        {
            Output = ex.ToString();
        }

    }
    [Inject]
    private IStringLocalizer<DeviceEditComponent> Localizer { get; set; }
}

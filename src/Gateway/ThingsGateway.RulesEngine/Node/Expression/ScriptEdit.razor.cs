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

namespace ThingsGateway.RulesEngine;

public partial class ScriptEdit
{
    private string Input { get; set; }
    private string Output { get; set; }

    [Parameter, EditorRequired]
    public string Script { get; set; }
    [Parameter, EditorRequired]
    public EventCallback<string> ScriptChanged { get; set; }
    private async Task Change(string script)
    {
        Script = script;
        if (ScriptChanged.HasDelegate)
            await ScriptChanged.InvokeAsync(script);
    }
    [Parameter, EditorRequired]
    public Func<string, Task<string>> OnCheckScript { get; set; }
    private async Task CheckScript()
    {
        try
        {
            if (OnCheckScript != null)
                Output = await OnCheckScript.Invoke(Input);
        }
        catch (Exception ex)
        {
            Output = ex.ToString();
        }
    }

    [Inject]
    IStringLocalizer<ThingsGateway.RulesEngine._Imports> Localizer { get; set; }
}

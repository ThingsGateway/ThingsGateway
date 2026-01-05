// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait

using Newtonsoft.Json.Linq;

using System.Text;
using ThingsGateway.Foundation.Common.Json.Extension;
using TouchSocket.Core;

using Size = BootstrapBlazor.Components.Size;

namespace ThingsGateway.Gateway.Razor
{
    public partial class CSharpScriptWidget
    {
        [Inject]
        IStringLocalizer<ThingsGateway.Gateway.Razor.Common._Imports> Localizer { get; set; }

        [Parameter]
        public TextNode Node { get; set; }

        private async Task CheckScript()
        {
            var op = new DialogOption()
            {
                IsScrolling = true,
                Title = Localizer["Check"],
                ShowFooter = false,
                ShowCloseButton = false,
                Size = Size.ExtraExtraLarge,
                FullScreenSize = FullScreenSize.None
            };

            op.Component = BootstrapDynamicComponent.CreateComponent<ScriptEdit>(new Dictionary<string, object?>
    {
        {nameof(ScriptEdit.OnCheckScript),  new Func<string,Task<string>>(async a=>{
            if(Node is IConditionNode conditionNode)
            {
                StringBuilder stringBuilder=new();
                conditionNode.Logger=new EasyLogger((a)=>stringBuilder.AppendLine(a));
                var out1=  (await conditionNode.ExecuteAsync(new NodeInput(){Value=a==null?a:JToken.Parse(a??string.Empty) },default).ConfigureAwait(false)).ToString();
                stringBuilder.AppendLine(out1);
                return stringBuilder.ToString();
            }
             if(Node is IExpressionNode expressionNode)
            {
                StringBuilder stringBuilder=new();
                expressionNode.Logger=new EasyLogger((a)=>stringBuilder.AppendLine(a));
                var data=await expressionNode.ExecuteAsync(new NodeInput(){Value=a==null?a:JToken.Parse(a??string.Empty) },default).ConfigureAwait(false);

                stringBuilder.AppendLine( data.IsSuccess? data.Content.JToken?.ToJsonString(SystemTextJsonExtension.SystemTextJsonService.IndentedOptions)??string.Empty: data.ToString());
                return stringBuilder.ToString();
            }
             if(Node is IActuatorNode actuatorNode)
            {
                StringBuilder stringBuilder=new();
                actuatorNode.Logger=new EasyLogger((a)=>stringBuilder.AppendLine(a));
                var data=await actuatorNode.ExecuteAsync(new NodeInput(){Value=a==null?a:JToken.Parse(a??string.Empty) },default).ConfigureAwait(false);

                stringBuilder.AppendLine( data.IsSuccess? data.Content.JToken?.ToJsonString(SystemTextJsonExtension.SystemTextJsonService.IndentedOptions)??string.Empty: data.ToString());
                return stringBuilder.ToString();
            }
        return string.Empty;
        }) },
        {nameof(ScriptEdit.Script),Node.Text },
        {nameof(ScriptEdit.ScriptChanged),EventCallback.Factory.Create<string>(this, v => Node.Text=v)},
    });
            await DialogService.Show(op);
        }

        [Inject]
        DialogService DialogService { get; set; }
    }
}
// ------------------------------------------------------------------------------
// �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
// �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
// Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
// GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
// GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
// ʹ���ĵ���https://thingsgateway.cn/
// QQȺ��605534569
// ------------------------------------------------------------------------------
#pragma warning disable CA2007 // ���ǶԵȴ���������� ConfigureAwait

using Newtonsoft.Json.Linq;

using Size = BootstrapBlazor.Components.Size;

namespace ThingsGateway.RulesEngine
{
    public partial class CSharpScriptWidget
    {
        [Inject]
        IStringLocalizer<ThingsGateway.RulesEngine._Imports> Localizer { get; set; }

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
          return  (await conditionNode.ExecuteAsync(new NodeInput(){Value=a==null?a:JToken.Parse(a??string.Empty) },default).ConfigureAwait(false)).ToString();
             if(Node is IExpressionNode expressionNode)
          return  (await expressionNode.ExecuteAsync(new NodeInput(){Value=a==null?a:JToken.Parse(a??string.Empty) },default).ConfigureAwait(false)).JToken?.ToString();
             if(Node is IActuatorNode actuatorNode)
          return  (await actuatorNode.ExecuteAsync(new NodeInput(){Value=a==null?a:JToken.Parse(a??string.Empty) },default).ConfigureAwait(false)).JToken?.ToString();
        return "";
        }) },
        {nameof(ScriptEdit.Script),Node.Text },
        {nameof(ScriptEdit.ScriptChanged),EventCallback.Factory.Create<string>(this, v =>
        {
           Node.Text=v;

        })},

    });
            await DialogService.Show(op);

        }

        [Inject]
        DialogService DialogService { get; set; }
    }
}
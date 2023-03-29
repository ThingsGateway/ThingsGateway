using CodingSeb.ExpressionEvaluator;

using System.Linq;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 表达式扩展
    /// </summary>
    public static class ExpressionEvaluatorExtension
    {
        static ExpressionEvaluator ExpressionEvaluator;

        static ExpressionEvaluatorExtension()
        {
            ExpressionEvaluator = new();
            ExpressionEvaluator.PreEvaluateVariable += Evaluator_PreEvaluateVariable;
        }
        /// <summary>
        /// 计算表达式
        /// </summary>
        public static object GetExpressionsResult(this string expressions, object rawvalue)
        {
            if (expressions.IsNullOrEmpty())
            {
                return rawvalue;
            }
            ExpressionEvaluator.Variables = new Dictionary<string, object>()
                {
                  { "Raw", rawvalue},
                  { "raw", rawvalue},
                };
            var value = ExpressionEvaluator.Evaluate(expressions);
            return value;
        }
        /// <summary>
        /// 表达式的扩展变量来源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void Evaluator_PreEvaluateVariable(object sender, VariablePreEvaluationEventArg e)
        {
            var data = App.GetService<GlobalCollectDeviceData>();
            var obj = data.CollectVariables.FirstOrDefault(it => it.Name == e.Name);
            if (obj == null)
            {
                return;
            }
            if (obj.Value != null)
                e.Value = Convert.ChangeType(obj.Value, obj.DataType);
        }
    }
}

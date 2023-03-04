using CodingSeb.ExpressionEvaluator;

using System.Linq;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation
{
    public static class ExpressionEvaluatorExtension
    {
        static ExpressionEvaluatorExtension()
        {
            ExpressionEvaluator = new();
            ExpressionEvaluator.PreEvaluateVariable += Evaluator_PreEvaluateVariable;
        }
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

        public static ExpressionEvaluator ExpressionEvaluator;

        /// <summary>
        /// 计算表达式：raw*100
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
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


    }
}

using CodingSeb.ExpressionEvaluator;

namespace ThingsGateway.Foundation
{
    [System.Security.SecuritySafeCritical]
    public static class ExpressionsHelper
    {
        /// <summary>
        /// 计算表达式：raw*100
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static object GetExpressionsResult(this ExpressionEvaluator evaluator, string expressions, object rawvalue)
        {
            if (expressions.IsNullOrEmpty())
            {
                return rawvalue;
            }
            evaluator.Variables = new Dictionary<string, object>()
                {
                  { "Raw", rawvalue},
                  { "raw", rawvalue},
                };
            var value = evaluator.Evaluate(expressions);
            return value;
        }
    }
}
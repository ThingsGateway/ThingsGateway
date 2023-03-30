using NewLife;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 脚本扩展方法
    /// </summary>
    public static class CSharpScriptEngineExtension 
    {
        static CSharpScriptEngine _cSharpScriptEngine;
        static CSharpScriptEngineExtension()
        {
            _cSharpScriptEngine = App.GetService< CSharpScriptEngine>();
        }
        /// <summary>
        /// 获取返回值
        /// </summary>
        public static string GetSciptListValue<T>(this T datas,string script) where T : class
        {
            var inPut = System.Text.Json.JsonSerializer.Serialize(datas);
            if (!script.IsNullOrEmpty())
            {
                //执行脚本，获取新实体
                var outPut = _cSharpScriptEngine.DoList(script, inPut);
                return outPut;
            }
            else
            {
                return inPut;
            }
        }
    }
}


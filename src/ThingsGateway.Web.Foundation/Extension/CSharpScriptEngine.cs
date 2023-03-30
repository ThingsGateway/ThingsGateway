using CSScriptLib;

using NewLife.Caching;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.Dynamic;
using System.Linq;
using System.Text;
namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// C#脚本扩展使用
    ///代码修改自IoTSharp：https://gitee.com/IoTSharp/IoTSharp
    /// </summary>
    public class CSharpScriptEngine : ISingleton
    {
        private ICache _cache;
        /// <summary>
        /// <inheritdoc cref="CSharpScriptEngine"/>
        /// </summary>
        public CSharpScriptEngine()
        {
            _cache = new MemoryCache();
        }

        /// <summary>
        /// 执行脚本获取返回值，通常用于上传实体返回脚本，参数为input
        /// </summary>
        /// <param name="_source"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public string DoList(string _source, string input)
        {
            var runscript = _cache.GetOrAdd(_source, c =>
            {
                var src = _source.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.TrimEntries);
                StringBuilder _using = new StringBuilder();
                StringBuilder _body = new StringBuilder();
                src.ToList().ForEach(l =>
                {
                    if (l.StartsWith("using "))
                    {
                        _using.AppendLine(l);
                    }
                    else
                    {
                        _body.AppendLine(l);
                    }

                });
                var eva = CSScript.Evaluator
                       .CreateDelegate(@$"
                                    using System; 
                                    using System.Collections.Generic; 
                                    {_using}
                                    dynamic  runscript(dynamic   input)
                                    {{
                                      {_body}
                                    }}");
                return eva;
            });
            var expConverter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<List<ExpandoObject>>(input, expConverter);
            dynamic result = runscript(obj);
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            return json;
        }
    }
}


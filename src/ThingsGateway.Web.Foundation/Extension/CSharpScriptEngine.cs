#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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

        /// <summary>
        /// 执行脚本获取返回值，通常用于上传实体返回脚本，参数为input
        /// </summary>
        /// <param name="_source"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Do(string _source, string input)
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
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(input, expConverter);
            dynamic result = runscript(obj);
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            return json;
        }

    }
}


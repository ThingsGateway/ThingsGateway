//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using CSScriptLib;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// C#脚本
/// </summary>
public class CSharpScriptEngine
{
    /// <summary>
    /// 执行脚本获取返回值，通常用于上传实体返回脚本，参数为input
    /// </summary>
    public T Do<T>(string _source) where T : class
    {
        var cacheKey = $"{nameof(CSharpScriptEngine)}-{Do<T>}-{_source}";
        var runscript = NewLife.Caching.Cache.Default.GetOrAdd(cacheKey, c =>
        {
            try
            {
                var eva = CSScript.Evaluator
                  .LoadCode<T>(
@$"
using System;
using System.Linq;
using System.Collections.Generic;
using ThingsGateway.Core.Extension.Json;
using ThingsGateway.Gateway.Application;
{_source}
");
                return eva;
            }
            catch (NullReferenceException)
            {
                throw new Exception("找不到对应的实现类，检查脚本!");
            }
        }, 3600);
        return runscript;
    }
}
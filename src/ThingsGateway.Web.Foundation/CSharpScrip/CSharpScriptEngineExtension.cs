#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion



using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 脚本扩展方法
/// </summary>
public static class CSharpScriptEngineExtension
{
    static CSharpScriptEngine _cSharpScriptEngine;
    static CSharpScriptEngineExtension()
    {
        _cSharpScriptEngine = App.GetService<CSharpScriptEngine>();
    }
    /// <summary>
    /// 获取返回值
    /// </summary>
    public static string GetSciptListValue<T>(this T datas, string script) where T : class
    {
        var inPut = datas.ToJson();
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

    /// <summary>
    /// 获取返回值
    /// </summary>
    public static string GetSciptValue<T>(this T datas, string script) where T : class
    {
        var inPut = datas.ToJson();
        if (!script.IsNullOrEmpty())
        {
            //执行脚本，获取新实体
            var outPut = _cSharpScriptEngine.Do(script, inPut);
            return outPut;
        }
        else
        {
            return inPut;
        }
    }

}


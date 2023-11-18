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

using BlazorComponent;

namespace ThingsGateway.Components;
/// <summary>
/// CookieStorage
/// </summary>
public class CookieStorage
{
    private readonly IJSRuntime _jsRuntime;
    /// <summary>
    /// CookieStorage
    /// </summary>
    /// <param name="jsRuntime"></param>
    public CookieStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    /// <summary>
    /// GetCookieAsync
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<string> GetCookieAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string>(JsInteropConstants.GetCookie, key);
    }
    /// <summary>
    /// GetCookie
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetCookie(string key)
    {
        if (_jsRuntime is IJSInProcessRuntime jsInProcess)
        {
            return jsInProcess.Invoke<string>(JsInteropConstants.GetCookie, key);
        }
        return null;
    }
    /// <summary>
    /// SetItemAsync
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync(JsInteropConstants.SetCookie, key, value);
        }
        catch
        {

        }
    }
}

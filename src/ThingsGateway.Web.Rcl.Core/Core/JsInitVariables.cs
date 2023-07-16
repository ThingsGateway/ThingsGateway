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

using BlazorComponent.I18n;

namespace ThingsGateway.Web.Rcl.Core;

public class JsInitVariables : IAsyncDisposable
{
    static readonly string _timezoneOffsetKey = "timezoneOffset";
    readonly IJSRuntime _jsRuntime;
    readonly CookieStorage _storage;
    TimeSpan _timezoneOffset;
    IJSObjectReference _helper;
    public event Action TimezoneOffsetChanged;

    public TimeSpan TimezoneOffset
    {
        get => _timezoneOffset;
        set
        {
            _storage.SetItemAsync(_timezoneOffsetKey, value.TotalMinutes);
            _timezoneOffset = value;
            TimezoneOffsetChanged?.Invoke();
        }
    }

    public JsInitVariables(IJSRuntime jsRuntime, CookieStorage storage, IHttpContextAccessor httpContextAccessor)
    {
        _jsRuntime = jsRuntime;
        _storage = storage;
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            var timezoneOffsetResult = httpContext.Request.Cookies[_timezoneOffsetKey];
            _timezoneOffset = TimeSpan.FromMinutes(Convert.ToDouble(timezoneOffsetResult));
        }
    }

    public async Task SetTimezoneOffsetAsync()
    {
        var timezoneOffsetResult = await _storage.GetCookieAsync(_timezoneOffsetKey);
        if (string.IsNullOrEmpty(timezoneOffsetResult) is false)
        {
            TimezoneOffset = TimeSpan.FromMinutes(Convert.ToDouble(timezoneOffsetResult));
            return;
        }
        _helper ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", $"{BlazorConst.ResourceUrl}js/jsInitVariables.js");
        var offset = await _helper.InvokeAsync<double>("getTimezoneOffset");
        TimezoneOffset = TimeSpan.FromMinutes(-offset);
    }

    public async ValueTask DisposeAsync()
    {
        if (_helper is not null)
            await _helper.DisposeAsync();
    }
}

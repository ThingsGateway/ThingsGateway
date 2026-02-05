//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Text;

using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

public partial class WebApiTask : AsyncDisposableObject
{
    internal const string LogPath = $"Logs/{nameof(WebApiTask)}";
    private ILog LogMessage;
    private ILogger _logger;
    private TextFileLogger TextLogger;

    public WebApiTask(ILogger logger, WebApiOptions webApiOptions)
    {
        _logger = logger;
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;

        _webApiOptions = webApiOptions;

    }

    private void Log_Out(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_webApiOptions.Enable) return;

        _httpService ??= await GetHttpService().ConfigureAwait(false);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex, "Start");
            }
            finally
            {
                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
            }

        }
    }

    private HttpService? _httpService;
    private WebApiOptions _webApiOptions;


    private async Task<HttpService> GetHttpService()
    {
        var httpService = new HttpService();
#pragma warning disable CA2000 // 丢失范围之前释放对象
        var config = new TouchSocketConfig()
               .SetListenIPHosts(_webApiOptions.ServerUri)
               .ConfigureContainer(a =>
               {
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<ControlController>();
                       store.RegisterServer<RuntimeInfoController>();
                       store.RegisterServer<TestController>();

                       store.RegisterServer<ManagementController>();


                       foreach (var targetType in App.EffectiveTypes.Where(p => typeof(IPluginRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass))
                       {
                           RegisterServer(store, targetType);
                       }

                       foreach (var targetType in GlobalData.PluginService.GetLoadContextAssemblyList().SelectMany(a => a.ExportedTypes).Where(p => typeof(IPluginRpcServer).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass))
                       {
                           RegisterServer(store, targetType);
                       }

                   });

                   //添加跨域服务
                   a.AddCors(corsOption =>
                   {
                       //添加跨域策略，后续使用policyName即可应用跨域策略。
                       corsOption.Add("cors", corsBuilder =>
                       {
                           corsBuilder.AllowAnyMethod()
                               .AllowAnyOrigin();
                       });
                   });


               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();

                   a.Add<AuthenticationPlugin>().SetCredentials(_webApiOptions.UserName, _webApiOptions.Password).SetRealm(nameof(ThingsGateway));

                   a.UseWebApi();

                   //#if DEBUG
                   //                   a.UseSwagger().SetPrefix("api");
                   //#else
                   //                   if (App.WebHostEnvironment.IsDevelopment())
                   //                       a.UseSwagger().SetPrefix("api");
                   //#endif

                   a.UseDefaultHttpServicePlugin();
               });
#pragma warning restore CA2000 // 丢失范围之前释放对象

        await httpService.SetupAsync(config).ConfigureAwait(false);
        return httpService;
    }

    private static void RegisterServer(RpcStore store, Type targetType)
    {
        var baseInterface = typeof(IRpcServer);
        var result = targetType.GetInterfaces()
// 1. 必须继承 IRpcServer
.Where(i => baseInterface.IsAssignableFrom(i))

.Where(i => targetType.GetInterfaceMap(i).TargetType == targetType)

// 3. 接口上带有指定特性并符合 GeneratorFlag
.Where(i =>
{
    var attr = i.GetCustomAttributes(inherit: false).Where(a => a.GetType().Name == nameof(GeneratorRpcProxyAttribute))
                .FirstOrDefault();
    return attr != null;
})
.ToList();
        foreach (var item in result)
        {
            store.RegisterServer(item, targetType);
        }
    }
    private async Task EnsureChannelOpenAsync(CancellationToken cancellationToken)
    {
        if (_httpService.ServerState != ServerState.Running)
        {
            if (_httpService.ServerState != ServerState.Stopped)
                await _httpService.StopAsync(cancellationToken).ConfigureAwait(false);

            await _httpService.StartAsync(cancellationToken).ConfigureAwait(false);
        }

    }


    protected override async Task DisposeAsync(bool disposing)
    {

        if (_httpService != null)
        {
            await _httpService.ClearAsync().ConfigureAwait(false);
            _httpService.SafeDispose();
            _httpService = null;
        }
        await base.DisposeAsync(disposing).ConfigureAwait(false);
        TextLogger?.Dispose();
    }
}

/// <summary>
/// Basic auth 认证插件
/// </summary>
public sealed class AuthenticationPlugin : PluginBase, IHttpPlugin
{
    public string UserName { get; set; } = "admin";
    public string Password { get; set; } = "111111";
    public string Realm { get; set; } = "Server";

    public AuthenticationPlugin SetCredentials(string userName, string password)
    {
        this.UserName = userName;
        this.Password = password;
        return this;
    }
    public AuthenticationPlugin SetRealm(string realm = "Server")
    {
        this.Realm = realm;
        return this;
    }

    private Task Challenge(HttpContextEventArgs e, string message)
    {
        e.Context.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{Realm}\"");
        return e.Context.Response
            .SetStatus(401, message)
            .AnswerAsync();
    }

    public Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        string authorizationHeader = e.Context.Request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authorizationHeader))
            return Challenge(e, "Empty Authorization Header");

        if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return Challenge(e, "Invalid Authorization Header");

        string authBase64 = authorizationHeader.Substring("Basic ".Length).Trim();

        string authString;
        try
        {
            authString = Encoding.UTF8.GetString(Convert.FromBase64String(authBase64));
        }
        catch
        {
            return Challenge(e, "Invalid Base64 Authorization Header");
        }

        var credentials = authString.Split(':');
        if (credentials.Length != 2)
            return Challenge(e, "Invalid Authorization Header");

        var username = credentials[0];
        var password = credentials[1];

        if (username != UserName || password != Password)
            return Challenge(e, "Invalid Username or Password");

        // 验证通过，继续下一个中间件或处理器
        return e.InvokeNext();
    }
}
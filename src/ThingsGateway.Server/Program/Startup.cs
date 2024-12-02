//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Extension;
using ThingsGateway.Logging;
using ThingsGateway.NewLife.Caching;

namespace ThingsGateway.Server;

[AppStartup(-99999)]
public class Startup : AppStartup
{
    public void ConfigBlazorServer(IServiceCollection services)
    {

        // 增加中文编码支持网页源码显示汉字
        services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
        //并发启动/停止host
        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });


        // 事件总线
        services.AddEventBus(options =>
        {

        });

        // 任务调度
        services.AddSchedule(options =>
        {
            options.AddPersistence<JobPersistence>();
        });

        // 缓存
        services.AddSingleton<ICache, MemoryCache>();

        // 允许跨域
        services.AddCorsAccessor();

        // 
        services.AddRazorPages();

        // Json序列化设置
        static void SetNewtonsoftJsonSetting(JsonSerializerSettings setting)
        {
            setting.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            setting.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            // setting.Converters.AddDateTimeTypeConverters(localized: true); // 时间本地化
            //setting.DateFormatString = "yyyy-MM-dd HH:mm:ss"; // 时间格式化
            setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // 忽略循环引用
            // setting.ContractResolver = new CamelCasePropertyNamesContractResolver(); // 解决动态对象属性名大写
            // setting.NullValueHandling = NullValueHandling.Ignore; // 忽略空值
            // setting.Converters.AddLongTypeConverters(); // long转string（防止js精度溢出） 超过17位开启
            // setting.MetadataPropertyHandling = MetadataPropertyHandling.Ignore; // 解决DateTimeOffset异常
            // setting.DateParseHandling = DateParseHandling.None; // 解决DateTimeOffset异常
            // setting.Converters.Add(new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }); // 解决DateTimeOffset异常
        };

        services.AddControllers()
            .AddNewtonsoftJson(options => SetNewtonsoftJsonSetting(options.SerializerSettings))
            //.AddXmlSerializerFormatters()
            //.AddXmlDataContractSerializerFormatters()
            .AddInjectWithUnifyResult<UnifyResultProvider>();


#if NET8_0_OR_GREATER
        services
         .AddRazorComponents(options =>
     {
         options.TemporaryRedirectionUrlValidityDuration = TimeSpan.FromMinutes(10);
     })
         .AddInteractiveServerComponents(options =>
         {
             options.RootComponents.MaxJSRootComponents = 500;
             options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);
             options.MaxBufferedUnacknowledgedRenderBatches = 20;
             options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
         })
         .AddHubOptions(options =>
         {
             //单个传入集线器消息的最大大小。默认 32 KB
             options.MaximumReceiveMessageSize = 1024 * 1024;
             //可为客户端上载流缓冲的最大项数。 如果达到此限制，则会阻止处理调用，直到服务器处理流项。
             options.StreamBufferCapacity = 30;
             options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
             options.KeepAliveInterval = TimeSpan.FromSeconds(15);
             options.HandshakeTimeout = TimeSpan.FromSeconds(30);
         });

#else

        services.AddServerSideBlazor(options =>
        {
            options.RootComponents.MaxJSRootComponents = 500;
            options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);
            options.MaxBufferedUnacknowledgedRenderBatches = 20;
            options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
        }).AddHubOptions(options =>
        {
            //单个传入集线器消息的最大大小。默认 32 KB
            options.MaximumReceiveMessageSize = 1024 * 1024;
            //可为客户端上载流缓冲的最大项数。 如果达到此限制，则会阻止处理调用，直到服务器处理流项。
            options.StreamBufferCapacity = 30;
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.HandshakeTimeout = TimeSpan.FromSeconds(30);
        });

#endif

        // 配置Nginx转发获取客户端真实IP
        // 注1：如果负载均衡不是在本机通过 Loopback 地址转发请求的，一定要加上options.KnownNetworks.Clear()和options.KnownProxies.Clear()
        // 注2：如果设置环境变量 ASPNETCORE_FORWARDEDHEADERS_ENABLED 为 True，则不需要下面的配置代码
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });


        services.AddHealthChecks();


        #region 控制台美化

        services.AddConsoleFormatter(options =>
        {
            options.WriteFilter = (logMsg) =>
            {
                return true;
            };

            options.MessageFormat = (logMsg) =>
            {
                //如果不是LoggingMonitor日志才格式化
                if (logMsg.LogName != "System.Logging.LoggingMonitor")
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("【日志级别】：" + logMsg.LogLevel);
                    stringBuilder.AppendLine("【日志类名】：" + logMsg.LogName);
                    stringBuilder.AppendLine("【日志时间】：" + DateTime.Now.ToDefaultDateTimeFormat());
                    stringBuilder.AppendLine("【日志内容】：" + logMsg.Message);
                    if (logMsg.Exception != null)
                    {
                        stringBuilder.AppendLine("【异常信息】：" + logMsg.Exception);
                    }
                    return stringBuilder.ToString();
                }
                else
                {
                    return logMsg.Message;
                }
            };
            options.WriteHandler = (logMsg, scopeProvider, writer, fmtMsg, opt) =>
            {
                ConsoleColor consoleColor = ConsoleColor.White;
                switch (logMsg.LogLevel)
                {
                    case LogLevel.Information:
                        consoleColor = ConsoleColor.DarkGreen;
                        break;

                    case LogLevel.Warning:
                        consoleColor = ConsoleColor.DarkYellow;
                        break;

                    case LogLevel.Error:
                        consoleColor = ConsoleColor.DarkRed;
                        break;
                }
                writer.WriteWithColor(fmtMsg, ConsoleColor.Black, consoleColor);
            };
        });

        #endregion 控制台美化

        #region api日志

        //Monitor日志配置
        services.AddMonitorLogging(options =>
        {
            options.JsonIndented = true;// 是否美化 JSON
            options.GlobalEnabled = false;//全局启用
            options.ConfigureLogger((logger, logContext, context) =>
            {
                var httpContext = context.HttpContext;//获取httpContext

                //获取头
                var userAgent = httpContext.Request.Headers["User-Agent"];
                if (string.IsNullOrEmpty(userAgent)) userAgent = "Other";//如果没有这个头就指定一个

                //获取客户端信息
                var client = App.GetService<IAppService>().ClientInfo;
                // 获取控制器/操作描述器
                var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                //操作名称默认是控制器名加方法名,自定义操作名称要在action上加Description特性
                var option = $"{controllerActionDescriptor.ControllerName}/{controllerActionDescriptor.ActionName}";

                var desc = App.CreateLocalizerByType(controllerActionDescriptor.ControllerTypeInfo.AsType())[controllerActionDescriptor.MethodInfo.Name];
                //获取特性
                option = desc.Value;//则将操作名称赋值为控制器上写的title

                logContext.Set(LoggingConst.CateGory, option);//传操作名称
                logContext.Set(LoggingConst.Operation, option);//传操作名称
                logContext.Set(LoggingConst.Client, client);//客户端信息
                logContext.Set(LoggingConst.Path, httpContext.Request.Path.Value);//请求地址
                logContext.Set(LoggingConst.Method, httpContext.Request.Method);//请求方法
            });
        });

        //日志写入数据库配置
        services.AddDatabaseLogging<DatabaseLoggingWriter>(options =>
        {
            options.WriteFilter = (logMsg) =>
            {
                return logMsg.LogName == "System.Logging.LoggingMonitor";//只写入LoggingMonitor日志
            };
        });

        #endregion api日志

        //已添加AddOptions
        // 增加多语言支持配置信息
        services.AddRequestLocalization<IOptionsMonitor<BootstrapBlazor.Components.BootstrapBlazorOptions>>((localizerOption, blazorOption) =>
        {
            blazorOption.OnChange(op => Invoke(op));
            Invoke(blazorOption.CurrentValue);

            void Invoke(BootstrapBlazor.Components.BootstrapBlazorOptions option)
            {
                var supportedCultures = option.GetSupportedCultures();
                localizerOption.SupportedCultures = supportedCultures;
                localizerOption.SupportedUICultures = supportedCultures;
            }
        });

        services.AddScoped<BlazorAppContext>(a =>
        {
            var appContext = new BlazorAppContext(
             a.GetService<ISysResourceService>(),
             a.GetService<IUserCenterService>(),
             a.GetService<ISysUserService>());
            appContext.TitleLocalizer = a.GetRequiredService<IStringLocalizer<ThingsGateway.Razor.MainLayout>>();

            return appContext;
        });

        services.AddHttpContextAccessor();

        //添加cookie授权
        var authenticationBuilder = services.AddAuthentication(Assembly.GetEntryAssembly().GetName().Name).AddCookie(Assembly.GetEntryAssembly().GetName().Name, a =>
        {
            a.AccessDeniedPath = "/Account/AccessDenied/";
            a.LogoutPath = "/Account/Logout/";
            a.LoginPath = "/Account/Login/";
        });

        // 添加jwt授权
        authenticationBuilder.AddJwt();

        services.AddAuthorization();
#if NET8_0_OR_GREATER
        services.AddCascadingAuthenticationState();
#endif
        services.AddScoped<IAuthorizationHandler, BlazorAuthenticationStateProvider>();

    }



    public void Use(IApplicationBuilder applicationBuilder, IWebHostEnvironment env)
    {
        var app = (WebApplication)applicationBuilder;
        app.UseBootstrapBlazor();

        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

        // 启用本地化
        var option = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
        if (option != null)
        {
            app.UseRequestLocalization(option.Value);
        }

        // 任务调度看板
        app.UseScheduleUI(options =>
        {
            options.RequestPath = "/schedule";  // 必须以 / 开头且不以 / 结尾
            options.DisableOnProduction = true; // 生产环境关闭
            options.DisplayEmptyTriggerJobs = true; // 是否显示空作业触发器的作业
            options.DisplayHead = false; // 是否显示页头
            options.DefaultExpandAllJobs = false; // 是否默认展开所有作业
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = ctx => ctx.ProcessCache(app.Configuration) });
        }


        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = (stf) =>
            {
                stf.ProcessCache(app.Configuration);
                stf.Context.Response.Headers.AccessControlAllowOrigin = "*";
                stf.Context.Response.Headers.AccessControlAllowHeaders = "*";
            }
        });
        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".properties"] = "application/octet-stream";
        provider.Mappings[".moc"] = "application/x-msdownload";
        provider.Mappings[".moc3"] = "application/x-msdownload";
        provider.Mappings[".mtn"] = "application/x-msdownload";

        app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
        app.UseStaticFiles();

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("ThingsGateway", "ThingsGateway");
            await next().ConfigureAwait(false);
        });


        // 特定文件类型（文件后缀）处理
        var contentTypeProvider = GetFileExtensionContentTypeProvider();
        // contentTypeProvider.Mappings[".文件后缀"] = "MIME 类型";
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = contentTypeProvider
        });

        //// 启用HTTPS
        //app.UseHttpsRedirection();


        // 添加状态码拦截中间件
        app.UseUnifyResultStatusCodes();

        // 路由注册
        app.UseRouting();

        // 启用跨域，必须在 UseRouting 和 UseAuthentication 之间注册
        app.UseCorsAccessor();

        // 启用鉴权授权
        app.UseAuthentication();
        app.UseAuthorization();

        // 任务调度看板
        app.UseScheduleUI(options =>
        {
            options.RequestPath = "/schedule";  // 必须以 / 开头且不以 / 结尾
            options.DisableOnProduction = true; // 生产环境关闭
            options.DisplayEmptyTriggerJobs = true; // 是否显示空作业触发器的作业
            options.DisplayHead = false; // 是否显示页头
            options.DefaultExpandAllJobs = false; // 是否默认展开所有作业
        });

        app.UseInject();

#if NET8_0_OR_GREATER
        app.UseAntiforgery();
#endif
        app.MapControllers();

    }

    /// <summary>
    /// 初始化文件 ContentType 提供器
    /// </summary>
    /// <returns></returns>
    private static FileExtensionContentTypeProvider GetFileExtensionContentTypeProvider()
    {
        var fileExtensionProvider = new FileExtensionContentTypeProvider();
        fileExtensionProvider.Mappings[".iec"] = "application/octet-stream";
        fileExtensionProvider.Mappings[".patch"] = "application/octet-stream";
        fileExtensionProvider.Mappings[".apk"] = "application/vnd.android.package-archive";
        fileExtensionProvider.Mappings[".pem"] = "application/x-x509-user-cert";
        fileExtensionProvider.Mappings[".gzip"] = "application/x-gzip";
        fileExtensionProvider.Mappings[".7zip"] = "application/zip";
        fileExtensionProvider.Mappings[".jpg2"] = "image/jp2";
        fileExtensionProvider.Mappings[".et"] = "application/kset";
        fileExtensionProvider.Mappings[".dps"] = "application/ksdps";
        fileExtensionProvider.Mappings[".cdr"] = "application/x-coreldraw";
        fileExtensionProvider.Mappings[".shtml"] = "text/html";
        fileExtensionProvider.Mappings[".php"] = "application/x-httpd-php";
        fileExtensionProvider.Mappings[".php3"] = "application/x-httpd-php";
        fileExtensionProvider.Mappings[".php4"] = "application/x-httpd-php";
        fileExtensionProvider.Mappings[".phtml"] = "application/x-httpd-php";
        fileExtensionProvider.Mappings[".pcd"] = "image/x-photo-cd";
        fileExtensionProvider.Mappings[".bcmap"] = "application/octet-stream";
        fileExtensionProvider.Mappings[".properties"] = "application/octet-stream";
        fileExtensionProvider.Mappings[".m3u8"] = "application/x-mpegURL";
        return fileExtensionProvider;
    }
}

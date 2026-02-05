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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Common;
using ThingsGateway.DB;
using ThingsGateway.Debug;
using ThingsGateway.Foundation.Common;
using ThingsGateway.Gateway.Razor;
using ThingsGateway.VirtualFileServer;

namespace ThingsGateway.Server;

[AppStartup(-999999)]
public class Startup : AppStartup
{
    /// <summary>
    /// 添加网站服务
    /// </summary>
    /// <param name="services"></param>
    public void AddWebSiteServices(IServiceCollection services)
    {
        services.AddSingleton<IAuthRazorService, HybridAuthRazorService>();
        services.AddSingleton<HybridAppService>();
        services.AddSingleton<IAppService, HybridAppService>(a => a.GetService<HybridAppService>());
        services.AddSingleton<IReturnUrlService>(a => a.GetService<IAppService>());
        services.AddSingleton<IClaimsPrincipalService, HybridClaimsPrincipalService>();

        services.AddScoped<IPlatformService, HybridPlatformService>();
        services.AddScoped<IDownloadPlatformService, HybridDownloadPlatformService>();
        services.AddScoped<IGatewayExportService, HybridGatewayExportService>();
    }

    public void ConfigBlazor(IServiceCollection services)
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
        services.AddSchedule(options => options.AddPersistence<JobPersistence>());

        // 允许跨域
        services.AddCorsAccessor();
        //services.AddRazorPages(); hybrid

        // Json序列化设置
        static void SetNewtonsoftJsonSetting(JsonSerializerSettings setting)
        {
            setting.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            setting.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            // setting.Converters.AddDateTimeTypeConverters(localized: true); // 时间本地化
            //setting.DateFormatString = "yyyy-MM-dd HH:mm:ss"; // 时间格式化
            setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // 忽略循环引用

            // setting.ContractResolver = new CamelCasePropertyNamesContractResolver(); // 解决动态对象属性名大写
            setting.NullValueHandling = NullValueHandling.Ignore; // 忽略空值
            // setting.Converters.AddLongTypeConverters(); // long转string（防止js精度溢出） 超过17位开启
            // setting.MetadataPropertyHandling = MetadataPropertyHandling.Ignore; // 解决DateTimeOffset异常
            // setting.DateParseHandling = DateParseHandling.None; // 解决DateTimeOffset异常
            // setting.Converters.Add(new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }); // 解决DateTimeOffset异常
        }

        services.AddMvcFilter<RequestAuditFilter>();

        services.AddControllers()
            .AddNewtonsoftJson(options => SetNewtonsoftJsonSetting(options.SerializerSettings))
            //.AddXmlSerializerFormatters()
            //.AddXmlDataContractSerializerFormatters()
            .AddInjectWithUnifyResult<UnifyResultProvider>();

        // 配置Nginx转发获取客户端真实IP
        // 注1：如果负载均衡不是在本机通过 Loopback 地址转发请求的，一定要加上options.KnownNetworks.Clear()和options.KnownProxies.Clear()
        // 注2：如果设置环境变量 ASPNETCORE_FORWARDEDHEADERS_ENABLED 为 True，则不需要下面的配置代码
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
#if NET10_0_OR_GREATER
            options.KnownIPNetworks.Clear();
#else
            options.KnownNetworks.Clear();
#endif
            options.KnownProxies.Clear();
        });

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
            appContext.TitleLocalizer = a.GetRequiredService<IStringLocalizer<MainLayout>>();
            return appContext;
        });

        services.AddHttpContextAccessor();

        //添加cookie授权
        var authenticationBuilder = services.AddAuthentication(ClaimConst.Scheme).AddCookie(ClaimConst.Scheme, a =>
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

        #region hybrid
        services.AddAuthorizationCore();
        services.AddSingleton<IAuthorizationHandler, BlazorHybridAuthorizationHandler>();
        services.AddSingleton<AuthenticationStateProvider, BlazorHybridAuthenticationStateProvider>();

        services.AddSignalR();
        #endregion

        if (!Runtime.IsLegacyWindows)
        {
#pragma warning disable CA2000 // 丢失范围之前释放对象
#if NET9_0_OR_GREATER
            var certificate = X509CertificateLoader.LoadPkcs12FromFile("ThingsGateway.pfx", "ThingsGateway", X509KeyStorageFlags.EphemeralKeySet);
#else
            var certificate = new X509Certificate2("ThingsGateway.pfx", "ThingsGateway", X509KeyStorageFlags.EphemeralKeySet);
#pragma warning restore CA2000 // 丢失范围之前释放对象
#endif
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("Keys"))
                .ProtectKeysWithCertificate(certificate)
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
        }
    }

    public void Use(IApplicationBuilder applicationBuilder, IWebHostEnvironment env)
    {
        var app = (WebApplication)applicationBuilder;
        app.UseStatusCodePagesWithReExecute("/notfound", "?statusCode={0}");
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.All,
#if NET10_0_OR_GREATER
            KnownIPNetworks = { },
#else
            KnownNetworks = { },
#endif
            KnownProxies = { }
        });
        app.UseBootstrapBlazor();

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

        // 特定文件类型（文件后缀）处理
        var contentTypeProvider = FS.GetFileExtensionContentTypeProvider();
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
        //app.UseAntiforgery();
#endif

        app.MapControllers();
        app.MapHubs();
    }
}

using AspNetCoreRateLimit;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WebEncoders;

using Newtonsoft.Json;

using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace ThingsGateway.Web.Core
{
    public class Startup : AppStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // 启用HTTPS
            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            // 任务调度看板
            app.UseScheduleUI();

            // 限流服务
            app.UseIpRateLimiting();

            app.UseInject("api");


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();

                endpoints.MapHubs();

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 允许跨域
            services.AddCorsAccessor();

            services.Configure<WebEncoderOptions>(options =>
            options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

            // 限流服务
            services.Configure<IpRateLimitOptions>(App.Configuration.GetSection("IpRateLimiting"));
            services.AddInMemoryRateLimiting();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // 任务调度
            services.AddSchedule(options =>
            {
                options.AddPersistence<JobPersistence>();
            });

            //启动LoggingMonitor操作日志写入数据库组件
            services.AddComponent<LoggingMonitorComponent>();

            //认证组件
            services.AddComponent<AuthComponent>();

            //启动Web设置SignalRComponent组件
            services.AddComponent<SignalRComponent>();

            services.AddRazorPages();
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
             .AddUnifyResult<UnifyResultProvider>()
             .AddFriendlyException()
            .AddInject()
            .AddDataValidation()
                ;

            services.AddServerSideBlazor().AddHubOptions(options => options.MaximumReceiveMessageSize = 64 * 1024); ;
            services.AddHealthChecks();

        }
    }
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.WebEncoders;

using System.Text.Encodings.Web;
using System.Text.Unicode;

using ThingsGateway.Components;

namespace ThingsGateway.Demo.Web;

/// <inheritdoc/>
public class Startup
{
    /// <inheritdoc/>
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPlatformService, PlatformService>();
        services.AddComponents();

        services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

        services.AddRazorPages();

        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        //services.AddEndpointsApiExplorer();
        //services.AddSwaggerGen();

        services.AddServerSideBlazor(options =>
        {
            options.RootComponents.MaxJSRootComponents = 500;
        }).AddHubOptions(options => options.MaximumReceiveMessageSize = 64 * 1024);

        services.AddHealthChecks();
    }

    /// <inheritdoc/>
    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //app.UseSwagger();
            //app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        // 启用HTTPS
        //app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();

            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;

using System.Reflection;

using ThingsGateway.Authorization;
using ThingsGateway.DataEncryption;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// JWT 授权服务拓展类
/// </summary>
public static class JWTAuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// 添加 JWT 授权
    /// </summary>
    /// <param name="authenticationBuilder"></param>
    /// <param name="tokenValidationParameters">token 验证参数</param>
    /// <param name="jwtBearerConfigure"></param>
    /// <param name="enableGlobalAuthorize">启动全局授权</param>
    /// <returns></returns>
    public static AuthenticationBuilder AddJwt(this AuthenticationBuilder authenticationBuilder, object tokenValidationParameters = default, Action<JwtBearerOptions> jwtBearerConfigure = null, bool enableGlobalAuthorize = false)
    {
        // 获取框架上下文
        _ = JWTEncryption.GetFrameworkContext(Assembly.GetCallingAssembly());

        // 配置 JWT 选项
        ConfigureJWTOptions(authenticationBuilder.Services);

        // 添加授权
        authenticationBuilder.AddJwtBearer(options =>
        {
            // 反射获取全局配置
            var jwtSettings = JWTEncryption.FrameworkApp.GetMethod("GetOptions").MakeGenericMethod(typeof(JWTSettingsOptions)).Invoke(null, new object[] { null }) as JWTSettingsOptions;

            // 配置 JWT 验证信息
            options.TokenValidationParameters = (tokenValidationParameters as TokenValidationParameters) ?? JWTEncryption.CreateTokenValidationParameters(jwtSettings);

            // 添加自定义配置
            jwtBearerConfigure?.Invoke(options);
        });

        //启用全局授权
        if (enableGlobalAuthorize)
        {
            authenticationBuilder.Services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new AuthorizeFilter());
            });
        }

        return authenticationBuilder;
    }

    /// <summary>
    /// 添加 JWT 授权
    /// </summary>
    /// <param name="services"></param>
    /// <param name="authenticationConfigure">授权配置</param>
    /// <param name="tokenValidationParameters">token 验证参数</param>
    /// <param name="jwtBearerConfigure"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddJwt(this IServiceCollection services, Action<AuthenticationOptions> authenticationConfigure = null, object tokenValidationParameters = default, Action<JwtBearerOptions> jwtBearerConfigure = null)
    {
        // 获取框架上下文
        _ = JWTEncryption.GetFrameworkContext(Assembly.GetCallingAssembly());

        // 添加默认授权
        var authenticationBuilder = services.AddAuthentication(options =>
         {
             options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
             options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

             // 添加自定义配置
             authenticationConfigure?.Invoke(options);
         });

        AddJwt(authenticationBuilder, tokenValidationParameters, jwtBearerConfigure);

        return authenticationBuilder;
    }

    /// <summary>
    /// 添加 JWT 授权
    /// </summary>
    /// <typeparam name="TAuthorizationHandler"></typeparam>
    /// <param name="services"></param>
    /// <param name="authenticationConfigure"></param>
    /// <param name="tokenValidationParameters"></param>
    /// <param name="jwtBearerConfigure"></param>
    /// <param name="enableGlobalAuthorize"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddJwt<TAuthorizationHandler>(this IServiceCollection services, Action<AuthenticationOptions> authenticationConfigure = null, object tokenValidationParameters = default, Action<JwtBearerOptions> jwtBearerConfigure = null, bool enableGlobalAuthorize = false)
        where TAuthorizationHandler : class, IAuthorizationHandler
    {
        // 植入框架
        var furionAssembly = JWTEncryption.GetFrameworkContext(Assembly.GetCallingAssembly());

        // 获取添加授权类型
        var authorizationServiceCollectionExtensionsType = furionAssembly.GetType("Microsoft.Extensions.DependencyInjection.AuthorizationServiceCollectionExtensions");
        var addAppAuthorizationMethod = authorizationServiceCollectionExtensionsType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(u => u.Name == "AddAppAuthorization" && u.IsGenericMethod && u.GetParameters().Length > 0 && u.GetParameters()[0].ParameterType == typeof(IServiceCollection)).First();

        // 添加策略授权服务
        addAppAuthorizationMethod.MakeGenericMethod(typeof(TAuthorizationHandler)).Invoke(null, new object[] { services, null, enableGlobalAuthorize });

        // 添加授权
        return services.AddJwt(authenticationConfigure, tokenValidationParameters, jwtBearerConfigure);
    }

    /// <summary>
    /// 添加 JWT 授权
    /// </summary>
    /// <param name="services"></param>
    private static void ConfigureJWTOptions(IServiceCollection services)
    {
        // 配置验证
        services.AddOptions<JWTSettingsOptions>()
                .BindConfiguration("JWTSettings")
                .ValidateDataAnnotations()
                .PostConfigure(options =>
                {
                    _ = JWTEncryption.SetDefaultJwtSettings(options);
                });
    }
}
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using ThingsGateway.Authorization;

using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ThingsGateway.DataEncryption;

/// <summary>
/// JWT 加解密
/// </summary>
public class JWTEncryption
{
    /// <summary>
    /// 刷新 Token 身份标识
    /// </summary>
    private static readonly string[] _refreshTokenClaims = new[] { "f", "e", "s", "l", "k" };

    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="expiredTime">过期时间（分钟），最大支持 13 年</param>
    /// <returns></returns>
    public static string Encrypt(IDictionary<string, object> payload, long? expiredTime = null)
    {
        var (Payload, JWTSettings) = CombinePayload(payload, expiredTime);
        return Encrypt(JWTSettings.IssuerSigningKey, Payload, JWTSettings.Algorithm);
    }
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="issuerSigningKey"></param>
    /// <param name="payload"></param>
    /// <param name="algorithm"></param>
    /// <returns></returns>
    public static string Encrypt(string issuerSigningKey, IDictionary<string, object> payload, string algorithm = SecurityAlgorithms.HmacSha256)
    {
        // 处理 JwtPayload 序列化不一致问题
        var stringPayload = payload is JwtPayload jwtPayload ? jwtPayload.SerializeToJson() : JsonSerializer.Serialize(payload, _jsonSerializerOptions);
        return Encrypt(issuerSigningKey, stringPayload, algorithm);
    }

    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="issuerSigningKey"></param>
    /// <param name="payload"></param>
    /// <param name="algorithm"></param>
    /// <returns></returns>
    public static string Encrypt(string issuerSigningKey, string payload, string algorithm = SecurityAlgorithms.HmacSha256)
    {
        SigningCredentials credentials = null;

        if (!string.IsNullOrWhiteSpace(issuerSigningKey))
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));
            credentials = new SigningCredentials(securityKey, algorithm);
        }

        var tokenHandler = new JsonWebTokenHandler();
        return credentials == null ? tokenHandler.CreateToken(payload) : tokenHandler.CreateToken(payload, credentials);
    }

    /// <summary>
    /// 生成刷新 Token
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="expiredTime">刷新 Token 有效期（分钟），最大支持 13 年</param>
    /// <returns></returns>
    public static string GenerateRefreshToken(string accessToken, int expiredTime = 43200)
    {
        // 分割Token
        var tokenParagraphs = accessToken.Split('.', StringSplitOptions.RemoveEmptyEntries);

        var s = RandomNumberGenerator.GetInt32(10, tokenParagraphs[1].Length / 2 + 2);
        var l = RandomNumberGenerator.GetInt32(3, 13);

        var payload = new Dictionary<string, object>
            {
                { "f",tokenParagraphs[0] },
                { "e",tokenParagraphs[2] },
                { "s",s },
                { "l",l },
                { "k",tokenParagraphs[1].Substring(s,l) }
            };

        return Encrypt(payload, expiredTime);
    }

    /// <summary>
    /// 通过过期Token 和 刷新Token 换取新的 Token
    /// </summary>
    /// <param name="expiredToken"></param>
    /// <param name="refreshToken"></param>
    /// <param name="expiredTime">过期时间（分钟），最大支持 13 年</param>
    /// <param name="clockSkew">刷新token容差值，秒做单位</param>
    /// <returns></returns>
    public static async Task<string> Exchange(string expiredToken, string refreshToken, long? expiredTime = null, long clockSkew = 5)
    {
        // 交换刷新Token 必须原Token 已过期
        var (_isValid, _, _) = await Validate(expiredToken).ConfigureAwait(false);
        if (_isValid) return default;

        // 判断刷新Token 是否过期
        var (isValid, refreshTokenObj, _) = await Validate(refreshToken).ConfigureAwait(false);
        if (!isValid) return default;

        // 解析 HttpContext
        var httpContext = GetCurrentHttpContext();

        // 判断这个刷新Token 是否已刷新过
        var blacklistRefreshKey = "BLACKLIST_REFRESH_TOKEN:" + refreshToken;
        var distributedCache = httpContext?.RequestServices?.GetRequiredService<IDistributedCache>();

        // 处理token并发容错问题
        var nowTime = DateTimeOffset.UtcNow;
        var cachedValue = await distributedCache.GetStringAsync(blacklistRefreshKey).ConfigureAwait(false);
        var isRefresh = !string.IsNullOrWhiteSpace(cachedValue);    // 判断是否刷新过
        if (isRefresh)
        {
            var refreshTime = new DateTimeOffset(long.Parse(cachedValue), TimeSpan.Zero);
            // 处理并发时容差值
            if ((nowTime - refreshTime).TotalSeconds > clockSkew) return default;
        }

        // 分割过期Token
        var tokenParagraphs = expiredToken.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (tokenParagraphs.Length < 3) return default;

        // 判断各个部分是否匹配
        if (!refreshTokenObj.GetPayloadValue<string>("f").Equals(tokenParagraphs[0])) return default;
        if (!refreshTokenObj.GetPayloadValue<string>("e").Equals(tokenParagraphs[2])) return default;
        if (!tokenParagraphs[1].Substring(refreshTokenObj.GetPayloadValue<int>("s"), refreshTokenObj.GetPayloadValue<int>("l")).Equals(refreshTokenObj.GetPayloadValue<string>("k"))) return default;

        // 获取过期 Token 的存储信息
        var jwtSecurityToken = SecurityReadJwtToken(expiredToken);
        var payload = jwtSecurityToken.Payload;

        // 移除 Iat，Nbf，Exp
        foreach (var innerKey in DateTypeClaimTypes)
        {
            if (!payload.ContainsKey(innerKey)) continue;

            payload.Remove(innerKey);
        }

        // 交换成功后登记刷新Token，标记失效
        if (!isRefresh)
        {
            await distributedCache.SetStringAsync(blacklistRefreshKey, nowTime.Ticks.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(refreshTokenObj.GetPayloadValue<long>(JwtRegisteredClaimNames.Exp))
            }).ConfigureAwait(false);
        }

        return Encrypt(payload, expiredTime);
    }

    /// <summary>
    /// 自动刷新 Token 信息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="httpContext"></param>
    /// <param name="expiredTime">新 Token 过期时间（分钟），最大支持 13 年</param>
    /// <param name="refreshTokenExpiredTime">新刷新 Token 有效期（分钟）</param>
    /// <param name="tokenPrefix"></param>
    /// <param name="clockSkew"></param>
    /// <returns></returns>
    public static async Task<bool> AutoRefreshToken(AuthorizationHandlerContext context, DefaultHttpContext httpContext, long? expiredTime = null, int refreshTokenExpiredTime = 43200, string tokenPrefix = "Bearer ", long clockSkew = 5)
    {
        // 如果验证有效，则跳过刷新
        if (context.User.Identity.IsAuthenticated)
        {
            // 禁止使用刷新 Token 进行单独校验
            if (_refreshTokenClaims.All(k => context.User.Claims.Any(c => c.Type == k)))
            {
                return false;
            }

            return true;
        }

        // 判断是否含有匿名特性
        if (httpContext.GetEndpoint()?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null) return true;

        // 获取过期Token 和 刷新Token
        var expiredToken = GetJwtBearerToken(httpContext, tokenPrefix: tokenPrefix);
        var refreshToken = GetJwtBearerToken(httpContext, "X-Authorization", tokenPrefix: tokenPrefix);
        if (string.IsNullOrWhiteSpace(expiredToken) || string.IsNullOrWhiteSpace(refreshToken)) return false;

        // 交换新的 Token
        var accessToken = await Exchange(expiredToken, refreshToken, expiredTime, clockSkew).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(accessToken)) return false;

        // 读取新的 Token Clamis
        var claims = ReadJwtToken(accessToken)?.Claims;
        if (claims == null) return false;

        // 创建身份信息
        var claimIdentity = new ClaimsIdentity("AuthenticationTypes.Federation");
        claimIdentity.AddClaims(claims);
        var claimsPrincipal = new ClaimsPrincipal(claimIdentity);

        // 设置 HttpContext.User 并登录
        httpContext.User = claimsPrincipal;
        await httpContext.SignInAsync(claimsPrincipal).ConfigureAwait(false);

        string accessTokenKey = "access-token"
             , xAccessTokenKey = "x-access-token"
             , accessControlExposeKey = "Access-Control-Expose-Headers";

        // 返回新的 Token
        httpContext.Response.Headers[accessTokenKey] = accessToken;
        // 返回新的 刷新Token
        httpContext.Response.Headers[xAccessTokenKey] = GenerateRefreshToken(accessToken, refreshTokenExpiredTime);

        // 处理 axios 问题
        httpContext.Response.Headers.TryGetValue(accessControlExposeKey, out var acehs);
        httpContext.Response.Headers[accessControlExposeKey] = string.Join(',', StringValues.Concat(acehs, new StringValues(new[] { accessTokenKey, xAccessTokenKey })).Distinct());

        return true;
    }

    /// <summary>
    /// 验证 Token
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public static async Task<(bool IsValid, JsonWebToken Token, TokenValidationResult validationResult)> Validate(string accessToken)
    {
        var jwtSettings = GetJWTSettings();
        if (jwtSettings == null) return (false, default, default);

        // 加密Key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey));
        var creds = new SigningCredentials(key, jwtSettings.Algorithm);

        // 创建Token验证参数
        var tokenValidationParameters = CreateTokenValidationParameters(jwtSettings);
        tokenValidationParameters.IssuerSigningKey ??= creds.Key;

        // 验证 Token
        var tokenHandler = new JsonWebTokenHandler();
        try
        {
            var tokenValidationResult = await tokenHandler.ValidateTokenAsync(accessToken, tokenValidationParameters).ConfigureAwait(false);
            if (!tokenValidationResult.IsValid) return (false, null, tokenValidationResult);

            var jsonWebToken = tokenValidationResult.SecurityToken as JsonWebToken;
            return (true, jsonWebToken, tokenValidationResult);
        }
        catch
        {
            return (false, default, default);
        }
    }

    /// <summary>
    /// 验证 Token
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="headerKey"></param>
    /// <param name="tokenPrefix"></param>
    /// <returns></returns>
    public static async Task<(bool, JsonWebToken)> ValidateJwtBearerToken(DefaultHttpContext httpContext, string headerKey = "Authorization", string tokenPrefix = "Bearer ")
    {
        // 获取 token
        var accessToken = GetJwtBearerToken(httpContext, headerKey, tokenPrefix);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return (false, null);
        }

        // 验证token
        var (IsValid, Token, _) = await Validate(accessToken).ConfigureAwait(false);
        var token = IsValid ? Token : null;

        return (IsValid, token);
    }

    /// <summary>
    /// 读取 Token，不含验证
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public static JsonWebToken ReadJwtToken(string accessToken)
    {
        var tokenHandler = new JsonWebTokenHandler();
        if (tokenHandler.CanReadToken(accessToken))
        {
            return tokenHandler.ReadJsonWebToken(accessToken);
        }

        return default;
    }

    /// <summary>
    /// 读取 Token，不含验证
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public static JwtSecurityToken SecurityReadJwtToken(string accessToken)
    {
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(accessToken);
        return jwtSecurityToken;
    }

    /// <summary>
    /// 获取 JWT Bearer Token
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="headerKey"></param>
    /// <param name="tokenPrefix"></param>
    /// <returns></returns>
    public static string GetJwtBearerToken(DefaultHttpContext httpContext, string headerKey = "Authorization", string tokenPrefix = "Bearer ")
    {
        // 判断请求报文头中是否有 "Authorization" 报文头
        var bearerToken = httpContext.Request.Headers[headerKey].ToString();
        if (string.IsNullOrWhiteSpace(bearerToken)) return default;

        var prefixLenght = tokenPrefix.Length;
        return bearerToken.StartsWith(tokenPrefix, true, null) && bearerToken.Length > prefixLenght ? bearerToken[prefixLenght..].Trim() : default;
    }

    /// <summary>
    /// 获取 JWT 配置
    /// </summary>
    /// <returns></returns>
    public static JWTSettingsOptions GetJWTSettings()
    {
        // 获取框架上下文
        _ = GetFrameworkContext(Assembly.GetCallingAssembly());

        if (FrameworkApp == null)
        {
            Debug.WriteLine("No register the code `services.AddJwt()` on Startup.cs.");
        }

        var jwtSettingsOptions = FrameworkApp.GetMethod("GetOptions").MakeGenericMethod(typeof(JWTSettingsOptions)).Invoke(null, new object[] { null }) as JWTSettingsOptions;
        if (jwtSettingsOptions.Algorithm == null && jwtSettingsOptions.ExpiredTime == null)
        {
            SetDefaultJwtSettings(jwtSettingsOptions);
        }
        return jwtSettingsOptions;
    }

    /// <summary>
    /// 生成Token验证参数
    /// </summary>
    /// <param name="jwtSettings"></param>
    /// <returns></returns>
    public static TokenValidationParameters CreateTokenValidationParameters(JWTSettingsOptions jwtSettings)
    {
        return new TokenValidationParameters
        {
            // 验证签发方密钥
            ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey.Value,
            // 签发方密钥
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey)),
            // 验证签发方
            ValidateIssuer = jwtSettings.ValidateIssuer.Value,
            // 设置签发方
            ValidIssuer = jwtSettings.ValidIssuer,
            // 验证签收方
            ValidateAudience = jwtSettings.ValidateAudience.Value,
            // 设置接收方
            ValidAudience = jwtSettings.ValidAudience,
            // 验证生存期
            ValidateLifetime = jwtSettings.ValidateLifetime.Value,
            // 过期时间容错值
            ClockSkew = TimeSpan.FromSeconds(jwtSettings.ClockSkew.Value),
            // 验证过期时间，设置 false 永不过期
            RequireExpirationTime = jwtSettings.RequireExpirationTime
        };
    }

    /// <summary>
    /// 组合 Claims 负荷
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="expiredTime">过期时间，单位：分钟，最大支持 13 年</param>
    /// <returns></returns>
    private static (IDictionary<string, object> Payload, JWTSettingsOptions JWTSettings) CombinePayload(IDictionary<string, object> payload, long? expiredTime = null)
    {
        var jwtSettings = GetJWTSettings();
        var datetimeOffset = DateTimeOffset.UtcNow;

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Iat))
        {
            payload.Add(JwtRegisteredClaimNames.Iat, datetimeOffset.ToUnixTimeSeconds());
        }

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Nbf))
        {
            payload.Add(JwtRegisteredClaimNames.Nbf, datetimeOffset.ToUnixTimeSeconds());
        }

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Exp))
        {
            var minute = expiredTime ?? jwtSettings?.ExpiredTime ?? 20;
            payload.Add(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(minute).ToUnixTimeSeconds());
        }

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Iss))
        {
            payload.Add(JwtRegisteredClaimNames.Iss, jwtSettings?.ValidIssuer);
        }

        if (!payload.ContainsKey(JwtRegisteredClaimNames.Aud))
        {
            payload.Add(JwtRegisteredClaimNames.Aud, jwtSettings?.ValidAudience);
        }

        return (payload, jwtSettings);
    }

    /// <summary>
    /// 设置默认 Jwt 配置
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    internal static JWTSettingsOptions SetDefaultJwtSettings(JWTSettingsOptions options)
    {
        options.ValidateIssuerSigningKey ??= true;
        if (options.ValidateIssuerSigningKey == true)
        {
            options.IssuerSigningKey ??= "U2FsdGVkX1+6H3D8Q//yQMhInzTdRZI9DbUGetbyaag=";
        }
        options.ValidateIssuer ??= true;
        if (options.ValidateIssuer == true)
        {
            options.ValidIssuer ??= "diego";
        }
        options.ValidateAudience ??= true;
        if (options.ValidateAudience == true)
        {
            options.ValidAudience ??= "powerby ThingsGateway";
        }
        options.ValidateLifetime ??= true;
        if (options.ValidateLifetime == true)
        {
            options.ClockSkew ??= 10;
        }
        options.ExpiredTime ??= 20;
        options.Algorithm ??= SecurityAlgorithms.HmacSha256;

        return options;
    }

    /// <summary>
    /// 获取当前的 HttpContext
    /// </summary>
    /// <returns></returns>
    private static HttpContext GetCurrentHttpContext()
    {
        return FrameworkApp.GetProperty("HttpContext").GetValue(null) as HttpContext;
    }

    /// <summary>
    /// 日期类型的 Claim 类型
    /// </summary>
    private static readonly string[] DateTypeClaimTypes = new[] { JwtRegisteredClaimNames.Iat, JwtRegisteredClaimNames.Nbf, JwtRegisteredClaimNames.Exp };

    /// <summary>
    /// 框架 App 静态类
    /// </summary>
    internal static Type FrameworkApp { get; set; }

    /// <summary>
    /// 获取框架上下文
    /// </summary>
    /// <returns></returns>
    internal static Assembly GetFrameworkContext(Assembly callAssembly)
    {
        if (FrameworkApp != null) return FrameworkApp.Assembly;

        // 修复不注册 AddJwt 服务不能使用 JWT 加密问题
        var executeAssembly = callAssembly == typeof(JWTEncryption).Assembly
            ? Assembly.GetEntryAssembly()
            : callAssembly;

        // 获取 程序集名称
        var furionAssemblyName = executeAssembly.GetReferencedAssemblies()
            .FirstOrDefault(u => u.Name == "ThingsGateway" || u.Name == "ThingsGateway.Furion")
            ?? throw new InvalidOperationException("No `ThingsGateway` assembly installed in the current project was detected.");

        // 加载 程序集
        var furionAssembly = AssemblyLoadContext.Default.LoadFromAssemblyName(furionAssemblyName);

        // 获取 App 静态类
        FrameworkApp = furionAssembly.GetType("ThingsGateway.App");

        return furionAssembly;
    }
}
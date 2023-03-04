using NewLife.Caching;

namespace ThingsGateway.Core
{
    public static class CacheSetup
    {
        /// <summary>
        /// 缓存注册,注意是单例
        /// </summary>
        /// <param name="services"></param>
        public static void AddCache(this IServiceCollection services)
        {
            services.AddSingleton(options =>
            {
                Cache.Default.Expire = 60 * 60 * 24;
                return Cache.Default;
            });
        }
    }
}
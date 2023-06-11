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
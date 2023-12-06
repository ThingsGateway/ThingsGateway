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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// DateHandleAdapterExtension
    /// </summary>
    public static class DataHandlingAdapterExtension
    {
        #region SingleStreamDataHandlingAdapter

        /// <summary>
        /// 将<see cref="TouchSocketConfig"/>中的配置，装载在<see cref="SingleStreamDataHandlingAdapter"/>上。
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="config"></param>
        public static void Config(this SingleStreamDataHandlingAdapter adapter, TouchSocketConfig config)
        {
            var option = config.GetValue(AdapterOptionProperty) ?? throw new ArgumentNullException(nameof(AdapterOptionProperty));

            if (option.MaxPackageSize.HasValue)
            {
                adapter.MaxPackageSize = option.MaxPackageSize.Value;
            }

            if (option.CacheTimeout.HasValue)
            {
                adapter.CacheTimeout = option.CacheTimeout.Value;
            }

            if (option.CacheTimeoutEnable.HasValue)
            {
                adapter.CacheTimeoutEnable = option.CacheTimeoutEnable.Value;
            }

            if (option.UpdateCacheTimeWhenRev.HasValue)
            {
                adapter.UpdateCacheTimeWhenRev = option.UpdateCacheTimeWhenRev.Value;
            }
        }

        /// <summary>
        /// 将<see cref="TouchSocketConfig"/>中的配置，装载在<see cref="SingleStreamDataHandlingAdapter"/>上。
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="config"></param>
        public static void Config(this DataHandlingAdapter adapter, TouchSocketConfig config)
        {
            var option = config.GetValue(AdapterOptionProperty) ?? throw new ArgumentNullException(nameof(AdapterOptionProperty));

            if (option.MaxPackageSize.HasValue)
            {
                adapter.MaxPackageSize = option.MaxPackageSize.Value;
            }
        }

        #endregion

        #region 适配器配置

        /// <summary>
        /// 设置适配器相关的配置
        /// </summary>
        public static readonly DependencyProperty<AdapterOption> AdapterOptionProperty = DependencyProperty<AdapterOption>.Register("AdapterOption", new AdapterOption());

        /// <summary>
        /// 设置适配器相关的配置
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetAdapterOption(this TouchSocketConfig config, AdapterOption value)
        {
            config.SetValue(AdapterOptionProperty, value);
            return config;
        }

        #endregion 适配器配置

        /// <summary>
        /// 将对象构建到字节数组
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <returns></returns>
        public static byte[] BuildAsBytes(this IRequestInfoBuilder requestInfo)
        {
            using (var byteBlock = new ByteBlock(requestInfo.MaxLength))
            {
                requestInfo.Build(byteBlock);
                return byteBlock.ToArray();
            }
        }
    }
}

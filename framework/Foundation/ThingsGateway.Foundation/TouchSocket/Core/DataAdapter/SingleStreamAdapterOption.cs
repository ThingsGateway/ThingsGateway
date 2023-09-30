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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 单线程流式适配器配置
    /// </summary>
    public class SingleStreamAdapterOption
    {
        /// <summary>
        /// 适配器数据包缓存启用。默认为缺省（null），如果有正常值会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        public bool? CacheTimeoutEnable { get; set; } = true;

        /// <summary>
        /// 适配器数据包缓存时长。默认为缺省（null）。当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.CacheTimeout"/>
        /// </summary>
        public int? CacheTimeout { get; set; }

        /// <summary>
        /// 适配器数据包最大值。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="DataHandlingAdapter.MaxPackageSize"/>
        /// </summary>
        public int? MaxPackageSize { get; set; }

        /// <summary>
        ///  适配器数据包缓存策略。默认缺省（null），当该值有效时会在设置适配器时，直接作用于<see cref="SingleStreamDataHandlingAdapter.UpdateCacheTimeWhenRev"/>
        /// </summary>
        public bool? UpdateCacheTimeWhenRev { get; set; }
    }
}

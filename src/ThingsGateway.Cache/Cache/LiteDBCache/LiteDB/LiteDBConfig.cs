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

namespace ThingsGateway.Cache
{
    /// <summary>
    /// liteDB配置
    /// </summary>
    public class LiteDBConfig
    {
        /// <summary>
        /// 最大文件大小MB
        /// </summary>
        public long MaxFileLength { get; set; } = 400;

        /// <summary>
        /// 每个Id文件夹内的最大文件数量
        /// </summary>
        public int MaxFileCount { get; set; } = 20;

        /// <summary>
        /// 磁盘阈值限制%，达到后将删除缓存文件夹中的前90%条文件
        /// </summary>
        public int MaxDriveUsage { get; set; } = 90;
    }
}
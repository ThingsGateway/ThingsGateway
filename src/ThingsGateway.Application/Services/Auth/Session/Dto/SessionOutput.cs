#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Application
{
    /// <summary>
    /// 会话输出
    /// </summary>
    public class SessionOutput : PrimaryKeyEntity
    {
        /// <summary>
        /// 账号
        ///</summary>
        [Description("账号")]
        public virtual string Account { get; set; }

        /// <summary>
        /// 姓名
        ///</summary>

        /// <summary>
        /// 最新登录ip
        ///</summary>
        [Description("最新登录ip")]
        public string LatestLoginIp { get; set; }

        /// <summary>
        /// 最新登录时间
        ///</summary>
        [Description("最新登录时间")]
        public DateTime? LatestLoginTime { get; set; }

        /// <summary>
        /// 在线状态
        /// </summary>
        [Description("在线状态")]
        public bool OnlineStatus { get; set; }

        /// <summary>
        /// 令牌数量
        /// </summary>
        [Description("令牌数量")]
        public int VerificatCount { get; set; }

        /// <summary>
        /// 令牌信息集合
        /// </summary>
        [Description("令牌列表")]
        public List<VerificatInfo> VerificatSignList { get; set; }
    }
}
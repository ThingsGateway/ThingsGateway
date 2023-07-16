#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Application
{
    /// <summary>
    /// 通知服务
    /// </summary>
    public interface INoticeService : ITransient
    {
        /// <summary>
        /// 通知用户下线
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="verificatInfos">verificat列表</param>
        /// <param name="message">消息内容</param>
        /// <returns></returns>
        Task LoginOut(string userId, List<VerificatInfo> verificatInfos, string message);
    }
}
namespace ThingsGateway.Application
{
    /// <summary>
    /// <inheritdoc cref="INoticeService"/>
    /// </summary>
    public class NoticeService : INoticeService
    {
        /// <inheritdoc/>
        public virtual async Task LoginOut(string userId, List<VerificatInfo> verificatInfos, string message)
        {
            //客户端ID列表
            var clientIds = new List<string>();
            //遍历token列表获取客户端ID列表
            verificatInfos.ForEach(it =>
            {
                clientIds.AddRange(it.ClientIds);
            });
            //获取signalr实例
            var signalr = App.GetService<IHubContext<TGHub, ITGHub>>();
            //发送其他客户端登录消息
            await signalr.Clients.Users(clientIds).LoginOut(message);
        }
    }
}
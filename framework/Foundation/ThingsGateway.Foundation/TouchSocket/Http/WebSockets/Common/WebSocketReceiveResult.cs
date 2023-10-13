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

namespace ThingsGateway.Foundation.Http.WebSockets
{
    /// <summary>
    /// WebSocketReceiveResult
    /// </summary>
    public struct WebSocketReceiveResult : IDisposable
    {
        private Action m_disAction;

        /// <summary>
        /// WebSocketReceiveResult
        /// </summary>
        /// <param name="disAction"></param>
        /// <param name="dataFrame"></param>
        public WebSocketReceiveResult(Action disAction, WSDataFrame dataFrame)
        {
            this.m_disAction = disAction;
            this.DataFrame = dataFrame;
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            m_disAction?.Invoke();
        }

        /// <summary>
        /// WebSocket数据帧
        /// </summary>
        public WSDataFrame DataFrame { get; private set; }

        /// <summary>
        /// 连接已关闭
        /// </summary>
        public bool IsClosed => this.DataFrame == null;
    }
}

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

using System.Diagnostics;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 插件事件类
    /// </summary>
    public class PluginEventArgs : TouchSocketEventArgs
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool m_end = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int m_index;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PluginModel m_pluginModel;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object m_sender;

        /// <summary>
        /// 由使用者自定义的状态对象。
        /// </summary>
        public object State { get; set; }

        /// <summary>
        /// 执行的插件数量。
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 调用下一个插件。
        /// </summary>
        /// <returns></returns>
        public Task InvokeNext()
        {
            if (this.m_end || this.Handled)
            {
                return EasyTask.CompletedTask;
            }

            if (this.m_pluginModel.Funcs.Count > this.m_index)
            {
                this.Count++;
                return this.m_pluginModel.Funcs[this.m_index++].Invoke(this.m_sender, this);
            }
            else
            {
                this.m_end = true;
                return EasyTask.CompletedTask;
            }
        }

        internal void LoadModel(PluginModel pluginModel, object sender)
        {
            this.m_sender = sender;
            this.m_pluginModel = pluginModel;
            this.m_end = false;
            this.m_index = 0;
        }
    }
}
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
    /// PluginBase
    /// </summary>
    public class PluginBase : DisposableObject, IPlugin
    {
        /// <inheritdoc/>
        [Obsolete("该属性已被弃用，插件顺序将直接由添加顺序决定。本设置将在正式版发布时直接删除", true)]
        public int Order { get; set; }

        /// <inheritdoc cref="IPlugin.Loaded(IPluginsManager)"/>
        protected virtual void Loaded(IPluginsManager pluginsManager)
        {
        }

        void IPlugin.Loaded(IPluginsManager pluginsManager)
        {
            this.Loaded(pluginsManager);
        }
    }
}
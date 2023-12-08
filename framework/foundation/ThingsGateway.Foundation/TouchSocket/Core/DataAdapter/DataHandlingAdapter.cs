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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 数据处理适配器
    /// </summary>
    public abstract class DataHandlingAdapter : DisposableObject
    {
        /// <summary>
        /// 是否允许发送<see cref="IRequestInfo"/>对象。
        /// </summary>
        public abstract bool CanSendRequestInfo { get; }

        /// <summary>
        /// 拼接发送
        /// </summary>
        public abstract bool CanSplicingSend { get; }

        /// <summary>
        /// 日志记录器。
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// 获取或设置适配器能接收的最大数据包长度。默认1024*1024 Byte。
        /// </summary>
        public int MaxPackageSize { get; set; } = 1024 * 1024 * 10;

        /// <summary>
        /// 适配器所有者
        /// </summary>
        public object Owner { get; private set; }

        /// <summary>
        /// 当适配器在被第一次加载时调用。
        /// </summary>
        /// <param name="owner"></param>
        /// <exception cref="Exception">此适配器已被其他终端使用，请重新创建对象。</exception>
        public virtual void OnLoaded(object owner)
        {
            if (this.Owner != null && this.Owner != owner)
            {
                throw new Exception("此适配器已被其他终端使用，请重新创建对象。");
            }
            this.Owner = owner;
        }

        /// <summary>
        /// 在解析时发生错误。
        /// </summary>
        /// <param name="error">错误异常</param>
        /// <param name="reset">是否调用<see cref="Reset"/></param>
        /// <param name="log">是否记录日志</param>
        protected virtual void OnError(string error, bool reset = true, bool log = true)
        {
            if (reset)
            {
                this.Reset();
            }
            if (log)
            {
                this.Logger?.Error(error);
            }
        }

        /// <summary>
        /// 重置解析器到初始状态，一般在<see cref="OnError(string, bool, bool)"/>被触发时，由返回值指示是否调用。
        /// </summary>
        protected abstract void Reset();
    }
}
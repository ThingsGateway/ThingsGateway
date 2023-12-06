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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 容器注册器
    /// </summary>
    public interface IRegistrator : IRegistered
    {

        /// <summary>
        /// 添加类型描述符。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descriptor"></param>
        void Register(DependencyDescriptor descriptor, string key);

        /// <summary>
        /// 添加类型描述符
        /// </summary>
        /// <param name="descriptor"></param>
        void Register(DependencyDescriptor descriptor);

        /// <summary>
        /// 移除注册信息。
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        void Unregister(DependencyDescriptor descriptor, string key);

        /// <summary>
        /// 移除注册信息
        /// </summary>
        /// <param name="descriptor"></param>
        void Unregister(DependencyDescriptor descriptor);

        /// <summary>
        /// 获取已注册的所有类型描述
        /// </summary>
        /// <returns></returns>
        IEnumerable<DependencyDescriptor> GetDescriptors();

        /// <summary>
        /// 构建一个服务获取器
        /// </summary>
        /// <returns></returns>
        IResolver BuildResolver();
    }
}
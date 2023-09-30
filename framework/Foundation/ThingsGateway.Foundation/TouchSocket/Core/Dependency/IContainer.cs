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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 注入容器接口
    /// </summary>
    public interface IContainer : IEnumerable<DependencyDescriptor>
    {
        /// <summary>
        /// 创建目标类型的对应实例。
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="ps"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object Resolve(Type fromType, object[] ps = null, string key = "");

        /// <summary>
        /// 判断某类型是否已经注册
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsRegistered(Type fromType, string key = "");

        /// <summary>
        /// 添加类型描述符。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descriptor"></param>
        void Register(DependencyDescriptor descriptor, string key = "");

        /// <summary>
        /// 移除注册信息。
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        void Unregister(DependencyDescriptor descriptor, string key = "");
    }
}
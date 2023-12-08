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

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// 密封的<see cref="DmtpActor"/>
    /// </summary>
    public sealed class SealedDmtpActor : DmtpActor
    {
        /// <summary>
        /// 创建一个Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute">是否允许路由</param>
        /// <param name="isReliable">是不是基于可靠协议运行的</param>
        public SealedDmtpActor(bool allowRoute, bool isReliable) : base(allowRoute, isReliable)
        {
        }

        /// <summary>
        /// 创建一个可靠协议的Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute"></param>
        public SealedDmtpActor(bool allowRoute) : this(allowRoute, true)
        {
        }
    }
}
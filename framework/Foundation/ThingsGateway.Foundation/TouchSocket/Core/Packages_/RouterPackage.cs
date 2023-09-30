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
    /// 具有目标id和源id的路由包
    /// </summary>
    public class RouterPackage : PackageBase
    {
        /// <summary>
        /// 标识是否路由
        /// </summary>
        public bool Route { get; set; }

        /// <summary>
        /// 源Id
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// 目标Id
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        /// 打包所有的路由包信息。顺序为：先调用<see cref="PackageRouter(in ByteBlock)"/>，然后<see cref="PackageBody(in ByteBlock)"/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public override sealed void Package(in ByteBlock byteBlock)
        {
            this.PackageRouter(byteBlock);
            this.PackageBody(byteBlock);
        }

        /// <summary>
        /// 打包数据体。一般不需要单独调用该方法。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public virtual void PackageBody(in ByteBlock byteBlock)
        {
        }

        /// <summary>
        /// 打包路由。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public virtual void PackageRouter(in ByteBlock byteBlock)
        {
            byteBlock.Write(this.Route);
            byteBlock.Write(this.SourceId);
            byteBlock.Write(this.TargetId);
        }

        /// <summary>
        /// 转换目标和源的id。
        /// </summary>
        public void SwitchId()
        {
            var value = this.SourceId;
            this.SourceId = this.TargetId;
            this.TargetId = value;
        }

        /// <inheritdoc/>
        public override sealed void Unpackage(in ByteBlock byteBlock)
        {
            this.UnpackageRouter(byteBlock);
            this.UnpackageBody(byteBlock);
        }

        /// <summary>
        /// 解包数据体。一般不需要单独调用该方法。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public virtual void UnpackageBody(in ByteBlock byteBlock)
        {
        }

        /// <summary>
        /// 只解包路由部分。一般不需要单独调用该方法。
        /// <para>重写的话，约定基类方法必须先执行</para>
        /// </summary>
        /// <param name="byteBlock"></param>
        public virtual void UnpackageRouter(in ByteBlock byteBlock)
        {
            this.Route = byteBlock.ReadBoolean();
            this.SourceId = byteBlock.ReadString();
            this.TargetId = byteBlock.ReadString();
        }
    }
}
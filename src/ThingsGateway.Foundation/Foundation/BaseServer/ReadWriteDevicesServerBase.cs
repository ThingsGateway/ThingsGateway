#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation
{
    /// <inheritdoc cref="ReadWriteDevicesBase"/>
    public abstract class ReadWriteDevicesServerBase : ReadWriteDevicesBase, IReadWriteDevice
    {
        /// <summary>
        /// 链接超时时间
        /// </summary>
        public ushort ConnectTimeOut { get; set; } = 3000;

        /// <summary>
        /// 启动服务
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// 停止服务
        /// </summary>
        public abstract void Stop();
        /// <summary>
        /// 设置适配器
        /// </summary>
        /// <param name="client"></param>
        public abstract void SetDataAdapter(SocketClient client);


    }
}
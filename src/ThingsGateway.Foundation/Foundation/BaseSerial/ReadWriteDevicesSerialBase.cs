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
    public abstract class ReadWriteDevicesSerialBase : ReadWriteDevicesBase, IReadWriteDevice
    {
        /// <summary>
        /// 连接操作
        /// </summary>
        public abstract void Open();
        /// <inheritdoc cref="Open"/>
        public abstract Task OpenAsync();
        /// <summary>
        /// 断开操作
        /// </summary>
        public abstract void Close();
        /// <summary>
        /// 设置适配器
        /// </summary>
        public abstract void SetDataAdapter();

    }
}
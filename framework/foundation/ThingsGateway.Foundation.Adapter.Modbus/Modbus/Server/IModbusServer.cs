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

using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Adapter.Modbus
{
    /// <inheritdoc/>
    internal interface IModbusServer : IReadWrite
    {
        /// <summary>
        /// 外部写入数据后，是否写入内存(下次读取实时更新，否则需要内部调用Write)
        /// </summary>
        public bool WriteMemory { get; set; }
        /// <summary>
        /// 读写锁
        /// </summary>
        public EasyLock EasyLock { get; }
        /// <summary>
        /// 多站点
        /// </summary>
        bool MulStation { get; set; }
        /// <summary>
        /// 默认站点
        /// </summary>
        byte Station { get; set; }
        /// <summary>
        /// 是否Rtu报文
        /// </summary>
        bool IsRtu { get; }

        /// <summary>
        /// 继电器
        /// </summary>
        internal ConcurrentDictionary<byte, ByteBlock> ModbusServer01ByteBlocks { get; set; }

        /// <summary>
        /// 开关输入
        /// </summary>
        internal ConcurrentDictionary<byte, ByteBlock> ModbusServer02ByteBlocks { get; set; }

        /// <summary>
        /// 输入寄存器
        /// </summary>
        internal ConcurrentDictionary<byte, ByteBlock> ModbusServer03ByteBlocks { get; set; }

        /// <summary>
        /// 保持寄存器
        /// </summary>
        internal ConcurrentDictionary<byte, ByteBlock> ModbusServer04ByteBlocks { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="mAddress"></param>
        public void Init(ModbusAddress mAddress);
        /// <summary>
        /// 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
        /// </summary>
        public Func<ModbusAddress, byte[], IThingsGatewayBitConverter, ISenderClient, Task<OperResult>> OnWriteData { get; set; }

    }
}
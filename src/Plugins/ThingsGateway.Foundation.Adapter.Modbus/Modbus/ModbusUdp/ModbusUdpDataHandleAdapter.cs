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

using ThingsGateway.Foundation.Extension;

namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusUdpDataHandleAdapter : ReadWriteDevicesUdpDataHandleAdapter<ModbusTcpMessage>
    {
        private readonly EasyIncrementCount easyIncrementCount = new EasyIncrementCount(ushort.MaxValue);

        public bool IsCheckMessageId
        {
            get
            {
                return Request?.IsCheckMessageId ?? false;
            }
            set
            {
                Request.IsCheckMessageId = value;
            }
        }

        public EasyIncrementCount MessageId => easyIncrementCount;
        public override byte[] PackCommand(byte[] command)
        {
            return ModbusHelper.AddModbusTcpHead(command, (ushort)easyIncrementCount.GetCurrentValue());
        }

        protected override ModbusTcpMessage GetInstance()
        {
            return new ModbusTcpMessage();
        }

        protected override OperResult<byte[]> UnpackResponse(
                          byte[] send,
          byte[] response)
        {
            return ModbusHelper.GetModbusData(send.RemoveBegin(6), response.RemoveBegin(6));
        }
    }
}

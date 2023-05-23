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

namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusRtuDataHandleAdapter : ReadWriteDevicesSerialDataHandleAdapter<ModbusRtuMessage>
    {
        public bool Crc16CheckEnable { get; set; } = true;

        public override byte[] PackCommand(byte[] command)
        {
            return ModbusHelper.AddCrc(command);
        }

        protected override ModbusRtuMessage GetInstance()
        {
            return new ModbusRtuMessage();
        }
        /// <summary>
        /// 解包获取实际数据包
        /// </summary>
        protected override FilterResult GetResponse(ByteBlock byteBlock, ModbusRtuMessage request, byte[] allBytes, byte[] bytes)
        {
            var unpackbytes = UnpackResponse(request.SendBytes, allBytes);
            request.Message = unpackbytes.Message;
            request.ResultCode = unpackbytes.ResultCode;
            if (unpackbytes.IsSuccess)
            {
                request.Content = unpackbytes.Content;
                request.ReceivedBytes = allBytes;
                return FilterResult.Success;
            }
            else
            {
                byteBlock.Pos = byteBlock.Len;
                request.ReceivedBytes = allBytes;
                request.Message = unpackbytes.Message;
                if (allBytes.Length <= 1)
                {
                    return FilterResult.Cache;
                }
                if (!(allBytes[1] <= 0x10))
                {
                    return FilterResult.Success;
                }
                else
                {
                    if ((allBytes.Length > allBytes[2] + 4))
                    {
                        return FilterResult.Success;
                    }
                    else
                    {
                        return FilterResult.Cache;
                    }
                }

            }
        }

        protected override OperResult<byte[]> UnpackResponse(byte[] send, byte[] response)
        {
            return ModbusHelper.GetModbusRtuData(send, response, Crc16CheckEnable);

        }

    }
}

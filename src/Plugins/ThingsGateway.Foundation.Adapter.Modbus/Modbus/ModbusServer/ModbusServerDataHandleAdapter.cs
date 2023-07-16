#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusServerDataHandleAdapter : ReadWriteDevicesTcpDataHandleAdapter<ModbusServerMessage>
    {

        public override byte[] PackCommand(byte[] command)
        {
            return command;
        }

        protected override ModbusServerMessage GetInstance()
        {
            return new ModbusServerMessage();
        }

        protected override OperResult<byte[]> UnpackResponse(
                          byte[] send,
          byte[] response)
        {
            return GetModbusData(response.RemoveBegin(6));
        }
        public ThingsGatewayBitConverter ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        protected override FilterResult GetResponse(ByteBlock byteBlock, ModbusServerMessage request, byte[] allBytes, byte[] bytes)
        {
            var unpackbytes = UnpackResponse(request.SendBytes, allBytes);
            request.Message = unpackbytes.Message;
            request.ResultCode = unpackbytes.ResultCode;
            if (unpackbytes.IsSuccess)
            {
                request.ReceivedBytes = allBytes;
                //解析01 03 00 00 00 0A
                var station = ThingsGatewayBitConverter.ToByte(bytes, 6);
                var function = ThingsGatewayBitConverter.ToByte(bytes, 7);
                int addressStart = ThingsGatewayBitConverter.ToInt16(bytes, 8);
                if (addressStart == -1)
                {
                    addressStart = 65535;
                }
                if (function > 4)
                {
                    if (function > 6)
                    {
                        request.CurModbusAddress = new ModbusAddress()
                        {
                            Station = station,
                            AddressStart = addressStart,
                            WriteFunction = function,
                            ReadFunction = function == 16 ? 3 : function == 15 ? 1 : 3,
                            Length = ThingsGatewayBitConverter.ToByte(bytes, 11),
                        };
                        request.Content = unpackbytes.Content.RemoveBegin(7);
                    }
                    else
                    {
                        request.CurModbusAddress = new ModbusAddress()
                        {
                            Station = station,
                            AddressStart = addressStart,
                            WriteFunction = function,
                            ReadFunction = function == 6 ? 3 : function == 5 ? 1 : 3,
                            Length = 1,
                        };
                        request.Content = unpackbytes.Content.RemoveBegin(4);
                    }
                }
                else
                {
                    request.CurModbusAddress = new ModbusAddress()
                    {
                        Station = station,
                        AddressStart = addressStart,
                        ReadFunction = function,
                        Length = ThingsGatewayBitConverter.ToByte(bytes, 11),
                    };
                }


                return FilterResult.Success;
            }
            else
            {
                byteBlock.Pos = byteBlock.Len;
                request.ReceivedBytes = allBytes;
                return FilterResult.Success;
            }
        }

        /// <summary>
        /// 获取modbus写入数据区内容
        /// </summary>
        /// <param name="send">发送数据</param>
        /// <param name="response">返回数据</param>
        /// <returns></returns>
        internal OperResult<byte[]> GetModbusData(byte[] response)
        {
            try
            {
                var func = ThingsGatewayBitConverter.ToByte(response, 1);
                if (func == 1 || func == 2 || func == 3 || func == 4 || func == 5 || func == 6)
                {
                    if (response.Length == 6)
                        return OperResult.CreateSuccessResult(response);
                }
                else if (func == 15 || func == 16)
                {
                    var length = ThingsGatewayBitConverter.ToByte(response, 6);
                    if (response.Length == 7 + length)
                    {
                        return OperResult.CreateSuccessResult(response);
                    }
                }

                return new OperResult<byte[]>(response) { Message = $"数据长度{response.Length}错误" };
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex.Message);
            }
        }






    }



}

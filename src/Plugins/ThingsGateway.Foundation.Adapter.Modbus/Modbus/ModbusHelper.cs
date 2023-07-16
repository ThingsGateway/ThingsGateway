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

using System.Text;

using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.Modbus
{
    internal class ModbusHelper
    {
        /// <summary>
        /// modbus地址格式说明
        /// </summary>
        /// <returns></returns>
        internal static string GetAddressDescription()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Modbus寄存器");
            stringBuilder.AppendLine("线圈寄存器使用从 00001 开始的地址编号。");
            stringBuilder.AppendLine("离散输入寄存器使用从 10001 开始的地址编号。");
            stringBuilder.AppendLine("输入寄存器使用从 30001 开始的地址编号。");
            stringBuilder.AppendLine("保持寄存器使用从 40001 开始的地址编号。");
            stringBuilder.AppendLine("举例：");
            stringBuilder.AppendLine("40001=>保持寄存器第一个寄存器");
            stringBuilder.AppendLine("额外格式：");
            stringBuilder.AppendLine("设备站号 ，比如40001;s=2; ，代表设备地址为2的保持寄存器第一个寄存器");
            stringBuilder.AppendLine("写入功能码 ，比如40001;w=16; ，代表保持寄存器第一个寄存器，写入值时采用0x10功能码，而不是默认的0x06功能码");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 添加Crc16
        /// </summary>
        internal static byte[] AddCrc(byte[] command)
        {
            return EasyCRC16.CRC16(command);
        }

        /// <summary>
        /// 添加ModbusTcp报文头
        /// </summary>
        internal static byte[] AddModbusTcpHead(byte[] modbus, ushort id)
        {
            byte[] tcp = new byte[modbus.Length + 6];
            tcp[0] = BitConverter.GetBytes(id)[1];
            tcp[1] = BitConverter.GetBytes(id)[0];
            tcp[4] = BitConverter.GetBytes(modbus.Length)[1];
            tcp[5] = BitConverter.GetBytes(modbus.Length)[0];
            modbus.CopyTo(tcp, 6);
            return tcp;
        }

        /// <summary>
        /// 通过错误码来获取到对应的文本消息
        /// </summary>
        internal static string GetDescriptionByErrorCode(byte code)
        {
            switch (code)
            {
                case 1:
                    return "不支持的功能码";
                case 2:
                    return "读取寄存器越界";
                case 3:
                    return "读取长度超限";
                case 4:
                    return "读写异常";
                default:
                    return "未知错误";
            }
        }

        /// <summary>
        /// 获取modbus数据区内容，返回数据需去除Crc和报文头，例如：01 03 02 00 01，发送数据需报文头
        /// </summary>
        /// <param name="send">发送数据</param>
        /// <param name="response">返回数据</param>
        /// <returns></returns>
        internal static OperResult<byte[]> GetModbusData(byte[] send, byte[] response)
        {
            try
            {

                if (response[1] >= 0x80)//错误码
                    return new OperResult<byte[]>(GetDescriptionByErrorCode(response[2]));
                if (send.Length == 0)
                {
                    var result = OperResult.CreateSuccessResult(GenericHelpers.ArrayRemoveBegin(response, 3));
                    result.Message = "接收数据正确，但主机并没有主动请求数据";
                    result.ResultCode = ResultCode.Canceled;
                    return result;
                }
                if (send[0] != response[0])
                    return new OperResult<byte[]>(string.Format("站号不一致", send[0], response[0]));
                if (send[1] != response[1])
                    return new OperResult<byte[]>(response) { Message = "功能码不一致" };
                if (response.Length > 3)
                    return OperResult.CreateSuccessResult(GenericHelpers.ArrayRemoveBegin(response, 3));
                else
                    return new OperResult<byte[]>(response) { Message = "数据长度为0" };
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex.Message);
            }
        }
        /// <summary>
        /// 去除Crc，返回modbus数据区
        /// </summary>
        /// <param name="send"></param>
        /// <param name="response"></param>
        /// <param name="crcCheck"></param>
        /// <returns></returns>
        internal static OperResult<byte[]> GetModbusRtuData(byte[] send, byte[] response, bool crcCheck = true)
        {
            if (response.Length < 5)
                return new OperResult<byte[]>("数据长度不足" + response.ToHexString());
            if (crcCheck && !EasyCRC16.CheckCRC16(response))
                return new OperResult<byte[]>("Crc校验失败" + DataHelper.ByteToHexString(response, ' '));
            return GetModbusData(send, response.RemoveLast(2));
        }
        /// <summary>
        /// 获取读取报文
        /// </summary>
        internal static OperResult<byte[]> GetReadModbusCommand(string address, int length, byte station)
        {
            try
            {
                ModbusAddress mAddress = new ModbusAddress(address, station);
                return GetReadModbusCommand(mAddress, length);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex.Message);
            }
        }
        /// <summary>
        /// 获取写入布尔量报文，根据地址识别功能码
        /// </summary>
        internal static OperResult<byte[]> GetWriteBoolModbusCommand(string address, bool[] values, byte station)
        {
            try
            {
                ModbusAddress mAddress = new ModbusAddress(address, station);
                //功能码或实际长度
                if (values?.Length > 1 || mAddress.WriteFunction == 15)
                    return GetWriteBoolModbusCommand(mAddress, values, values.Length);
                else
                    return GetWriteBoolModbusCommand(address, values[0], station);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex.Message);
            }
        }

        /// <summary>
        /// 获取写入字报文，根据地址识别功能码
        /// </summary>
        internal static OperResult<byte[]> GetWriteModbusCommand(string address, byte[] value, byte station)
        {
            try
            {
                ModbusAddress mAddress = new ModbusAddress(address, station);
                //功能码或实际长度
                if (value?.Length > 2 || mAddress.WriteFunction == 16)
                    return GetWriteModbusCommand(mAddress, value);
                else
                    return GetWriteOneModbusCommand(mAddress, value);

            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex.Message);
            }
        }
        /// <summary>
        /// 获取读取报文
        /// </summary>
        private static OperResult<byte[]> GetReadModbusCommand(ModbusAddress mAddress, int length)
        {
            byte[] array = new byte[6]
            {
            (byte) mAddress.Station,
            (byte) mAddress.ReadFunction,
            BitConverter.GetBytes(mAddress.AddressStart)[1],
            BitConverter.GetBytes(mAddress.AddressStart)[0],
            BitConverter.GetBytes(length)[1],
            BitConverter.GetBytes(length)[0]
            };

            return OperResult.CreateSuccessResult(array);
        }

        /// <summary>
        /// 获取05写入布尔量报文
        /// </summary>
        private static OperResult<byte[]> GetWriteBoolModbusCommand(string address, bool value, byte station)
        {
            try
            {
                if (address.IndexOf('.') <= 0)
                {
                    ModbusAddress mAddress = new ModbusAddress(address, station);
                    return GetWriteBoolModbusCommand(mAddress, value);
                }
                return new("不支持写入字寄存器的某一位");
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex.Message);
            }
        }

        /// <summary>
        /// 获取05写入布尔量报文
        /// </summary>
        private static OperResult<byte[]> GetWriteBoolModbusCommand(ModbusAddress mAddress, bool value)
        {
            byte[] array = new byte[6]
            {
        (byte) mAddress.Station,
        (byte)5,
        BitConverter.GetBytes(mAddress.AddressStart)[1],
        BitConverter.GetBytes(mAddress.AddressStart)[0],
         0,
         0
            };
            if (value)
            {
                array[4] = 0xFF;
                array[5] = 0;
            }
            else
            {
                array[4] = 0;
                array[5] = 0;
            }
            return OperResult.CreateSuccessResult(array);
        }

        /// <summary>
        /// 获取15写入布尔量报文
        /// </summary>
        private static OperResult<byte[]> GetWriteBoolModbusCommand(ModbusAddress mAddress, bool[] values, int length)
        {
            try
            {
                byte[] numArray1 = values.BoolArrayToByte();
                byte[] numArray2 = new byte[7 + numArray1.Length];
                numArray2[0] = (byte)mAddress.Station;
                numArray2[1] = (byte)15;
                numArray2[2] = BitConverter.GetBytes(mAddress.AddressStart)[1];
                numArray2[3] = BitConverter.GetBytes(mAddress.AddressStart)[0];
                numArray2[4] = (byte)(length / 256);
                numArray2[5] = (byte)(length % 256);
                numArray2[6] = (byte)numArray1.Length;
                numArray1.CopyTo(numArray2, 7);
                return OperResult.CreateSuccessResult(numArray2);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex.Message);
            }
        }

        /// <summary>
        /// 获取16写入字报文
        /// </summary>
        private static OperResult<byte[]> GetWriteModbusCommand(ModbusAddress mAddress, byte[] values)
        {
            byte[] numArray = new byte[7 + values.Length];
            numArray[0] = (byte)mAddress.Station;
            numArray[1] = (byte)16;
            numArray[2] = BitConverter.GetBytes(mAddress.AddressStart)[1];
            numArray[3] = BitConverter.GetBytes(mAddress.AddressStart)[0];
            numArray[4] = (byte)(values.Length / 2 / 256);
            numArray[5] = (byte)(values.Length / 2 % 256);
            numArray[6] = (byte)values.Length;
            values.CopyTo(numArray, 7);
            return OperResult.CreateSuccessResult(numArray);

        }

        /// <summary>
        /// 获取6写入字报文
        /// </summary>
        private static OperResult<byte[]> GetWriteOneModbusCommand(ModbusAddress mAddress, byte[] values)
        {
            byte[] numArray = new byte[4 + values.Length];
            numArray[0] = (byte)mAddress.Station;
            numArray[1] = (byte)6;
            numArray[2] = BitConverter.GetBytes(mAddress.AddressStart)[1];
            numArray[3] = BitConverter.GetBytes(mAddress.AddressStart)[0];
            values.CopyTo(numArray, 4);
            return OperResult.CreateSuccessResult(numArray);
        }

    }
}

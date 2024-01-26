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

using Newtonsoft.Json.Linq;

using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;

namespace ThingsGateway.Foundation
{
    internal class ModbusMatserTest
    {
        public async Task Test()
        {
            var clientConfig = new TouchSocketConfig();
            //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取
            var clientChannel = clientConfig.GetTcpClientWithIPHost("tcp://127.0.0.1:502");

            //创建modbus客户端，传入通道
            using ModbusMaster modbusMaster = new(clientChannel)
            {
                //modbus协议格式
                //ModbusType = Modbus.ModbusTypeEnum.ModbusRtu,
                ModbusType = Modbus.ModbusTypeEnum.ModbusTcp,
            };

            //测试5千次
            for (int i = 0; i < 5000; i++)
            {
                //读写对应数据类型
                var result = await modbusMaster.ReadInt32Async("40001", 1);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result);
                }
            }

            var wResult = await modbusMaster.WriteAsync("40001", 1);

            //动态类型读写
            var objResult = await modbusMaster.ReadAsync("40001", 1, DataTypeEnum.Int32);
            var objWResult = await modbusMaster.WriteAsync("40001", JToken.FromObject(1), DataTypeEnum.Int32);

            //地址说明
            //单独设置解析顺序
            var objABCDResult = await modbusMaster.ReadAsync("40001;dataformat=badc", 1, DataTypeEnum.Int32);
            //单独设置站号
            var objSResult = await modbusMaster.ReadAsync("40001;dataformat=badc;s=2", 1, DataTypeEnum.Int32);
            //单独设置写入功能码
            var objFWResult = await modbusMaster.ReadAsync("40001;s=2;w=16", 1, DataTypeEnum.Int16);
        }
    }
}
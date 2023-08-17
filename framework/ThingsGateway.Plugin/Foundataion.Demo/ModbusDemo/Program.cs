using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension.Byte;
using ThingsGateway.Foundation.Serial;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ModbusDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await ModbusClientAsync();

            await ModbusServerAsync();

            Console.ReadLine();
        }

        private static async Task ModbusServerAsync()
        {
            //同样建立链路与协议对象
            var config = new TouchSocketConfig().SetListenIPHosts(new IPHost[] { "127.0.0.1:502" });
            var service = new TcpService();
            service.Setup(config);
            //载入配置
            ModbusServer _plc = new(service)
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                Station = 1,
                MulStation = true
            };

            //客户端写入数据的事件
            _plc.WriteData += ModbusServerWriteData;
            await _plc.ConnectAsync(CancellationToken.None);
            //把需要的数据推到缓存区
            await _plc.WriteAsync("400001", (Int16)123);

        }

        private static OperResult ModbusServerWriteData(ModbusAddress address, byte[] bytes, IThingsGatewayBitConverter converter, SocketClient client)
        {
            //接收到写入的地址/字节数组/解析类
            //比如规定400001是Int16类型
            var data = converter.GetDynamicDataFormBytes(address.ToString(), typeof(Int16), bytes);
            //处理数据走向
            //如果处理成功，返回
            return OperResult.CreateSuccessResult();
            //否则返回new OperResult();
        }

        private static async Task ModbusClientAsync()
        {
            //链路基础配置项
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:502"))//TCP/UDP链路才需要
            .SetSerialProperty(new SerialProperty() //串口链路才需要
            {
                PortName = "COM1"
            }).SetBufferLength(1024);

            var tcpClient1 = new TcpClientEx();//链路对象
            var tcpClient2 = new TcpClientEx();//链路对象
            var udpSession1 = new UdpSession();//链路对象
            var udpSession2 = new UdpSession();//链路对象
            var serialClient = new SerialClient();//链路对象
            tcpClient1.Setup(config);
            tcpClient2.Setup(config);
            udpSession1.Setup(config);
            udpSession2.Setup(config);
            serialClient.Setup(config);

            //创建协议对象,构造函数需要传入对应链路对象

            ModbusTcp modbusTcp = new(tcpClient1)
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                FrameTime = 0,
                CacheTimeout = 1000,
                ConnectTimeOut = 3000,
                Station = 1,
                TimeOut = 3000,
                IsCheckMessageId = true
            };
            ModbusRtuOverTcp modbusRtuOvrTcp = new(tcpClient2)//传入链路
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                FrameTime = 0,
                CacheTimeout = 1000,
                ConnectTimeOut = 3000,
                Station = 1,
                TimeOut = 3000,
                Crc16CheckEnable = true
            };
            ModbusUdp modbusUdp = new(udpSession2)//传入链路
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                FrameTime = 0,
                ConnectTimeOut = 3000,
                Station = 1,
                TimeOut = 3000,
                IsCheckMessageId = true
            };
            ModbusRtu modbusRtu = new(serialClient)//传入链路
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                FrameTime = 0,
                CacheTimeout = 1000,
                ConnectTimeOut = 3000,
                Station = 1,
                TimeOut = 3000,
                Crc16CheckEnable = true
            };
            ModbusRtuOverUdp modbusRtuOvrUdp = new(udpSession1)//传入链路
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                FrameTime = 0,
                ConnectTimeOut = 3000,
                Station = 1,
                TimeOut = 3000,
                Crc16CheckEnable = true
            };

            #region 读写测试
            //测试读取写入
            Console.WriteLine("modbusTcp：" + modbusTcp.TcpClientEx.RemoteIPHost);
            await TestAsync(modbusTcp);
            Console.WriteLine("modbusRtu：" + modbusRtu.SerialClient.SerialProperty.ToJson());
            await TestAsync(modbusRtu);
            Console.WriteLine("modbusRtuOvrTcp：" + modbusRtuOvrTcp.TcpClientEx.RemoteIPHost);
            await TestAsync(modbusRtuOvrTcp);
            Console.WriteLine("modbusRtuOvrUdp：" + modbusRtuOvrUdp.UdpSession.RemoteIPHost);
            await TestAsync(modbusRtuOvrUdp);
            Console.WriteLine("modbusUdp：" + modbusUdp.UdpSession.RemoteIPHost);
            await TestAsync(modbusUdp);

            #endregion

            #region 共用链路
            //有些场景可能需要在同一个连接内同时支持多个协议，这时传入同一个链路对象
            //注意在更换到其他协议进行读写时，需要手动改一下解析协议
            //这里传入同一个tcpClient1
            modbusTcp = new(tcpClient1)
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                FrameTime = 0,
                CacheTimeout = 1000,
                ConnectTimeOut = 3000,
                Station = 1,
                TimeOut = 3000,
                IsCheckMessageId = true
            };
            modbusRtuOvrTcp = new(tcpClient1)//传入链路
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                FrameTime = 0,
                CacheTimeout = 1000,
                ConnectTimeOut = 3000,
                Station = 1,
                TimeOut = 3000,
                Crc16CheckEnable = true
            };
            await tcpClient1.ConnectAsync();
            modbusTcp.SetDataAdapter();//连接成后设置协议适配器
            await TestAsync(modbusTcp);
            modbusRtuOvrTcp.SetDataAdapter();//连接成后设置协议适配器
            await TestAsync(modbusRtuOvrTcp);
            #endregion
        }

        private static async Task TestAsync(IReadWriteDevice plc)
        {
            //下面的方法对应整个Modbus协议对象都是通用的，包括Rtu/Tcp
            //注意下面的方法都带有CancellationToken传播，一般是在最后一个参数，默认为None


            //自由读取20个寄存器
            var bytesResult = await plc.ReadAsync("400001", 20);
            //返回带有是否成功等参数，实际数据在data.Content中
            Console.WriteLine("bytesResult-400001：" + (bytesResult.IsSuccess ? bytesResult.Content.ToHexString() : bytesResult.Message));

            //如果确认了数据类型，如读取400001地址的int16类型，读取其他类型也一样
            var int16Result = await plc.ReadAsync<Int16>("400001");
            Console.WriteLine("int16Result-400001：" +
                (int16Result.IsSuccess ? int16Result.Content : int16Result.Message));

            //如果需要读取4个字节以上的数据，并且需要自定义解析顺序，而不是ModbusTcp中的默认解析顺序
            var int32Result = await plc.ReadAsync<Int32>("400001;DATA=CDAB");
            Console.WriteLine("int32Result-400001;DATA=CDBA：" +
                (int32Result.IsSuccess ? int32Result.Content : int32Result.Message));
            var int32Result2 = await plc.ReadAsync<Int32>("400001;DATA=ABCD");
            Console.WriteLine("int32Result-400001;DATA=ABCD：" +
                (int32Result2.IsSuccess ? int32Result2.Content : int32Result2.Message));


            //如果需要读取字符串，在Modbus协议中需要固定字符串长度,注意这里的字符串长度指的是对应长度
            var stringResult = await plc.ReadAsync<String>("400001;LEN=10;TEXT=UTF8");
            Console.WriteLine("stringResult-400001;LEN=8;TEXT=UTF8：" +
                (stringResult.IsSuccess ? stringResult.Content : stringResult.Message));

            //写入Int16数据，第二个参数为对应值
            var int16WResult = await plc.WriteAsync("400001", (Int16)123);
            Console.WriteLine("int16WResult-400001：" +
                (int16WResult.Message));

            //写入Int32数据，第二个参数为对应值，同样可以定义字节顺序
            var int32WResult = await plc.WriteAsync("400001;DATA=CDAB", (Int32)123);
            Console.WriteLine("int32WResult-400001;DATA=CDAB：" +
                (int32WResult.Message));
        }
    }
}
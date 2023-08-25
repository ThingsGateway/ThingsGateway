using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.DLT645;
using ThingsGateway.Foundation.Extension.Byte;
using ThingsGateway.Foundation.Serial;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace DLT645Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            await DLT645_2007ClientAsync();

            Console.ReadLine();
        }


        private static async Task DLT645_2007ClientAsync()
        {
            //链路基础配置项
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:502"))//TCP/UDP链路才需要
            .SetSerialProperty(new SerialProperty() //串口链路才需要
            {
                PortName = "COM1"
            }).SetBufferLength(1024);

            var serialClient = new SerialClient();//链路对象
            serialClient.Setup(config);

            //创建协议对象,构造函数需要传入对应链路对象

            DLT645_2007 dlt6452007 = new(serialClient)//传入链路
            {
                //协议配置
                DataFormat = DataFormat.ABCD,
                FrameTime = 0,
                CacheTimeout = 1000,
                ConnectTimeOut = 3000,
                TimeOut = 3000,
                EnableFEHead = true,
            };

            #region 读写测试
            //测试读取写入
            Console.WriteLine("dlt6452007：" + dlt6452007.SerialClient.SerialProperty.ToJson());
            await TestAsync(dlt6452007);
            #endregion

        }

        private static async Task TestAsync(DLT645_2007 plc)
        {
            //下面的方法对应PLC读写协议对象都是通用的
            //注意下面的方法都带有CancellationToken传播，一般是在最后一个参数，默认为None


            //var bytes02010100Result = await plc.ReadAsync("03010100", 20);
            ////返回带有是否成功等参数，实际数据在data.Content中
            //Console.WriteLine("bytes02010100Result：" + (bytes02010100Result.IsSuccess ? bytes02010100Result.Content.ToHexString() : bytes02010100Result.Message));
            //Console.WriteLine("bytes02010100Result：" + (bytes02010100Result.IsSuccess ? plc.ThingsGatewayBitConverter.ToDouble(bytes02010100Result.Content, 0) : bytes02010100Result.Message));

            //var bytes02020100Result = await plc.ReadAsync("02020100", 20);
            ////返回带有是否成功等参数，实际数据在data.Content中
            //Console.WriteLine("bytes02020100Result：" + (bytes02020100Result.IsSuccess ? bytes02020100Result.Content.ToHexString() : bytes02020100Result.Message));
            //Console.WriteLine("bytes02020100Result：" + (bytes02020100Result.IsSuccess ? plc.ThingsGatewayBitConverter.ToDouble(bytes02020100Result.Content, 0) : bytes02020100Result.Message));

            //var bytes02020100Result = await plc.ReadAsync("04000103", 1);
            ////返回带有是否成功等参数，实际数据在data.Content中
            //Console.WriteLine("bytes02020100Result：" + (bytes02020100Result.IsSuccess ? bytes02020100Result.Content.ToHexString() : bytes02020100Result.Message));
            //Console.WriteLine("bytes02020100Result：" + (bytes02020100Result.IsSuccess ? plc.ThingsGatewayBitConverter.ToString(bytes02020100Result.Content) : bytes02020100Result.Message));


            var test1 = await plc.ReadDeviceStationAsync();
            Console.WriteLine("bytes02020100Result：" + (test1.IsSuccess ? test1.Content : test1.Message));

            var test2 = await plc.WriteDeviceStationAsync("311111111114");
            Console.WriteLine("bytes02020100Result：" + (test2.IsSuccess ? test2.Message : test2.Message));
            plc.BroadcastTime(DateTime.Now);
            plc.Station = "311111111114";

            var test3 = await plc.WriteBaudRateAsync(19200);
            Console.WriteLine("bytes02020100Result：" + (test3.IsSuccess ? test3.Message : test3.Message));
            var test4 = await plc.FreezeAsync(DateTime.Now);
            Console.WriteLine("bytes02020100Result：" + (test4.IsSuccess ? test4.Message : test4.Message));

            var test5 = await plc.WritePasswordAsync(1, "66666666", "11111111");
            Console.WriteLine("bytes02020100Result：" + (test5.IsSuccess ? test5.Message : test5.Message));
            plc.Password = "11111111";



            var bytes02020100Result1 = await plc.ReadAsync("04000403", 1);
            //返回带有是否成功等参数，实际数据在data.Content中
            Console.WriteLine("bytes02020100Result：" + (bytes02020100Result1.IsSuccess ? bytes02020100Result1.Content.ToHexString() : bytes02020100Result1.Message));
            Console.WriteLine("bytes02020100Result：" + (bytes02020100Result1.IsSuccess ? plc.ThingsGatewayBitConverter.ToString(bytes02020100Result1.Content) : bytes02020100Result1.Message));

            var test = await plc.WriteAsync("04000403", "33");
            //返回带有是否成功等参数，实际数据在data.Content中
            Console.WriteLine("test：" + test.Message);
            var bytes02020100Result = await plc.ReadAsync("04000403", 1);
            //返回带有是否成功等参数，实际数据在data.Content中
            Console.WriteLine("bytes02020100Result：" + (bytes02020100Result.IsSuccess ? bytes02020100Result.Content.ToHexString() : bytes02020100Result.Message));
            Console.WriteLine("bytes02020100Result：" + (bytes02020100Result.IsSuccess ? plc.ThingsGatewayBitConverter.ToString(bytes02020100Result.Content) : bytes02020100Result.Message));

        }
    }
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.SiemensS7;

using TouchSocket.Core;

namespace ThingsGateway.Foundation
{
    internal class S7MatserTest
    {
        private static void Test(S7Variable s7Variable, ushort value)
        {
            s7Variable.WriteData1(value, default);
            s7Variable.WriteData2(value, default);

            //执行连读
            s7Variable.MulRead();
            Console.WriteLine(s7Variable.ToJsonString());
            //源生成WriteData1与WriteData2方法(Write{属性名称})
            var data1 = s7Variable.WriteData1(value + 10, default);
            var data2 = s7Variable.WriteData2(value + 10, default);
            //执行连读
            s7Variable.MulRead();
            Console.WriteLine(s7Variable.ToJsonString());
        }

        private static SiemensS7Master GetMaster()
        {
            var clientConfig = new TouchSocketConfig();
            clientConfig.ConfigureContainer(a => a.AddConsoleLogger());
            //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取
            var clientChannel = clientConfig.GetTcpClientWithIPHost("tcp://127.0.0.1:102");
            //clientChannel.Logger.LogLevel = LogLevel.Trace;
            SiemensS7Master siemensS7Master = new(clientChannel)
            {
                SiemensType = SiemensTypeEnum.S1500,
            };
            return siemensS7Master;
        }
        public static void Test()
        {
            using SiemensS7Master siemensS7Master = GetMaster();
            //构造实体类对象，传入协议对象与连读打包的最大数量
            S7Variable s7Variable = new(siemensS7Master, 100);

            Test(s7Variable, 10);
            Console.ReadLine();

            static void Test(S7Variable s7Variable, ushort value)
            {
                s7Variable.WriteData1(value, default);
                s7Variable.WriteData2(value, default);

                //执行连读
                s7Variable.MulRead();
                Console.WriteLine(s7Variable.ToJsonString());
                //源生成WriteData1与WriteData2方法(Write{属性名称})
                var data1 = s7Variable.WriteData1(value + 10, default);
                var data2 = s7Variable.WriteData2(value + 10, default);
                //执行连读
                s7Variable.MulRead();
                Console.WriteLine(s7Variable.ToJsonString());
            }
        }

    }

    [GeneratorVariable]
    public partial class S7Variable : VariableObject
    {
        [VariableRuntime(RegisterAddress = "M100")]
        public ushort Data1 { get; set; }

        [VariableRuntime(RegisterAddress = "M200")]
        public ushort Data2 { get; set; }

        public S7Variable(IProtocol protocol, int maxPack) : base(protocol, maxPack)
        {
        }
    }
}
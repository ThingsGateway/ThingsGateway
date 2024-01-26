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

using ThingsGateway.Foundation.Dlt645;

using TouchSocket.Core;

namespace ThingsGateway.Foundation
{
    internal class Dlt645MasterTest
    {
        /// <summary>
        /// 新建链路
        /// </summary>
        /// <returns></returns>
        public IChannel GetChannel()
        {
            TouchSocketConfig touchSocketConfig = new TouchSocketConfig();
            return touchSocketConfig.GetSerialPortWithOption(new("COM1")); //直接获取串口对象
            //return touchSocketConfig.GetChannel(ChannelTypeEnum.SerialPortClient, null, null, new("COM1"));//通过链路枚举获取对象
        }

        /// <summary>
        /// 新建协议对象
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public IProtocol GetProtocol(IChannel channel)
        {
            var client = new Dlt645_2007Master(channel);
            client.Station = "311111111114";//表号
            return client;
        }
    }
}
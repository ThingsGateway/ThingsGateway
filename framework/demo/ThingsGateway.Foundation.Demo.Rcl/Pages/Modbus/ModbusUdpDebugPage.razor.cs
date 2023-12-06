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

namespace ThingsGateway.Foundation.Demo;


/// <inheritdoc/>
public partial class ModbusUdpDebugPage
{
    /// <summary>
    /// SerialPortClientPage
    /// </summary>
    private UdpSessionPage _udpSessionPage;
    private DriverDebugUIPage _driverDebugUIPage;

    private ThingsGateway.Foundation.Adapter.Modbus.ModbusUdp _plc;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _sections.Add((
                """
                private static async Task ModbusClientAsync()
                {
                     //链路基础配置项
                    var config = new TouchSocketConfig();
                    config
                    .SetRemoteIPHost(new IPHost("127.0.0.1:502"))//TCP/UDP链路才需要
                
                var udpSession1 = new UdpSession();//链路对象
                udpSession1.Setup(config);
                
                    //创建协议对象,构造函数需要传入对应链路对象
                    ModbusUdp plc = new(udpSession1)
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
                        var bytesResult = await plc.ReadAsync("400001", 20);
                        var int32sResult = await plc.ReadInt32Async("400001", 20);
                
                    #endregion

                }
                
                """, "csharp"));

            if (_udpSessionPage != null)
                _udpSessionPage.LogAction = _driverDebugUIPage.LogOut;
            _plc = new ThingsGateway.Foundation.Adapter.Modbus.ModbusUdp(_udpSessionPage.GetUdpSession());
            _driverDebugUIPage.Plc = _plc;
            //载入配置
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }

}
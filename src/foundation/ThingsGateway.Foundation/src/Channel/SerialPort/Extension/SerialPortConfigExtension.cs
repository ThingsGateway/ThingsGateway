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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// SerialPortConfigExtension
    /// </summary>
    public static class SerialPortConfigExtension
    {
        /// <summary>
        /// 设置串口适配器
        /// </summary>
        public static readonly DependencyProperty<Func<SingleStreamDataHandlingAdapter>> SerialDataHandlingAdapterProperty =
            DependencyProperty<Func<SingleStreamDataHandlingAdapter>>.Register("SerialDataHandlingAdapter", () => new NormalDataHandlingAdapter());

        /// <summary>
        /// 串口属性。
        /// </summary>
        public static readonly DependencyProperty<SerialPortOption> SerialPortOptionProperty =
            DependencyProperty<SerialPortOption>.Register("SerialPortOption", new SerialPortOption(string.Empty));

        /// <summary>
        /// 设置(串口系)数据处理适配器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerialDataHandlingAdapter(this TouchSocketConfig config, Func<SingleStreamDataHandlingAdapter> value)
        {
            config.SetValue(SerialDataHandlingAdapterProperty, value);
            return config;
        }

        /// <summary>
        /// 设置串口属性。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerialPortOption(this TouchSocketConfig config, SerialPortOption value)
        {
            config.SetValue(SerialPortOptionProperty, value);
            return config;
        }

        /// <summary>
        /// 获取一个新的串口通道。传入串口配置信息
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TgSerialPortClient GetSerialPortWithOption(this TouchSocketConfig config, SerialPortOption value)
        {
            if (value == null) throw new ArgumentNullException(nameof(SerialPortOption));
            config.SetValue(SerialPortOptionProperty, value);

            //载入配置
            TgSerialPortClient tgSerialPortClient = new TgSerialPortClient();
            tgSerialPortClient.Setup(config);

            return tgSerialPortClient;
        }
    }
}
namespace ThingsGateway.Foundation.Serial
{
    public static class SerialConfigExtension
    {
        public static readonly DependencyProperty<SerialProperty> SerialProperty =
            DependencyProperty<SerialProperty>.Register("SerialProperty", typeof(SerialConfigExtension), null);

        /// <summary>
        /// 数据处理适配器，默认为获取<see cref="NormalSerialDataHandlingAdapter"/>
        /// 所需类型<see cref="Func{TResult}"/>
        /// </summary>
        public static readonly DependencyProperty<Func<SerialDataHandlingAdapter>> DataHandlingAdapterProperty = DependencyProperty<Func<SerialDataHandlingAdapter>>.Register("SerialDataHandlingAdapter", typeof(SerialConfigExtension), () => { return new NormalSerialDataHandlingAdapter(); });

        /// <summary>
        /// 设置(Serial)数据处理适配器。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetDataHandlingAdapter(this TouchSocketConfig config, Func<SerialDataHandlingAdapter> value)
        {
            config.SetValue(DataHandlingAdapterProperty, value);
            return config;
        }




        /// <summary>
        /// 设置串口
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TouchSocketConfig SetSerialProperty(this TouchSocketConfig config, SerialProperty value)
        {
            config.SetValue(SerialProperty, value);
            return config;
        }


    }
}
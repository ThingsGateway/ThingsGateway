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

namespace ThingsGateway.Foundation.Serial;

/// <summary>
/// 串口附加属性
/// </summary>
public static class SerialConfigExtension
{
    /// <summary>
    /// 串口属性
    /// </summary>
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
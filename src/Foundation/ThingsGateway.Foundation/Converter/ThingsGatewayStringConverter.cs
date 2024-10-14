//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// String类型数据转换器
/// </summary>
public class ThingsGatewayStringConverter : StringSerializerConverter
{
    /// <summary>
    /// 默认实例
    /// </summary>
    public static ThingsGatewayStringConverter Default = new ThingsGatewayStringConverter();

    /// <summary>
    /// 构造函数
    /// </summary>
    public ThingsGatewayStringConverter(params ISerializerFormatter<string, object>[] converters) : base(converters)
    {
        Add(new StringToClassConverter<object>());
        Add(new JsonStringToClassSerializerFormatter<object>());
    }
}

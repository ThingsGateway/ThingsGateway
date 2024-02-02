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

using Newtonsoft.Json;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// String类型数据转换器
    /// </summary>
    public class ThingsGatewayStringConverter : StringConverter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ThingsGatewayStringConverter()
        {
            this.Add(new StringToClassConverter());
            this.Add(new StringToPrimitiveConverter());
            this.Add(new JsonStringToClassConverter());
        }
    }

    /// <summary>
    /// String值转换为基础类型。
    /// </summary>
    public class StringToClassConverter : IConverter<string>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; } = -100;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool TryConvertFrom(string source, Type targetType, out object target)
        {
            return targetType.GetTypeValue(source, out target);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool TryConvertTo<TTarget>(TTarget target, out string source)
        {
            if (target != null)
            {
                var targetType = target.GetType();
                return targetType.GetTypeStringValue(target, out source);
            }
            else
            {
                source = null;
                return true;
            }
        }
    }

    /// <summary>
    /// Json字符串转到对应类
    /// </summary>
    public class JsonStringToClassConverter : IConverter<string>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; } = -99;

        /// <summary>
        /// JsonSettings
        /// </summary>
        public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool TryConvertFrom(string source, Type targetType, out object target)
        {
            try
            {
                target = JsonConvert.DeserializeObject(source, targetType, this.JsonSettings);
                return true;
            }
            catch
            {
                target = default;
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool TryConvertTo<TTarget>(TTarget target, out string source)
        {
            try
            {
                source = JsonConvert.SerializeObject(target, JsonSettings);
                return true;
            }
            catch (Exception)
            {
                source = null;
                return false;
            }
        }
    }
}
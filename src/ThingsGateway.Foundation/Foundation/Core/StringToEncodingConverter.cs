namespace ThingsGateway.Foundation
{
    /// <summary>
    /// Json字符串转到对应类
    /// </summary>
    public class StringToEncodingConverter : IConverter<string>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }

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
                target = Encoding.Default;
                if (targetType == typeof(Encoding))
                {
                    if (source.Trim().ToUpper() == "Encoding.UTF8".ToUpper())
                    {
                        target = Encoding.UTF8;
                        return true;
                    }
                    else if (source.Trim().ToUpper() == "Encoding.ASCII".ToUpper())
                    {
                        target = Encoding.ASCII;
                        return true;
                    }
                    else if (source.Trim().ToUpper() == "Encoding.Unicode".ToUpper())
                    {
                        target = Encoding.Unicode;
                        return true;
                    }
                    else if (source.Trim().ToUpper() == "Encoding.Default".ToUpper())
                    {
                        target = Encoding.Default;
                        return true;
                    }
                }
            }
            catch
            {
                target = default;
                return false;
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool TryConvertTo(object target, out string source)
        {
            try
            {
                source = target.ToJson();
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
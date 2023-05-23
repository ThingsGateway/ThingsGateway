#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Linq;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 所有数据转换类的静态辅助方法
    /// </summary>
    public static class ByteConverterHelper
    {
        #region Public Methods

        /// <summary>
        ///设备地址可以带有的额外信息包含：
        /// DATA=XX;
        /// TEXT=XX;
        /// BCD=XX;
        /// LEN=XX;
        ///<br></br>
        /// 解析地址的附加<see cref="DataFormat"/> />参数方法，
        /// 并去掉address中的全部额外信息
        /// </summary>
        public static IThingsGatewayBitConverter GetTransByAddress(
          ref string address,
          IThingsGatewayBitConverter defaultTransform)
        {
            var strs = address.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var format = strs.FirstOrDefault(m => !m.Trim().ToUpper().Contains("DATA="))?.ToUpper();
            DataFormat dataFormat = defaultTransform.DataFormat;
            switch (format)
            {
                case "DATA=ABCD":
                    dataFormat = DataFormat.ABCD;
                    break;

                case "DATA=BADC":
                    dataFormat = DataFormat.BADC;
                    break;

                case "DATA=DCBA":
                    dataFormat = DataFormat.DCBA;
                    break;

                case "DATA=CDAB":
                    dataFormat = DataFormat.CDAB;
                    break;
            }

            //去除以上的额外信息
            address = String.Join(";", strs.Where(m =>
            (!m.Trim().ToUpper().Contains("DATA=")) &&
            (!m.Trim().ToUpper().Contains("TEXT=")) &&
            (!m.Trim().ToUpper().Contains("BCD=")) &&
            (!m.Trim().ToUpper().Contains("LEN="))
            ));

            return dataFormat != defaultTransform.DataFormat ?
                    defaultTransform.CreateByDateFormat(dataFormat) :
                    defaultTransform;
        }

        /// <summary>
        /// <inheritdoc cref="GetTransByAddress(ref string, IThingsGatewayBitConverter)"/>
        /// </summary>
        public static void GetTransByAddress(ref string address)
        {
            var strs = address.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            //去除以上的额外信息
            address = String.Join(";", strs.Where(m =>
            (!m.Trim().ToUpper().Contains("DATA=")) &&
            (!m.Trim().ToUpper().Contains("TEXT=")) &&
            (!m.Trim().ToUpper().Contains("BCD=")) &&
            (!m.Trim().ToUpper().Contains("LEN="))
            ));
        }

        /// <summary>
        /// <inheritdoc cref="GetTransByAddress(ref string, IThingsGatewayBitConverter)"/>
        /// </summary>
        public static IThingsGatewayBitConverter GetTransByAddress(
          ref string address,
          IThingsGatewayBitConverter defaultTransform,
          out int length,
          out BcdFormat bcdFormat)
        {
            length = 0;
            bcdFormat = BcdFormat.C8421;
            if (address.IsNullOrEmpty()) return defaultTransform;
            var strs = address.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var format = strs.FirstOrDefault(m => m.Trim().ToUpper().Contains("DATA="))?.ToUpper();
            DataFormat dataFormat = defaultTransform.DataFormat;
            switch (format)
            {
                case "DATA=ABCD":
                    dataFormat = DataFormat.ABCD;
                    break;

                case "DATA=BADC":
                    dataFormat = DataFormat.BADC;
                    break;

                case "DATA=DCBA":
                    dataFormat = DataFormat.DCBA;
                    break;

                case "DATA=CDAB":
                    dataFormat = DataFormat.CDAB;
                    break;
            }

            var strencoding = strs.FirstOrDefault(m => m.Trim().ToUpper().Contains("TEXT="))?.ToUpper();
            var encoding = Encoding.Default;
            switch (strencoding)
            {
                case "TEXT=UTF8":
                    encoding = Encoding.UTF8;
                    break;

                case "TEXT=ASCII":
                    encoding = Encoding.ASCII;
                    break;

                case "TEXT=Default":
                    encoding = Encoding.Default;
                    break;

                case "TEXT=Unicode":
                    encoding = Encoding.Unicode;
                    break;
            }

            var strlen = strs.FirstOrDefault(m => m.Trim().ToUpper().Contains("LEN="))?.ToUpper().Replace("LEN=", "");
            length = strlen.IsNullOrEmpty() ? (ushort)0 : Convert.ToUInt16(strlen);

            var strbCDFormat = strs.FirstOrDefault(m => m.Trim().ToUpper().Contains("BCD="))?.ToUpper();
            bcdFormat = BcdFormat.C8421;
            switch (strbCDFormat)
            {
                case "BCD=C8421":
                    bcdFormat = BcdFormat.C8421;
                    break;

                case "BCD=C2421":
                    bcdFormat = BcdFormat.C2421;
                    break;

                case "BCD=C3":
                    bcdFormat = BcdFormat.C3;
                    break;

                case "BCD=C5421":
                    bcdFormat = BcdFormat.C5421;
                    break;

                case "BCD=Gray":
                    bcdFormat = BcdFormat.Gray;
                    break;
            }

            //去除以上的额外信息
            address = String.Join(";", strs.Where(m =>
            (!m.Trim().ToUpper().Contains("DATA=")) &&
            (!m.Trim().ToUpper().Contains("TEXT=")) &&
            (!m.Trim().ToUpper().Contains("BCD=")) &&
            (!m.Trim().ToUpper().Contains("LEN="))
            ));
            var converter = defaultTransform.CreateByDateFormat(dataFormat);
            converter.Encoding = encoding;
            return converter;
        }

        #endregion Public Methods
    }
}
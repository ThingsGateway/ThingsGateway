//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.IO.Ports;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 串口配置
    /// </summary>
    public class SerialPortOption
    {
        /// <inheritdoc/>
        public SerialPortOption()
        {
        }

        /// <inheritdoc/>
        public SerialPortOption(string serialPortOption = default) => FromString(serialPortOption);

        /// <summary>
        /// 从字符串中更新值
        /// </summary>
        /// <param name="value"></param>
        public void FromString(string value)
        {
            var values = value?.SplitByHyphen();
            if (values?.Length > 6)
            {
                this.PortName = values[0];
                this.BaudRate = int.Parse(values[1]);
                this.DataBits = int.Parse(values[2]);
                this.Parity = (Parity)Enum.Parse(typeof(Parity), values[3]);
                this.StopBits = (StopBits)Enum.Parse(typeof(StopBits), values[4]);
                this.DtrEnable = values[5] == nameof(DtrEnable) ? true : false;
                this.RtsEnable = values[6] == nameof(RtsEnable) ? true : false;
            }
            else if (values?.Length == 1)
            {
                //默认
                this.PortName = values[0];
            }
        }

        #region Implicit

        /// <summary>
        /// 由字符串向<see cref="SerialPortOption"/>转换
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SerialPortOption(string value) => new SerialPortOption(value);

        #endregion Implicit

        /// <summary>
        /// 波特率
        /// </summary>
        [Description("波特率")]
        public int BaudRate { get; set; } = 9600;

        /// <summary>
        /// 数据位
        /// </summary>
        [Description("数据位")]
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// 校验位
        /// </summary>
        [Description("校验位")]
        public Parity Parity { get; set; } = Parity.None;

        /// <summary>
        /// COM
        /// </summary>
        [Description("COM口")]
        public string PortName { get; set; } = "COM1";

        /// <summary>
        /// 停止位
        /// </summary>
        [Description("停止位")]
        public StopBits StopBits { get; set; } = StopBits.One;

        /// <summary>
        /// DtrEnable，默认true
        /// </summary>
        [Description("DtrEnable")]
        public bool DtrEnable { get; set; } = true;

        /// <summary>
        /// RtsEnable，默认true
        /// </summary>
        [Description("RtsEnable")]
        public bool RtsEnable { get; set; } = true;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.PortName}-{this.BaudRate}-{this.DataBits}-{this.Parity}-{this.StopBits}-{(DtrEnable ? nameof(DtrEnable) : "DtrDisable")}-{(RtsEnable ? nameof(RtsEnable) : "RtsDisable")}";
        }
    }
}
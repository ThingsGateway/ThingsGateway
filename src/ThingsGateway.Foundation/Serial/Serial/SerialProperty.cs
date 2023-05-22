using System.ComponentModel;
using System.IO.Ports;

namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 串口属性
    /// </summary>
    public class SerialProperty
    {
        /// <summary>
        /// COM
        /// </summary>
        [Description("COM口")]
        public string PortName { get; set; } = "COM1";
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
        /// 停止位
        /// </summary>
        [Description("停止位")]
        public StopBits StopBits { get; set; } = StopBits.One;
        /// <summary>
        /// 字符串转实体类，属性用-分隔，需按顺序
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public SerialProperty Pase(string url)
        {
            var strs = url.Split('-');
            PortName = strs[0];
            BaudRate = Convert.ToInt32(strs[1]);
            DataBits = Convert.ToInt32(strs[2]);
            Parity = (Parity)Enum.Parse(typeof(Parity), strs[3]);
            StopBits = (StopBits)Enum.Parse(typeof(StopBits), strs[4]);
            return this;
        }
        /// <summary>
        /// 实体类转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{PortName}-{BaudRate}-{DataBits}-{Parity}-{StopBits}";
        }
    }
}
using System.ComponentModel;
using System.IO.Ports;

namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// ��������
    /// </summary>
    public class SerialProperty
    {
        /// <summary>
        /// COM
        /// </summary>
        [Description("COM��")]
        public string PortName { get; set; } = "COM1";
        /// <summary>
        /// ������
        /// </summary>
        [Description("������")]
        public int BaudRate { get; set; } = 9600;
        /// <summary>
        /// ����λ
        /// </summary>
        [Description("����λ")]
        public int DataBits { get; set; } = 8;
        /// <summary>
        /// У��λ
        /// </summary>
        [Description("У��λ")]
        public Parity Parity { get; set; } = Parity.None;
        /// <summary>
        /// ֹͣλ
        /// </summary>
        [Description("ֹͣλ")]
        public StopBits StopBits { get; set; } = StopBits.One;
        /// <summary>
        /// �ַ���תʵ���࣬������-�ָ����谴˳��
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
        /// ʵ����ת�ַ���
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{PortName}-{BaudRate}-{DataBits}-{Parity}-{StopBits}";
        }
    }
}
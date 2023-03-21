using System.IO.Ports;

namespace ThingsGateway.Foundation.Serial
{
    public class SerialProperty
    {
        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;

        public SerialProperty Pase(string url)
        {
            var strs = url.Split('-');
            PortName = strs[0];
            BaudRate = Convert.ToInt32(strs[1]);
            DataBits = Convert.ToInt32(strs[2]);
            Parity = (Parity)Enum.Parse(typeof(Parity),strs[3]);
            StopBits = (StopBits)Enum.Parse(typeof(StopBits),strs[4]);
            return this;
        }
        public override string ToString()
        {
            return $"{PortName}-{BaudRate}-{DataBits}-{Parity}-{StopBits}";
        }
    }
}
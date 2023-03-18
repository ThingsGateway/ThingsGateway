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

        public override string ToString()
        {
            return $"PortName:{PortName};BaudRate:{BaudRate};DataBits:{DataBits};Parity:{Parity};StopBits:{StopBits}";
        }
    }
}
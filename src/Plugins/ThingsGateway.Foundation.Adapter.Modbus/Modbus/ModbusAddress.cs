using System.Text;


namespace ThingsGateway.Foundation.Adapter.Modbus
{
    /// <summary>
    /// Modbus协议地址
    /// </summary>
    public class ModbusAddress : DeviceAddressBase
    {
        public ModbusAddress()
        {

        }

        public ModbusAddress(string address, ushort len)
        {
            Station = -1;
            AddressStart = 0;
            Parse(address, len);
        }

        public ModbusAddress(string address, byte station)
        {
            Station = station;
            AddressStart = 0;
            Parse(address, 0);
        }

        /// <summary>
        /// 读取功能码
        /// </summary>
        public int ReadFunction { get; set; }

        /// <summary>
        /// 站号信息
        /// </summary>
        public int Station { get; set; }

        /// <summary>
        /// 写入功能码
        /// </summary>
        public int WriteFunction { get; set; }

        public override void Parse(string address, int length)
        {
            Length = length;
            if (address.IndexOf(';') < 0)
            {
                Address(address);

            }
            else
            {
                string[] strArray = address.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                for (int index = 0; index < strArray.Length; ++index)
                {
                    if (strArray[index].ToUpper().StartsWith("S="))
                    {
                        if (Convert.ToInt16(strArray[index].Substring(2)) > 0)
                            Station = byte.Parse(strArray[index].Substring(2));
                    }
                    else if (strArray[index].ToUpper().StartsWith("W="))
                    {
                        if (Convert.ToInt16(strArray[index].Substring(2)) > 0)
                            this.WriteFunction = (int)byte.Parse(strArray[index].Substring(2));
                    }
                    else if (!strArray[index].Contains("="))
                    {
                        Address(strArray[index]);
                    }
                }
            }

            void Address(string address)
            {
                var readF = ushort.Parse(address.Substring(0, 1));
                if (readF > 4)
                    throw new("功能码错误");
                GetFunction(readF);
                AddressStart = int.Parse(address.Substring(1)) - 1;
            }

        }

        public override string ToString()
        {
            StringBuilder stringGeter = new StringBuilder();
            if (Station > 0)
            {
                stringGeter.Append("s=" + Station.ToString() + ";");
            }
            if (WriteFunction > 0)
            {
                stringGeter.Append("w=" + WriteFunction.ToString() + ";");
            }
            stringGeter.Append(GetFunctionString(ReadFunction) + (AddressStart + 1).ToString());
            return stringGeter.ToString();
        }

        private void GetFunction(ushort readF)
        {
            switch (readF)
            {
                case 0:
                    ReadFunction = 1;
                    break;
                case 1:
                    ReadFunction = 2;
                    break;
                case 3:
                    ReadFunction = 4;
                    break;
                case 4:
                    ReadFunction = 3;
                    break;
            }
        }
        private string GetFunctionString(int readF)
        {
            switch (readF)
            {
                case 1:
                    return "0";
                case 2:
                    return "1";
                case 3:
                    return "4";
                case 4:
                    return "3";
            }
            return "4";
        }
    }
}
namespace ThingsGateway.Foundation.Adapter.Siemens
{
    public enum S7Area : byte
    {
        PE = 0x81,
        PA = 0x82,
        MK = 0x83,
        DB = 0x84,
        CT = 0x1C,
        TM = 0x1D,
        AI = 0X06,
        AQ = 0x07,
    }

    /// <summary>
    /// 西门子PLC地址数据信息
    /// </summary>
    public class SiemensAddress : DeviceAddressBase
    {
        /// <summary>
        /// 数据块代码
        /// </summary>
        public byte DataCode { get; set; }

        /// <summary>
        /// DB块数据信息
        /// </summary>
        public ushort DbBlock { get; set; }


        public static int CalculateAddressStarted(string address, bool isCounterOrTimer = false)
        {
            if (address.IndexOf('.') < 0)
            {
                return isCounterOrTimer ? Convert.ToInt32(address) : Convert.ToInt32(address) * 8;
            }

            string[] strArray = address.Split('.');
            return (Convert.ToInt32(strArray[0]) * 8) + Convert.ToInt32(strArray[1]);
        }


        public static OperResult<SiemensAddress> ParseFrom(string address)
        {
            return ParseFrom(address, 0);
        }

        public static OperResult<SiemensAddress> ParseFrom(string address, int length)
        {
            SiemensAddress s7AddressData = new SiemensAddress();
            try
            {
                address = address.ToUpper();
                s7AddressData.Length = length;
                s7AddressData.DbBlock = 0;
                if (address.StartsWith("AI"))
                {
                    s7AddressData.DataCode = (byte)S7Area.AI;
                    if (address.StartsWith("AIX") || address.StartsWith("AIB") || address.StartsWith("AIW") || address.StartsWith("AID"))
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(3));
                    }
                    else
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                    }
                }
                else if (address.StartsWith("AQ"))
                {
                    s7AddressData.DataCode = (byte)S7Area.AQ;
                    if (address.StartsWith("AQX") || address.StartsWith("AQB") || address.StartsWith("AQW") || address.StartsWith("AQD"))
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(3));
                    }
                    else
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                    }
                }
                else if (address[0] == 'I')
                {
                    s7AddressData.DataCode = (byte)S7Area.PE;
                    if (address.StartsWith("IX") || address.StartsWith("IB") || address.StartsWith("IW") || address.StartsWith("ID"))
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                    }
                    else
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                    }
                }
                else if (address[0] == 'Q')
                {
                    s7AddressData.DataCode = (byte)S7Area.PA;
                    if (address.StartsWith("QX") || address.StartsWith("QB") || address.StartsWith("QW") || address.StartsWith("QD"))
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                    }
                    else
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                    }
                }
                else if (address[0] == 'M')
                {
                    s7AddressData.DataCode = (byte)S7Area.MK;
                    if (address.StartsWith("MX") || address.StartsWith("MB") || address.StartsWith("MW") || address.StartsWith("MD"))
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                    }
                    else
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                    }
                }
                else if (address[0] == 'D' || address.Substring(0, 2) == "DB")
                {
                    s7AddressData.DataCode = (byte)S7Area.DB;
                    string[] strArray = address.Split('.');
                    s7AddressData.DbBlock = address[1] != 'B' ? Convert.ToUInt16(strArray[0].Substring(1)) : Convert.ToUInt16(strArray[0].Substring(2));
                    string address1 = address.Substring(address.IndexOf('.') + 1);
                    if (address1.StartsWith("DBX") || address1.StartsWith("DBB") || address1.StartsWith("DBW") || address1.StartsWith("DBD"))
                    {
                        address1 = address1.Substring(3);
                    }

                    s7AddressData.AddressStart = CalculateAddressStarted(address1);
                }
                else if (address[0] == 'T')
                {
                    s7AddressData.DataCode = (byte)S7Area.TM;
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1), true);
                }
                else if (address[0] == 'C')
                {
                    s7AddressData.DataCode = (byte)S7Area.CT;
                    s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1), true);
                }
                else
                {
                    if (address[0] != 'V')
                    {
                        return new OperResult<SiemensAddress>("不支持的类型");
                    }

                    s7AddressData.DataCode = (byte)S7Area.DB;
                    s7AddressData.DbBlock = 1;
                    if (address.StartsWith("VB") || address.StartsWith("VW") || address.StartsWith("VD") || address.StartsWith("VX"))
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(2));
                    }
                    else
                    {
                        s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperResult<SiemensAddress>(ex.Message);
            }
            return OperResult.CreateSuccessResult<SiemensAddress>(s7AddressData);
        }

        public override void Parse(string address, int length)
        {
            OperResult<SiemensAddress> from = ParseFrom(address, length);
            if (!from.IsSuccess)
            {
                return;
            }

            AddressStart = from.Content.AddressStart;
            Length = from.Content.Length;
            DataCode = from.Content.DataCode;
            DbBlock = from.Content.DbBlock;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (DataCode == (byte)S7Area.TM)
            {
                return "T" + AddressStart.ToString();
            }

            if (DataCode == (byte)S7Area.CT)
            {
                return "C" + AddressStart.ToString();
            }

            if (DataCode == (byte)S7Area.AI)
            {
                return "AI" + GetActualStringAddress(AddressStart);
            }

            if (DataCode == (byte)S7Area.AQ)
            {
                return "AQ" + GetActualStringAddress(AddressStart);
            }

            if (DataCode == (byte)S7Area.PE)
            {
                return "I" + GetActualStringAddress(AddressStart);
            }

            if (DataCode == (byte)S7Area.PA)
            {
                return "Q" + GetActualStringAddress(AddressStart);
            }

            if (DataCode == (byte)S7Area.MK)
            {
                return "M" + GetActualStringAddress(AddressStart);
            }

            return DataCode == (byte)S7Area.DB ? "DB" + DbBlock.ToString() + "." + GetActualStringAddress(AddressStart) : AddressStart.ToString();
        }

        private static string GetActualStringAddress(int addressStart)
        {
            return addressStart % 8 == 0 ? (addressStart / 8).ToString() : string.Format("{0}.{1}", addressStart / 8, addressStart % 8);
        }

    }
}
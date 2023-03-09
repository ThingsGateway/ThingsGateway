namespace OpcDaClient.Da
{
    public class OpcItem
    {
        private static int _hanle = 0;
        public OpcItem(string itemId)
        {
            ItemID = itemId;
            ClientHandle = ++_hanle;
        }

        public string AccessPath { get; private set; } = "";

        public IntPtr Blob { get; set; } = IntPtr.Zero;

        public int BlobSize { get; set; } = 0;

        public int ClientHandle { get; private set; }

        /// <summary>
        /// active(1) or not(0)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 数据项在opc server的完全名称
        /// </summary>
        public string ItemID { get; private set; } = String.Empty;

        public int Quality { get; set; } = OpcRcw.Da.Qualities.OPC_QUALITY_BAD;
        public short RunTimeDataType { get; set; } = 0;
        public int ServerHandle { get; set; }
        public DateTime TimeStamp { get; set; } = new DateTime(0);
        public object Value { get; set; }
    }
}

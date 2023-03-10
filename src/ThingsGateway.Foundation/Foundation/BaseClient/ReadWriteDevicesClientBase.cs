namespace ThingsGateway.Foundation
{
    public abstract class ReadWriteDevicesClientBase : DisposableObject, IReadWriteDevice
    {
        public ushort ConnectTimeOut { get; set; } = 3000;

        public DataFormat DataFormat
        {
            get
            {
                return ThingsGatewayBitConverter.DataFormat;
            }
            set
            {
                ThingsGatewayBitConverter.DataFormat = value;
            }
        }

        public ILog Logger { get; protected set; }
        public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; protected set; } = new ThingsGatewayBitConverter(EndianType.Big);
        public ushort TimeOut { get; set; } = 3000;
        public ushort RegisterByteLength { get; set; } = 1;

        public abstract Task ConnectAsync();

        public abstract void Disconnect();

        public abstract Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token = default);

        public abstract Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default);

        public abstract void SetDataAdapter();

        public abstract Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default);

        public abstract Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken token = default);

        public virtual Task<OperResult> WriteAsync(string address, bool value, CancellationToken token = default)
        {
            return WriteAsync(address, new bool[1] { value }, token);
        }

        public virtual Task<OperResult> WriteAsync(string address, short value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return WriteAsync(address, transformParameter.GetBytes(value), token);
        }

        public virtual Task<OperResult> WriteAsync(string address, byte value, CancellationToken token = default)
        {
            return WriteAsync(address, new byte[1] { value }, token);
        }

        public virtual Task<OperResult> WriteAsync(string address, ushort value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return WriteAsync(address, transformParameter.GetBytes(value), token);
        }

        public virtual Task<OperResult> WriteAsync(string address, int value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return WriteAsync(address, transformParameter.GetBytes(value), token);
        }

        public virtual Task<OperResult> WriteAsync(string address, uint value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return WriteAsync(address, transformParameter.GetBytes(value), token);
        }

        public virtual Task<OperResult> WriteAsync(string address, long value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return WriteAsync(address, transformParameter.GetBytes(value), token);
        }

        public virtual Task<OperResult> WriteAsync(string address, ulong value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return WriteAsync(address, transformParameter.GetBytes(value), token);
        }

        public virtual Task<OperResult> WriteAsync(string address, float value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return WriteAsync(address, transformParameter.GetBytes(value), token);
        }

        public virtual Task<OperResult> WriteAsync(string address, double value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return WriteAsync(address, transformParameter.GetBytes(value), token);
        }

        public virtual Task<OperResult> WriteAsync(string address, string value, bool isBcd, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(
    ref address, ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            if (isBcd)
            {
                byte[] data = transformParameter.GetBytes(value, length, bcdFormat);
                return WriteAsync(address, data, token);
            }
            else
            {
                byte[] data = transformParameter.GetBytes(value, length);
                return WriteAsync(address, data, token);
            }
        }

        public virtual Task<OperResult> WriteAsync(string address, string value, Encoding encoding, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(
    ref address, ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            transformParameter.Encoding = encoding;
            byte[] data = transformParameter.GetBytes(value, length);
            return WriteAsync(address, data, token);
        }
    }
}
namespace ThingsGateway.Foundation
{
    public abstract class ReadWriteDevicesSerialBase : ReadWriteDevicesBase, IReadWriteDevice
    {
        public abstract void Open();
        public abstract Task OpenAsync();

        public abstract void Close();

        public abstract void SetDataAdapter();

    }
}
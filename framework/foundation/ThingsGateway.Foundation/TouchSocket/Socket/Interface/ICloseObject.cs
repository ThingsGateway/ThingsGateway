namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// 具有关闭动作的对象。
    /// </summary>
    public interface ICloseObject
    {
        /// <summary>
        /// 关闭客户端。
        /// </summary>
        /// <param name="msg"></param>
        /// <exception cref="Exception"></exception>
        void Close(string msg = TouchSocketCoreUtility.Empty);
    }
}
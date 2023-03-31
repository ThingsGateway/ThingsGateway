namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class ClientExtension
    {

        /// <summary>
        /// 获取最后活动时间。即<see cref="IClient.LastReceivedTime"/>与<see cref="IClient.LastSendTime"/>的最近值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static DateTime GetLastActiveTime<T>(this T client) where T : IClient
        {
            return client.LastSendTime > client.LastReceivedTime ? client.LastSendTime : client.LastReceivedTime;
        }

        /// <summary>
        /// 安全性关闭。不会抛出异常。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public static void SafeClose<T>(this T client, string msg) where T : ISerialClientBase
        {
            try
            {
                client.Close(msg);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 安全性发送关闭报文
        /// </summary>
        public static bool TryShutdown<T>(this T client) where T : ISerialClientBase
        {
            try
            {
                if (!client.MainSerialPort.IsOpen)
                {
                    return false;
                }
                client?.Close();
                return true;
            }
            catch
            {
            }

            return false;
        }
        #region 连接

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        public static Result TryOpen<TClient>(this TClient client) where TClient : ISerialClient
        {
            try
            {
                client.Open();
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        public static async Task<Result> TryOpenAsync<TClient>(this TClient client) where TClient : ISerialClient
        {
            try
            {
                await client.OpenAsync();
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.Message);
            }
        }

        #endregion 连接
    }
}
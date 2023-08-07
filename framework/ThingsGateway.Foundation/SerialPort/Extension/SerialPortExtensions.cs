#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.IO.Ports;

namespace ThingsGateway.Foundation;

/// <summary>
/// SocketExtension
/// </summary>
public static class SerialPortExtensions
{
    /// <summary>
    /// 会使用同步锁，保证所有数据上缓存区。
    /// </summary>
    /// <param name="serialPort"></param>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    public static void AbsoluteSend(this SerialPort serialPort, byte[] buffer, int offset, int length)
    {
        lock (serialPort)
        {
            serialPort.Write(buffer, offset, length);
        }
    }

    /// <summary>
    /// 尝试关闭<see cref="SerialPort"/>。不会抛出异常。
    /// </summary>
    /// <param name="serialPort"></param>
    public static void TryClose(this SerialPort serialPort)
    {
        lock (serialPort)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }
            }
            catch
            {

            }
        }
    }
}
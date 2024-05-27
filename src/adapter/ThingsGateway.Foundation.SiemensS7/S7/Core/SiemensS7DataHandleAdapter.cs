//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// SiemensS7DataHandleAdapter，西门子S7数据处理适配器
/// </summary>
internal class SiemensS7DataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<SiemensMessage>
{
    /// <inheritdoc/>
    protected override AdapterResult UnpackResponse(SiemensMessage request, IByteBlock byteBlock)
    {
        var result = new OperResult<AdapterResult>(); // 创建一个操作结果对象
        if (byteBlock[2] * 256 + byteBlock[3] == 7 || request.SendBytes == null) // 判断响应中的状态信息是否为7
        {
            result = new() { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } }; // 如果是7，则表示成功，设置操作结果为成功
        }
        else
        {
            var send = request.SendBytes;
            // 以请求方为准，分开返回类型校验
            switch (send[17]) // 根据请求命令中的类型字节进行分析
            {
                case 0x04: // 读取类型命令
                    result = SiemensHelper.AnalysisReadByte(send, byteBlock); // 调用辅助方法进行读取数据的解析
                    break;

                case 0x05: // 写入类型命令
                    result = SiemensHelper.AnalysisWrite(byteBlock); // 调用辅助方法进行写入数据的解析
                    break;

                default:
                    // 添加返回代码校验

                    if (byteBlock[5] == 0xD0) // 首次握手0XD0连接确认
                    {
                        result = new() { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } }; // 如果是7，则表示成功，设置操作结果为成功
                    }
                    else
                    {
                        // 其余情况判断错误代码
                        if (byteBlock[17] + byteBlock[18] > 0) // 如果错误代码不为0
                        {
                            result = new(SiemensS7Resource.Localizer["ReturnError", byteBlock[17].ToString("X2"), byteBlock[18].ToString("X2")]) { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } };
                        }
                        else
                        {
                            result = new() { Content = new AdapterResult() { Content = byteBlock.ToArray(byteBlock.Length - 2, 2), FilterResult = FilterResult.Success } }; // 如果错误代码为0，则设置操作结果为成功
                        }
                    }

                    break;
            }
        }
        request.OperCode = result.OperCode; // 设置请求的操作码
        request.ErrorMessage = result.ErrorMessage; // 设置请求的错误信息
        return result.Content; // 返回操作结果的第二部分内容
    }
}

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

namespace ThingsGateway.Foundation.Adapter.Siemens
{
    public class SiemensS7PLCDataHandleAdapter : ReadWriteDevicesTcpDataHandleAdapter<SiemensMessage>
    {
        public override byte[] PackCommand(byte[] command)
        {
            return command;
        }
        protected override void Reset()
        {
            base.Reset();
        }

        protected override OperResult<byte[]> UnpackResponse(
                          byte[] send,
                  byte[] response)
        {
            if (response[2] * 256 + response[3] == 7)
            {
                return new OperResult<byte[]>(response);
            }
            else
            {
                //以请求方为准，分开返回类型校验
                switch (send[17])
                {
                    case 0x04:
                        return SiemensHelper.AnalysisReadByte(send, response);
                    case 0x05:
                        return SiemensHelper.AnalysisWrite(response);
                }
                return OperResult.CreateSuccessResult(response);
            }
        }

        protected override SiemensMessage GetInstance()
        {
            return new SiemensMessage();
        }
    }
}

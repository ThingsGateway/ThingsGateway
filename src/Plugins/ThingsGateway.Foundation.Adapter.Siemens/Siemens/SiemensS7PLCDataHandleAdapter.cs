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
                //已请求方为准，分开返回类型校验
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

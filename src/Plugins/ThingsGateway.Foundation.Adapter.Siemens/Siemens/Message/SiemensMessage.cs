#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation.Adapter.Siemens
{
    public class SiemensMessage : MessageBase, IMessage
    {
        public override int HeadBytesLength => 4;

        public override bool CheckHeadBytes(byte[] token)
        {
            HeadBytes = token;
            byte[] headBytes = HeadBytes;
            if (headBytes == null || headBytes.Length < 4)
                BodyLength = 0;
            int length = (HeadBytes[2] * 256) + HeadBytes[3] - 4;
            if (length < 0)
                length = 0;
            BodyLength = length;
            return HeadBytes != null && HeadBytes[0] == 3 && HeadBytes[1] == 0;
        }

        protected override void SendBytesThen()
        {

        }

    }
}
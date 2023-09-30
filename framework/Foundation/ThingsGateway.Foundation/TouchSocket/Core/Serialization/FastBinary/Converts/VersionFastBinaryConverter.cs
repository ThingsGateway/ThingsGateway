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

namespace ThingsGateway.Foundation
{
    internal class VersionFastBinaryConverter : FastBinaryConverter<Version>
    {
        protected override Version Read(byte[] buffer, int offset, int len)
        {
            var byteBlock = new ValueByteBlock(buffer);
            byteBlock.Pos = offset;
            return new Version(byteBlock.ReadInt32(), byteBlock.ReadInt32(), byteBlock.ReadInt32(), byteBlock.ReadInt32());
        }

        protected override int Write(ByteBlock byteBlock, Version obj)
        {
            byteBlock.Write(obj.Major);
            byteBlock.Write(obj.Minor);
            byteBlock.Write(obj.Build);
            byteBlock.Write(obj.Revision);
            return sizeof(int) * 4;
        }
    }
}

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

/// <inheritdoc/>
internal class SiemensMessage : MessageBase, IResultMessage
{
    /// <inheritdoc/>
    public override int HeadBytesLength => 4;


    /// <inheritdoc/>
    public override bool CheckHeadBytes(byte[]? headBytes)
    {
        if (headByteBlock == null || headByteBlock.Length < 4)
            BodyLength = 0;
        int length = (headByteBlock[2] * 256) + headByteBlock[3] - 4;
        if (length < 0)
            length = 0;
        BodyLength = length;
        return headByteBlock != null && headByteBlock[0] == 3 && headByteBlock[1] == 0;
    }
}

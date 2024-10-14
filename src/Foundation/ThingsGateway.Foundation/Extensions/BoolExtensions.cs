//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// bool扩展
/// </summary>
public static class BoolExtensions
{
    /// <summary>
    /// 将bool数组转换到byte数组
    /// </summary>
    public static byte[] BoolArrayToByte(this bool[] array)
    {
        int byteLength = (array.Length + 7) / 8;
        byte[] byteArray = new byte[byteLength];

        for (int i = 0; i < array.Length; i++)
        {
            if (array[i])
            {
                int byteIndex = i / 8;
                int bitOffset = i % 8;
                byteArray[byteIndex] |= (byte)(1 << bitOffset);
            }
        }

        return byteArray;
    }
}

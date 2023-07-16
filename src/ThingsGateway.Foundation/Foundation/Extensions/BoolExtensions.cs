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

namespace ThingsGateway.Foundation.Extension;

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
        if (array == null)
            return null;
        byte[] numArray = new byte[array.Length % 8 == 0 ? array.Length / 8 : (array.Length / 8) + 1];
        for (int index = 0; index < array.Length; ++index)
        {
            if (array[index])
                numArray[index / 8] += DataHelper.GetDataByBitIndex(index % 8);
        }
        return numArray;
    }
}
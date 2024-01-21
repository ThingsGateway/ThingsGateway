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

namespace ThingsGateway.Admin.Core.Utils;

/// <summary>
/// 密码相关通用类
/// </summary>
public class PwdUtil
{
    /// <summary>
    /// 密码相似度
    /// </summary>
    /// <param name="oldPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public static double Similarity(string oldPassword, string newPassword)
    {
        var editDistance = LevenshteinDistance(oldPassword, newPassword);
        var similarity = 1.0 - (double)editDistance / (double)Math.Max(oldPassword.Length, newPassword.Length);
        return similarity * 100;
    }

    /// <summary>
    /// 计算莱文斯坦距离算法
    /// </summary>
    /// <param name="s1"></param>
    /// <param name="s2"></param>
    /// <returns></returns>
    public static int LevenshteinDistance(string s1, string s2)
    {
        var distance = new int[s1.Length + 1, s2.Length + 1];

        for (var i = 0; i <= s1.Length; i++)
        {
            distance[i, 0] = i;
        }

        for (var j = 0; j <= s2.Length; j++)
        {
            distance[0, j] = j;
        }

        for (var i = 1; i <= s1.Length; i++)
        {
            for (var j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }
        return distance[s1.Length, s2.Length];
    }
}
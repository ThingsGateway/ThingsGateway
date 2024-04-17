
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace NewLife.Algorithms
{
    /// <summary>
    /// 双线性插值
    /// </summary>
    public class BilinearInterpolation
    {
        /// <summary>
        /// 双线性插值
        /// </summary>
        public static Single[,] Process(Single[,] array, Int32 timeLength, Int32 valueLength)
        {
            var rs = new Single[timeLength, valueLength];

            var scale1 = (Single)array.GetLength(0) / timeLength;
            var scale2 = (Single)array.GetLength(1) / valueLength;

            for (var i = 0; i < timeLength; i++)
            {
                for (var j = 0; j < valueLength; j++)
                {
                    var d1 = i * scale1;
                    var d2 = j * scale2;
                    var n1 = (Int32)Math.Floor(d1);
                    var n2 = (Int32)Math.Floor(d2);
                    var leftUp = (d1 - n1) * (d2 - n2);
                    var rightUp = (n1 + 1 - d1) * (d2 - n2);
                    var rightDown = (n1 + 1 - d1) * (n2 + 1 - d2);
                    var leftDown = (d1 - n1) * (n2 + 1 - d2);
                    rs[i, j] =
                        array[n1, n2] * rightDown +
                        array[n1 + 1, n2] * leftDown +
                        array[n1 + 1, n2 + 1] * leftUp +
                        array[n1, n2 + 1] * rightUp;
                }
            }

            return rs;
        }
    }
}
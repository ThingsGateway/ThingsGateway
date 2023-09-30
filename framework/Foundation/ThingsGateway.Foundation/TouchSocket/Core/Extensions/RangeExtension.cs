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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// RangeExtension
    /// </summary>
    public static class RangeExtension
    {
        /// <summary>
        /// 枚举扩展
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static CustomIntEnumerator GetEnumerator(this Range range)
        {
            return new CustomIntEnumerator(range);
        }
    }

    /// <summary>
    /// CustomIntEnumerator
    /// </summary>
    public ref struct CustomIntEnumerator
    {
        private int m_current;
        private readonly int m_end;

        /// <summary>
        /// CustomIntEnumerator
        /// </summary>
        /// <param name="range"></param>
        public CustomIntEnumerator(Range range)
        {
            if (range.End.IsFromEnd)
            {
                throw new NotSupportedException("不支持无限枚举。");
            }
            this.m_current = range.Start.Value - 1;
            this.m_end = range.End.Value;
        }

        /// <summary>
        /// Current
        /// </summary>
        public int Current => this.m_current;

        /// <summary>
        /// MoveNext
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            this.m_current++;
            return this.m_current <= this.m_end;
        }
    }
#endif
}
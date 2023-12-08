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

using System.Diagnostics;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 依赖项属性
    /// </summary>
    [DebuggerDisplay("Name={Name},Id={Id}")]
    public class DependencyProperty<TValue> : DependencyPropertyBase, IDependencyProperty<TValue>, IEquatable<DependencyProperty<TValue>>
    {
        /// <summary>
        /// 依赖项属性
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value">依赖项属性值，一般该值应该是值类型，因为它可能会被用于多个依赖对象。</param>
        public DependencyProperty(string propertyName, TValue value)
        {
            this.DefauleValue = value;
            this.m_name = propertyName;
        }

        /// <summary>
        /// 属性名称
        /// </summary>
        protected string m_name;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TValue DefauleValue { get; private set; }

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        public string Name => this.m_name;

        /// <summary>
        /// 判断否
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(DependencyProperty<TValue> left, DependencyProperty<TValue> right)
        {
            return left.Id != right.Id;
        }

        /// <summary>
        /// 判断是
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(DependencyProperty<TValue> left, DependencyProperty<TValue> right)
        {
            return left.Id == right.Id;
        }

        /// <summary>
        /// 注册依赖项属性。
        /// <para>依赖属性的默认值，可能会应用于所有的<see cref="IDependencyObject"/></para>
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value">依赖项属性值，一般该值应该是值类型，因为它可能会被用于多个依赖对象。</param>
        /// <returns></returns>
        public static DependencyProperty<TValue> Register(string propertyName, TValue value)
        {
            var dp = new DependencyProperty<TValue>(propertyName, value);
            return dp;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DependencyProperty<TValue> other)
        {
            return this.Id == other.Id;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is DependencyProperty<TValue> obj2)
            {
                if (this.Id == obj2.Id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Id;
        }
    }

    /// <summary>
    /// DependencyPropertyBase
    /// </summary>
    public class DependencyPropertyBase
    {
        private static int m_idCount = 0;

        /// <summary>
        /// 依赖项属性
        /// </summary>
        protected DependencyPropertyBase()
        {
            this.Id = Interlocked.Increment(ref m_idCount);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Id { get; }
    }
}
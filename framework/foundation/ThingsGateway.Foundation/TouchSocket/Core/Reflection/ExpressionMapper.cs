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
using System.Linq.Expressions;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 表达式复制
    /// </summary>
    public class ExpressionMapper
    {
        private static readonly Dictionary<string, object> m_dic = new Dictionary<string, object>();

        /// <summary>
        /// 字典缓存表达式树
        /// </summary>
        public static TOut Trans<TIn, TOut>(TIn tIn)
        {
            var key = string.Format("funckey_{0}_{1}", typeof(TIn).FullName, typeof(TOut).FullName);
            if (!m_dic.ContainsKey(key))
            {
                var parameterExpression = Expression.Parameter(typeof(TIn), "p");
                var memberBindingList = new List<MemberBinding>();
                foreach (var item in typeof(TOut).GetProperties())
                {
                    var property = Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
                    MemberBinding memberBinding = Expression.Bind(item, property);
                    memberBindingList.Add(memberBinding);
                }
                foreach (var item in typeof(TOut).GetFields())
                {
                    var property = Expression.Field(parameterExpression, typeof(TIn).GetField(item.Name));
                    MemberBinding memberBinding = Expression.Bind(item, property);
                    memberBindingList.Add(memberBinding);
                }
                var memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
                var lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, parameterExpression);
                var func = lambda.Compile();//拼装是一次性的
                m_dic[key] = func;
            }
            return ((Func<TIn, TOut>)m_dic[key]).Invoke(tIn);
        }
    }
}
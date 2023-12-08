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
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 为System提供扩展。
    /// </summary>
    public static class SystemExtensions
    {
        #region 其他

        /// <summary>
        /// 安全性释放（不用判断对象是否为空）。不会抛出任何异常。
        /// </summary>
        /// <param name="dis"></param>
        /// <returns></returns>
        public static void SafeDispose(this IDisposable dis)
        {
            if (dis == default)
            {
                return;
            }
            try
            {
                dis.Dispose();
            }
            catch
            {
            }
        }

        #endregion 其他

        /// <summary>
        /// 获取自定义attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Enum enumObj) where T : Attribute
        {
            var type = enumObj.GetType();
            var enumName = Enum.GetName(type, enumObj);  //获取对应的枚举名
            var field = type.GetField(enumName);
            var attr = field.GetCustomAttribute(typeof(T), false);

            return (T)attr;
        }

        /// <summary>
        /// 格林尼治标准时间
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string ToGMTString(this DateTime dt, string v)
        {
            return dt.ToString("r", CultureInfo.InvariantCulture);
        }

#if !NETCOREAPP3_1_OR_GREATER

        /// <summary>
        /// 清除所有成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out _))
            {
            }
        }

#endif

        /// <summary>
        /// 清除所有成员
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <param name="action"></param>
        public static void Clear<T>(this ConcurrentQueue<T> queue, Action<T> action)
        {
            while (queue.TryDequeue(out var t))
            {
                action?.Invoke(t);
            }
        }

        /// <summary>
        /// 获取字节中的指定Bit的值
        /// </summary>
        /// <param name="this">字节</param>
        /// <param name="index">Bit的索引值(0-7)</param>
        /// <returns></returns>
        public static int GetBit(this byte @this, short index)
        {
            byte x;
            switch (index)
            {
                case 0: { x = 0x01; } break;
                case 1: { x = 0x02; } break;
                case 2: { x = 0x04; } break;
                case 3: { x = 0x08; } break;
                case 4: { x = 0x10; } break;
                case 5: { x = 0x20; } break;
                case 6: { x = 0x40; } break;
                case 7: { x = 0x80; } break;
                default: { return 0; }
            }
            return (@this & x) == x ? 1 : 0;
        }

        /// <summary>
        /// 设置字节中的指定Bit的值
        /// </summary>
        /// <param name="this">字节</param>
        /// <param name="index">Bit的索引值(0-7)</param>
        /// <param name="bitvalue">Bit值(0,1)</param>
        /// <returns></returns>
        public static byte SetBit(this byte @this, short index, int bitvalue)
        {
            var _byte = @this;
            if (bitvalue == 1)
            {
                switch (index)
                {
                    case 0: { return _byte |= 0x01; }
                    case 1: { return _byte |= 0x02; }
                    case 2: { return _byte |= 0x04; }
                    case 3: { return _byte |= 0x08; }
                    case 4: { return _byte |= 0x10; }
                    case 5: { return _byte |= 0x20; }
                    case 6: { return _byte |= 0x40; }
                    case 7: { return _byte |= 0x80; }
                    default: { return _byte; }
                }
            }
            else
            {
                switch (index)
                {
                    case 0: { return _byte &= 0xFE; }
                    case 1: { return _byte &= 0xFD; }
                    case 2: { return _byte &= 0xFB; }
                    case 3: { return _byte &= 0xF7; }
                    case 4: { return _byte &= 0xEF; }
                    case 5: { return _byte &= 0xDF; }
                    case 6: { return _byte &= 0xBF; }
                    case 7: { return _byte &= 0x7F; }
                    default: { return _byte; }
                }
            }
        }

        #region Tuple

        /// <summary>
        /// 获取元组的名称列表。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTupleElementNames(this ParameterInfo parameter)
        {
            return (IEnumerable<string>)DynamicMethodMemberAccessor.Default.GetValue(parameter.GetCustomAttribute(Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute")), "TransformNames");
        }

        /// <summary>
        /// 获取元组的名称列表。
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetTupleElementNames(this MemberInfo memberInfo)
        {
            return (IEnumerable<string>)DynamicMethodMemberAccessor.Default.GetValue(memberInfo.GetCustomAttribute(Type.GetType("System.Runtime.CompilerServices.TupleElementNamesAttribute")), "TransformNames");
        }

        #endregion

        /// <summary>
        /// 获取方法的确定性名称，即使在重载时，也能区分。
        /// <para>计算规则是：命名空间.类名.方法名(参数：全名)</para>
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static string GetDeterminantName(this MethodInfo methodInfo)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{methodInfo.DeclaringType?.Namespace}.{methodInfo.DeclaringType?.Name}.{methodInfo.GetName()}(");

            foreach (var item in methodInfo.GetParameters())
            {
                stringBuilder.Append(item.ParameterType.FullName);
                if (item != methodInfo.GetParameters().Last())
                {
                    stringBuilder.Append('.');
                }
            }
            stringBuilder.Append(')');
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 获取方法的方法名。主要解决显式实现时函数名称的问题。
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static string GetName(this MethodInfo methodInfo)
        {
            var r = methodInfo.Name.LastIndexOf('.');
            if (r < 0)
            {
                return methodInfo.Name;
            }
            return methodInfo.Name.Substring(r + 1, methodInfo.Name.Length - r - 1);
        }
    }
}
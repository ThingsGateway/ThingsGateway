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
using System.Reflection;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// AppMessengerExtensions
    /// </summary>
    public static class AppMessengerExtensions
    {
        /// <summary>
        /// 注册类的静态消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RegisterStatic<T>(this AppMessenger appMessenger) where T : IMessageObject
        {
            RegisterStatic(appMessenger, typeof(T));
        }

        private static MethodInfo[] GetStaticMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static MethodInfo[] GetInstanceMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 注册类的静态消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="type"></param>
        /// <exception cref="NotSupportedException"></exception>
        public static void RegisterStatic(this AppMessenger appMessenger, Type type)
        {
            var methods = GetStaticMethods(type);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is AppMessageAttribute att)
                    {
                        if (string.IsNullOrEmpty(att.Token))
                        {
                            Register(appMessenger, null, method.Name, method);
                        }
                        else
                        {
                            Register(appMessenger, null, att.Token, method);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="messageObject"></param>
        public static void Register(this AppMessenger appMessenger, IMessageObject messageObject)
        {
            var methods = GetInstanceMethods(messageObject.GetType());
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is AppMessageAttribute att)
                    {
                        if (string.IsNullOrEmpty(att.Token))
                        {
                            Register(appMessenger, messageObject, method.Name, method);
                        }
                        else
                        {
                            Register(appMessenger, messageObject, att.Token, method);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="messageObject"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="methodInfo"></param>
        /// <exception cref="MessageRegisteredException"></exception>
        public static void Register(this AppMessenger appMessenger, IMessageObject messageObject, string cancellationToken, MethodInfo methodInfo)
        {
            appMessenger.Add(cancellationToken, new MessageInstance(methodInfo, messageObject));
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register(this AppMessenger appMessenger, Action action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T>(this AppMessenger appMessenger, Action<T> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T1, T2>(this AppMessenger appMessenger, Action<T1, T2> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T1, T2, T3>(this AppMessenger appMessenger, Action<T1, T2, T3> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T1, T2, T3, T4>(this AppMessenger appMessenger, Action<T1, T2, T3, T4> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T1, T2, T3, T4, T5>(this AppMessenger appMessenger, Action<T1, T2, T3, T4, T5> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T, TReturn>(this AppMessenger appMessenger, Func<T, TReturn> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T1, T2, TReturn>(this AppMessenger appMessenger, Func<T1, T2, TReturn> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T1, T2, T3, TReturn>(this AppMessenger appMessenger, Func<T1, T2, T3, TReturn> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T1, T2, T3, T4, TReturn>(this AppMessenger appMessenger, Func<T1, T2, T3, T4, TReturn> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<T1, T2, T3, T4, T5, TReturn>(this AppMessenger appMessenger, Func<T1, T2, T3, T4, T5, TReturn> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="appMessenger"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        public static void Register<TReturn>(this AppMessenger appMessenger, Func<TReturn> action, string cancellationToken = default)
        {
            RegisterDelegate(appMessenger, cancellationToken, action);
        }

        /// <summary>
        /// 卸载消息
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="messageObject"></param>
        public static void Unregister(this AppMessenger appMessenger, IMessageObject messageObject)
        {
            appMessenger.Remove(messageObject);
        }

        /// <summary>
        /// 移除注册
        /// </summary>
        /// <param name="appMessenger"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Unregister(this AppMessenger appMessenger, string cancellationToken)
        {
            if (cancellationToken is null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }
            appMessenger.Remove(cancellationToken);
        }

        private static void RegisterDelegate(this AppMessenger appMessenger, string cancellationToken, Delegate dele)
        {
            var attributes = dele.Method.GetCustomAttributes();
            foreach (var attribute in attributes)
            {
                if (attribute is AppMessageAttribute att)
                {
                    if (cancellationToken.IsNullOrEmpty())
                    {
                        cancellationToken = string.IsNullOrEmpty(att.Token) ? dele.Method.Name : att.Token;
                    }

                    appMessenger.Add(cancellationToken, new MessageInstance(dele.Method, dele.Target));
                }
            }
        }
    }
}
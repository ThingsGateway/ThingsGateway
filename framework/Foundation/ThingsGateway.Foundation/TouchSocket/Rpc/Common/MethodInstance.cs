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
using System.ComponentModel;
using System.Reflection;

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// Rpc函数实例
    /// </summary>
    public sealed class MethodInstance : Method
    {
        private RpcAttribute[] m_rpcAttributes;
        private RpcAttribute[] m_serverRpcAttributes;

        /// <summary>
        /// 实例化一个Rpc调用函数，并在方法声明的类上操作
        /// </summary>
        /// <param name="methodInfo"></param>
        public MethodInstance(MethodInfo methodInfo) : this(methodInfo, methodInfo.DeclaringType)
        {

        }
        /// <summary>
        /// 实例化一个Rpc调用函数，并在指定类上操作
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="serverType"></param>
        public MethodInstance(MethodInfo methodInfo, Type serverType) : base(methodInfo, false)
        {
            var name = $"{serverType.Name}{methodInfo.Name}Func";
            var property = serverType.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Static);
            if (property == null)
            {
                if (GlobalEnvironment.DynamicBuilderType == DynamicBuilderType.IL)
                {
                    this.m_invoker = this.CreateILInvoker(methodInfo);
                }
                else if (GlobalEnvironment.DynamicBuilderType == DynamicBuilderType.Expression)
                {
                    this.m_invoker = this.CreateExpressionInvoker(methodInfo);
                }
            }
            else
            {
                this.m_invoker = (Func<object, object[], object>)property.GetValue(null);
            }
        }

        /// <summary>
        /// 筛选器
        /// </summary>
        public IRpcActionFilter[] Filters { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 是否为单例
        /// </summary>
        public bool IsSingleton { get; internal set; }

        /// <summary>
        /// 函数标识
        /// </summary>
        public MethodFlags MethodFlags { get; internal set; }

        /// <summary>
        /// 参数名集合
        /// </summary>
        public string[] ParameterNames { get; internal set; }

        /// <summary>
        /// 参数集合
        /// </summary>
        public ParameterInfo[] Parameters { get; internal set; }

        /// <summary>
        /// 参数类型集合，已处理out及ref，无参数时为空集合，
        /// </summary>
        public Type[] ParameterTypes { get; internal set; }

        /// <summary>
        /// Rpc属性集合
        /// </summary>
        public RpcAttribute[] RpcAttributes
        {
            get
            {
                this.m_rpcAttributes ??= this.Info.GetCustomAttributes<RpcAttribute>(true).ToArray();
                return this.m_rpcAttributes;
            }
        }

        /// <summary>
        /// 服务实例工厂
        /// </summary>
        public IRpcServerFactory ServerFactory { get; internal set; }

        /// <summary>
        /// Rpc服务属性集合
        /// </summary>
        public RpcAttribute[] ServerRpcAttributes
        {
            get
            {
                this.m_serverRpcAttributes ??= this.ServerType.GetCustomAttributes<RpcAttribute>(true).ToArray();
                return this.m_serverRpcAttributes;
            }
        }

        /// <summary>
        /// 实例类型
        /// </summary>
        public Type ServerType { get; internal set; }

        /// <summary>
        /// 获取指定类型属性标签
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAttribute<T>()
        {
            var attribute = this.GetAttribute(typeof(T));
            return attribute != null ? (T)attribute : default;
        }

        /// <summary>
        /// 获取指定类型属性标签
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public object GetAttribute(Type attributeType)
        {
            object attribute = this.RpcAttributes.FirstOrDefault((a) => { return attributeType.IsAssignableFrom(a.GetType()); });
            if (attribute != null)
            {
                return attribute;
            }

            attribute = this.ServerRpcAttributes.FirstOrDefault((a) => { return attributeType.IsAssignableFrom(a.GetType()); });
            return attribute ?? default;
        }

        /// <summary>
        /// 描述属性
        /// </summary>
        public string GetDescription()
        {
            return this.Info.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }
    }
}
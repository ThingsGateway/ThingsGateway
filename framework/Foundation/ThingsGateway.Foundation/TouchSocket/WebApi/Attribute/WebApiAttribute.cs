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
using System.Text;

namespace ThingsGateway.Foundation.WebApi
{
    /// <summary>
    /// WebApiAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WebApiAttribute : RpcAttribute
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="method"></param>
        public WebApiAttribute(HttpMethodType method)
        {
            this.Method = method;
        }

        /// <summary>
        /// 函数类型。
        /// </summary>
        public HttpMethodType Method { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override Type[] GetGenericConstraintTypes()
        {
            return new Type[] { typeof(IWebApiClientBase) };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public override string GetInvokenKey(MethodInstance methodInstance)
        {
            var i = 0;
            if (methodInstance.MethodFlags.HasFlag(MethodFlags.IncludeCallContext))
            {
                i = 1;
            }
            if (this.Method == HttpMethodType.GET)
            {
                var actionUrl = this.GetRouteUrls(methodInstance)[0];
                if (methodInstance.ParameterNames.Length > i)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append(actionUrl);
                    stringBuilder.Append('?');
                    for (; i < methodInstance.ParameterNames.Length; i++)
                    {
                        stringBuilder.Append(methodInstance.ParameterNames[i] + "={&}".Replace("&", i.ToString()));
                        if (i != methodInstance.ParameterNames.Length - 1)
                        {
                            stringBuilder.Append('&');
                        }
                    }
                    actionUrl = stringBuilder.ToString();
                }
                return $"GET:{actionUrl}";
            }
            else if (this.Method == HttpMethodType.POST)
            {
                var actionUrl = this.GetRouteUrls(methodInstance)[0];
                if (methodInstance.ParameterNames.Length > i + 1)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append(actionUrl);
                    stringBuilder.Append('?');
                    for (; i < methodInstance.ParameterNames.Length - 1; i++)
                    {
                        stringBuilder.Append(methodInstance.ParameterNames[i] + "={&}".Replace("&", i.ToString()));
                        if (i != methodInstance.ParameterNames.Length - 2)
                        {
                            stringBuilder.Append('&');
                        }
                    }
                    actionUrl = stringBuilder.ToString();
                }
                return $"POST:{actionUrl}";
            }
            else
            {
                return base.GetInvokenKey(methodInstance);
            }
        }

        /// <summary>
        /// 获取路由路径。
        /// <para>路由路径的第一个值会被当做调用值。</para>
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public virtual string[] GetRouteUrls(MethodInstance methodInstance)
        {
            if (methodInstance.GetAttribute<WebApiAttribute>() is WebApiAttribute webApiAttribute)
            {
                var urls = new List<string>();

                var attrs = methodInstance.Info.GetCustomAttributes(typeof(RouterAttribute), true);
                if (attrs.Length > 0)
                {
                    foreach (RouterAttribute item in attrs)
                    {
                        var url = item.RouteTemple.ToLower()
                            .Replace("[api]", methodInstance.ServerType.Name)
                            .Replace("[action]", webApiAttribute.GetMethodName(methodInstance, false)).ToLower();

                        if (!urls.Contains(url))
                        {
                            urls.Add(url);
                        }
                    }
                }
                else
                {
                    attrs = methodInstance.ServerType.GetCustomAttributes(typeof(RouterAttribute), true);
                    if (attrs.Length > 0)
                    {
                        foreach (RouterAttribute item in attrs)
                        {
                            var url = item.RouteTemple.ToLower()
                                .Replace("[api]", methodInstance.ServerType.Name)
                                .Replace("[action]", webApiAttribute.GetMethodName(methodInstance, false)).ToLower();

                            if (!urls.Contains(url))
                            {
                                urls.Add(url);
                            }
                        }
                    }
                    else
                    {
                        urls.Add($"/{methodInstance.Info.DeclaringType.Name}/{methodInstance.Name}".ToLower());
                    }
                }

                return urls.ToArray();
            }
            return default;
        }
    }
}
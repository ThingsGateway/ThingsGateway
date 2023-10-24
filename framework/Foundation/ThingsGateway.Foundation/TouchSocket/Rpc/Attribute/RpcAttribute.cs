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
using System.Text;

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// Rpc方法属性基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public abstract class RpcAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RpcAttribute()
        {
            this.Exceptions.Add(typeof(TimeoutException), "调用超时");
            this.Exceptions.Add(typeof(RpcInvokeException), "Rpc调用异常");
            this.Exceptions.Add(typeof(Exception), "其他异常");
        }

        /// <summary>
        /// 类生成器
        /// </summary>
        public ClassCodeGenerator ClassCodeGenerator { get; private set; }

        /// <summary>
        /// 异常提示
        /// </summary>
        public Dictionary<Type, string> Exceptions { get; } = new Dictionary<Type, string>();

        /// <summary>
        /// 生成代码
        /// </summary>
        public CodeGeneratorFlag GeneratorFlag { get; set; } =
            CodeGeneratorFlag.InstanceSync | CodeGeneratorFlag.InstanceAsync | CodeGeneratorFlag.ExtensionSync | CodeGeneratorFlag.ExtensionAsync
            | CodeGeneratorFlag.InterfaceSync | CodeGeneratorFlag.InterfaceAsync;

        /// <summary>
        /// 生成泛型方法的约束
        /// </summary>
        public Type[] GenericConstraintTypes { get; set; } = new Type[] { typeof(IRpcClient) };

        /// <summary>
        /// 调用键。
        /// </summary>
        public string InvokeKey { get; set; }

        /// <summary>
        /// 是否仅以函数名调用，当为True是，调用时仅需要传入方法名即可。
        /// </summary>
        public bool MethodInvoke { get; set; }

        /// <summary>
        /// 重新指定生成的函数名称。可以使用类似“JsonRpc_{0}”的模板格式。
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 获取注释信息
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public virtual string GetDescription(MethodInstance methodInstance)
        {
            return string.IsNullOrEmpty(methodInstance.GetDescription()) ? "无注释信息" : methodInstance.GetDescription();
        }

        /// <summary>
        /// 获取扩展的代理代码
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public virtual string GetExtensionsMethodProxyCode(MethodInstance methodInstance)
        {
            var codeString = new StringBuilder();

            var description = this.GetDescription(methodInstance);

            var parametersStr = this.GetParameters(methodInstance, out var isOut, out var isRef, out var parameters);
            var InterfaceTypes = this.GetGenericConstraintTypes();
            if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.ExtensionSync))
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{description}");
                codeString.AppendLine("///</summary>");
                foreach (var item in this.Exceptions)
                {
                    codeString.AppendLine($"/// <exception cref=\"{item.Key.FullName}\">{item.Value}</exception>");
                }

                codeString.Append("public static ");
                codeString.Append(this.GetReturn(methodInstance, false));
                codeString.Append(' ');
                codeString.Append(this.GetMethodName(methodInstance, false));
                codeString.Append("<TClient>(");//方法参数

                codeString.Append($"this TClient client");

                codeString.Append(',');
                for (var i = 0; i < parametersStr.Count; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(',');
                    }

                    codeString.Append(parametersStr[i]);
                }
                if (parametersStr.Count > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(this.GetInvokeOption());
                codeString.AppendLine(") where TClient:");

                for (var i = 0; i < InterfaceTypes.Length; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(',');
                    }

                    codeString.Append(InterfaceTypes[i].FullName);
                }

                codeString.AppendLine("{");//方法开始
                if (parametersStr.Count > 0)
                {
                    codeString.Append($"object[] parameters = new object[]");
                    codeString.Append('{');

                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType.Name.Contains("&") && parameter.IsOut)
                        {
                            codeString.Append($"default({this.GetProxyParameterName(parameter)})");
                        }
                        else
                        {
                            codeString.Append(parameter.Name);
                        }
                        if (parameter != parameters[parameters.Length - 1])
                        {
                            codeString.Append(',');
                        }
                    }
                    codeString.AppendLine("};");

                    if (isOut || isRef)
                    {
                        codeString.Append($"Type[] types = new Type[]");
                        codeString.Append('{');
                        foreach (var parameter in parameters)
                        {
                            codeString.Append($"typeof({this.GetProxyParameterName(parameter)})");
                            if (parameter != parameters[parameters.Length - 1])
                            {
                                codeString.Append(',');
                            }
                        }
                        codeString.AppendLine("};");
                    }
                }

                if (methodInstance.HasReturn)
                {
                    if (parametersStr.Count == 0)
                    {
                        codeString.Append(string.Format("{0} returnData=({0})client.Invoke", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append(string.Format("{0} returnData=({0})client.Invoke", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append(string.Format("{0} returnData=({0})client.Invoke", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                else
                {
                    if (parametersStr.Count == 0)
                    {
                        codeString.Append("client.Invoke(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append("client.Invoke(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append("client.Invoke(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                if (isOut || isRef)
                {
                    codeString.AppendLine("if(parameters!=null)");
                    codeString.AppendLine("{");
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        codeString.AppendLine(string.Format("{0}=({1})parameters[{2}];", parameters[i].Name, this.GetProxyParameterName(parameters[i]), i));
                    }
                    codeString.AppendLine("}");
                    if (isOut)
                    {
                        codeString.AppendLine("else");
                        codeString.AppendLine("{");
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].IsOut)
                            {
                                codeString.AppendLine(string.Format("{0}=default({1});", parameters[i].Name, this.GetProxyParameterName(parameters[i])));
                            }
                        }
                        codeString.AppendLine("}");
                    }
                }

                if (methodInstance.HasReturn)
                {
                    codeString.AppendLine("return returnData;");
                }

                codeString.AppendLine("}");
            }

            //以下生成异步
            if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.ExtensionAsync) /*&& !isOut && !isRef*/)
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{description}");
                codeString.AppendLine("///</summary>");
                if (methodInstance.HasReturn && !isOut && !isRef)
                {
                    codeString.Append("public static async ");
                }
                else
                {
                    codeString.Append("public static ");
                }
                codeString.Append(this.GetReturn(methodInstance, true));
                codeString.Append(' ');
                codeString.Append(this.GetMethodName(methodInstance, true));
                codeString.Append("<TClient>(");//方法参数

                codeString.Append($"this TClient client");

                codeString.Append(',');
                for (var i = 0; i < parametersStr.Count; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(',');
                    }
                    codeString.Append(parametersStr[i]);
                }
                if (parametersStr.Count > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(this.GetInvokeOption());
                codeString.AppendLine(") where TClient:");

                for (var i = 0; i < InterfaceTypes.Length; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(',');
                    }

                    codeString.Append(InterfaceTypes[i].FullName);
                }

                codeString.AppendLine("{");//方法开始

                if (parametersStr.Count > 0)
                {
                    codeString.Append($"object[] parameters = new object[]");
                    codeString.Append('{');

                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType.Name.Contains("&") && parameter.IsOut)
                        {
                            codeString.Append($"default({this.GetProxyParameterName(parameter)})");
                        }
                        else
                        {
                            codeString.Append(parameter.Name);
                        }
                        if (parameter != parameters[parameters.Length - 1])
                        {
                            codeString.Append(',');
                        }
                    }
                    codeString.AppendLine("};");

                    if (isOut || isRef)
                    {
                        codeString.Append($"Type[] types = new Type[]");
                        codeString.Append('{');
                        foreach (var parameter in parameters)
                        {
                            codeString.Append($"typeof({this.GetProxyParameterName(parameter)})");
                            if (parameter != parameters[parameters.Length - 1])
                            {
                                codeString.Append(',');
                            }
                        }
                        codeString.AppendLine("};");
                    }
                }

                if (methodInstance.HasReturn)
                {
                    if (parametersStr.Count == 0)
                    {
                        codeString.Append(string.Format("return ({0}) await client.InvokeAsync", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append(string.Format("{0} returnData=({0})client.Invoke", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append(string.Format("return ({0}) await client.InvokeAsync", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                else
                {
                    if (parametersStr.Count == 0)
                    {
                        codeString.Append("return client.InvokeAsync(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append("client.Invoke(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append("return client.InvokeAsync(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }

                if (isOut || isRef)
                {
                    codeString.AppendLine("if(parameters!=null)");
                    codeString.AppendLine("{");
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        codeString.AppendLine(string.Format("{0}=({1})parameters[{2}];", parameters[i].Name, this.GetProxyParameterName(parameters[i]), i));
                    }
                    codeString.AppendLine("}");
                    if (isOut)
                    {
                        codeString.AppendLine("else");
                        codeString.AppendLine("{");
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].IsOut)
                            {
                                codeString.AppendLine(string.Format("{0}=default({1});", parameters[i].Name, this.GetProxyParameterName(parameters[i])));
                            }
                        }
                        codeString.AppendLine("}");
                    }
                }

                if (isOut || isRef)
                {
                    if (methodInstance.HasReturn)
                    {
                        codeString.AppendLine(string.Format("return Task.FromResult<{0}>(returnData);", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                    }
                    else
                    {
                        codeString.AppendLine(string.Format("return  EasyTask.CompletedTask;"));
                    }
                }

                codeString.AppendLine("}");
            }
            return codeString.ToString();
        }

        /// <summary>
        /// 获取生成的函数泛型限定名称。默认<see cref="IRpcClient"/>
        /// </summary>
        /// <returns></returns>
        public virtual Type[] GetGenericConstraintTypes()
        {
            return this.GenericConstraintTypes;
        }

        /// <summary>
        /// 获取生成实体类时的代码块
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public virtual string GetInstanceProxyCode(MethodInstance methodInstance)
        {
            var codeString = new StringBuilder();

            var description = this.GetDescription(methodInstance);
            var parametersStr = this.GetParameters(methodInstance, out var isOut, out var isRef, out var parameters);
            if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.InstanceSync))
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{description}");
                codeString.AppendLine("///</summary>");
                foreach (var item in this.Exceptions)
                {
                    codeString.AppendLine($"/// <exception cref=\"{item.Key.FullName}\">{item.Value}</exception>");
                }

                codeString.Append("public ");
                codeString.Append(this.GetReturn(methodInstance, false));
                codeString.Append(' ');
                codeString.Append(this.GetMethodName(methodInstance, false));
                codeString.Append('(');//方法参数

                for (var i = 0; i < parametersStr.Count; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(',');
                    }
                    codeString.Append(parametersStr[i]);
                }
                if (parametersStr.Count > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(this.GetInvokeOption());
                codeString.AppendLine(")");

                codeString.AppendLine("{");//方法开始

                codeString.AppendLine("if(Client==null)");
                codeString.AppendLine("{");
                codeString.AppendLine("throw new RpcException(\"IRpcClient为空，请先初始化或者进行赋值\");");
                codeString.AppendLine("}");

                if (parametersStr.Count > 0)
                {
                    codeString.Append($"object[] parameters = new object[]");
                    codeString.Append('{');

                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType.Name.Contains("&") && parameter.IsOut)
                        {
                            codeString.Append($"default({this.GetProxyParameterName(parameter)})");
                        }
                        else
                        {
                            codeString.Append(parameter.Name);
                        }
                        if (parameter != parameters[parameters.Length - 1])
                        {
                            codeString.Append(',');
                        }
                    }
                    codeString.AppendLine("};");

                    if (isOut || isRef)
                    {
                        codeString.Append($"Type[] types = new Type[]");
                        codeString.Append('{');
                        foreach (var parameter in parameters)
                        {
                            codeString.Append($"typeof({this.GetProxyParameterName(parameter)})");
                            if (parameter != parameters[parameters.Length - 1])
                            {
                                codeString.Append(',');
                            }
                        }
                        codeString.AppendLine("};");
                    }
                }

                if (methodInstance.HasReturn)
                {
                    if (parametersStr.Count == 0)
                    {
                        codeString.Append(string.Format("{0} returnData=({0})Client.Invoke", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append(string.Format("{0} returnData=({0})Client.Invoke", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append("(");
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append(string.Format("{0} returnData=({0})Client.Invoke", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append("(");
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                else
                {
                    if (parametersStr.Count == 0)
                    {
                        codeString.Append("Client.Invoke(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append("Client.Invoke(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append("Client.Invoke(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                if (isOut || isRef)
                {
                    codeString.AppendLine("if(parameters!=null)");
                    codeString.AppendLine("{");
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        codeString.AppendLine(string.Format("{0}=({1})parameters[{2}];", parameters[i].Name, this.GetProxyParameterName(parameters[i]), i));
                    }
                    codeString.AppendLine("}");
                    if (isOut)
                    {
                        codeString.AppendLine("else");
                        codeString.AppendLine("{");
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].IsOut)
                            {
                                codeString.AppendLine(string.Format("{0}=default({1});", parameters[i].Name, this.GetProxyParameterName(parameters[i])));
                            }
                        }
                        codeString.AppendLine("}");
                    }
                }

                if (methodInstance.HasReturn)
                {
                    codeString.AppendLine("return returnData;");
                }

                codeString.AppendLine("}");
            }

            //以下生成异步
            if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.InstanceAsync))
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{description}");
                codeString.AppendLine("///</summary>");
                if (methodInstance.HasReturn && (!isOut && !isRef))
                {
                    codeString.Append("public async ");
                }
                else
                {
                    codeString.Append("public ");
                }
                codeString.Append(this.GetReturn(methodInstance, true));
                codeString.Append(' ');
                codeString.Append(this.GetMethodName(methodInstance, true));
                codeString.Append('(');//方法参数

                for (var i = 0; i < parametersStr.Count; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(',');
                    }
                    codeString.Append(parametersStr[i]);
                }
                if (parametersStr.Count > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(this.GetInvokeOption());
                codeString.AppendLine(")");

                codeString.AppendLine("{");//方法开始

                codeString.AppendLine("if(Client==null)");
                codeString.AppendLine("{");
                codeString.AppendLine("throw new RpcException(\"IRpcClient为空，请先初始化或者进行赋值\");");
                codeString.AppendLine("}");

                if (parametersStr.Count > 0)
                {
                    codeString.Append($"object[] parameters = new object[]");
                    codeString.Append('{');

                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType.Name.Contains("&") && parameter.IsOut)
                        {
                            codeString.Append($"default({this.GetProxyParameterName(parameter)})");
                        }
                        else
                        {
                            codeString.Append(parameter.Name);
                        }
                        if (parameter != parameters[parameters.Length - 1])
                        {
                            codeString.Append(',');
                        }
                    }
                    codeString.AppendLine("};");

                    if (isOut || isRef)
                    {
                        codeString.Append($"Type[] types = new Type[]");
                        codeString.Append('{');
                        foreach (var parameter in parameters)
                        {
                            codeString.Append($"typeof({this.GetProxyParameterName(parameter)})");
                            if (parameter != parameters[parameters.Length - 1])
                            {
                                codeString.Append(',');
                            }
                        }
                        codeString.AppendLine("};");
                    }
                }

                if (methodInstance.HasReturn)
                {
                    if (parametersStr.Count == 0)
                    {
                        codeString.Append(string.Format("return ({0}) await Client.InvokeAsync", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append(string.Format("{0} returnData=({0})Client.Invoke", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append(string.Format("return ({0}) await Client.InvokeAsync", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append('(');
                        codeString.Append(string.Format("typeof({0}),", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }
                else
                {
                    if (parametersStr.Count == 0)
                    {
                        codeString.Append("return Client.InvokeAsync(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, null);");
                    }
                    else if (isOut || isRef)
                    {
                        codeString.Append("Client.Invoke(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption,ref parameters,types);");
                    }
                    else
                    {
                        codeString.Append("return Client.InvokeAsync(");
                        codeString.Append($"\"{this.GetInvokenKey(methodInstance)}\"");
                        codeString.AppendLine(",invokeOption, parameters);");
                    }
                }

                if (isOut || isRef)
                {
                    codeString.AppendLine("if(parameters!=null)");
                    codeString.AppendLine("{");
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        codeString.AppendLine(string.Format("{0}=({1})parameters[{2}];", parameters[i].Name, this.GetProxyParameterName(parameters[i]), i));
                    }
                    codeString.AppendLine("}");
                    if (isOut)
                    {
                        codeString.AppendLine("else");
                        codeString.AppendLine("{");
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].IsOut)
                            {
                                codeString.AppendLine(string.Format("{0}=default({1});", parameters[i].Name, this.GetProxyParameterName(parameters[i])));
                            }
                        }
                        codeString.AppendLine("}");
                    }
                }

                if (isOut || isRef)
                {
                    if (methodInstance.HasReturn)
                    {
                        codeString.AppendLine(string.Format("return Task.FromResult<{0}>(returnData);", this.GetProxyParameterName(methodInstance.Info.ReturnParameter)));
                    }
                    else
                    {
                        codeString.AppendLine(string.Format("return  EasyTask.CompletedTask;"));
                    }
                }
                codeString.AppendLine("}");
            }


            return codeString.ToString();
        }

        /// <summary>
        /// 获取接口的代理代码
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public virtual string GetInterfaceProxyCode(MethodInstance methodInstance)
        {
            var codeString = new StringBuilder();
            var description = this.GetDescription(methodInstance);
            var parameters = this.GetParameters(methodInstance, out var isOut, out var isRef, out _);
            if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.InterfaceSync))
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{description}");
                codeString.AppendLine("///</summary>");
                foreach (var item in this.Exceptions)
                {
                    codeString.AppendLine($"/// <exception cref=\"{item.Key.FullName}\">{item.Value}</exception>");
                }

                codeString.Append(this.GetReturn(methodInstance, false));
                codeString.Append(' ');
                codeString.Append(this.GetMethodName(methodInstance, false));
                codeString.Append('(');//方法参数
                for (var i = 0; i < parameters.Count; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(',');
                    }
                    codeString.Append(parameters[i]);
                }
                if (parameters.Count > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(this.GetInvokeOption());
                codeString.AppendLine(");");
            }

            if (this.GeneratorFlag.HasFlag(CodeGeneratorFlag.InterfaceAsync))
            {
                codeString.AppendLine("///<summary>");
                codeString.AppendLine($"///{description}");
                codeString.AppendLine("///</summary>");
                foreach (var item in this.Exceptions)
                {
                    codeString.AppendLine($"/// <exception cref=\"{item.Key.FullName}\">{item.Value}</exception>");
                }

                codeString.Append(this.GetReturn(methodInstance, true));
                codeString.Append(' ');
                codeString.Append(this.GetMethodName(methodInstance, true));
                codeString.Append('(');//方法参数

                for (var i = 0; i < parameters.Count; i++)
                {
                    if (i > 0)
                    {
                        codeString.Append(',');
                    }
                    codeString.Append(parameters[i]);
                }
                if (parameters.Count > 0)
                {
                    codeString.Append(',');
                }
                codeString.Append(this.GetInvokeOption());
                codeString.AppendLine(");");
            }

            return codeString.ToString();
        }

        /// <summary>
        /// 获取调用键
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public virtual string GetInvokenKey(MethodInstance methodInstance)
        {
            if (this.MethodInvoke)
            {
                return this.GetMethodName(methodInstance, false);
            }
            else
            {
                return !this.InvokeKey.IsNullOrEmpty() ? this.InvokeKey : $"{methodInstance.ServerFromType.FullName}.{methodInstance.Name}".ToLower();
            }
        }

        /// <summary>
        /// 获取调用配置
        /// </summary>
        /// <returns></returns>
        public virtual string GetInvokeOption()
        {
            return "IInvokeOption invokeOption = default";
        }

        /// <summary>
        /// 获取生成的函数名称
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        public virtual string GetMethodName(MethodInstance methodInstance, bool isAsync)
        {
            var name = string.IsNullOrEmpty(this.MethodName) ? methodInstance.Name : this.MethodName.Format(methodInstance.Name);
            return isAsync ? name + "Async" : name;
        }

        /// <summary>
        /// 获取参数生成
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <param name="isOut"></param>
        /// <param name="isRef"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual List<string> GetParameters(MethodInstance methodInstance, out bool isOut, out bool isRef, out ParameterInfo[] parameters)
        {
            var list = new List<string>();
            isOut = false;
            isRef = false;

            if (methodInstance.IncludeCallContext)
            {
                var infos = new List<ParameterInfo>(methodInstance.Parameters);
                infos.RemoveAt(0);
                parameters = infos.ToArray();
            }
            else
            {
                parameters = methodInstance.Parameters;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var codeString = new StringBuilder();
                if (parameters[i].ParameterType.Name.Contains("&"))
                {
                    if (parameters[i].IsOut)
                    {
                        isOut = true;
                        codeString.Append(string.Format("out {0} {1}", this.GetProxyParameterName(parameters[i]), parameters[i].Name));
                    }
                    else
                    {
                        isRef = true;
                        codeString.Append(string.Format("ref {0} {1}", this.GetProxyParameterName(parameters[i]), parameters[i].Name));
                    }
                }
                else
                {
                    codeString.Append(string.Format("{0} {1}", this.GetProxyParameterName(parameters[i]), parameters[i].Name));
                }

                if (parameters[i].HasDefaultValue)
                {
                    var defaultValue = parameters[i].DefaultValue;
                    if (defaultValue == null)
                    {
                        codeString.Append(string.Format("=null"));
                    }
                    else if (defaultValue.ToString() == string.Empty)
                    {
                        codeString.Append(string.Format("=\"\""));
                    }
                    else if (defaultValue.GetType() == typeof(string))
                    {
                        codeString.Append(string.Format("=\"{0}\"", defaultValue));
                    }
                    else if (defaultValue.GetType() == typeof(bool))
                    {
                        codeString.Append(string.Format("={0}", defaultValue.ToString().ToLower()));
                    }
                    else if (typeof(ValueType).IsAssignableFrom(defaultValue.GetType()))
                    {
                        codeString.Append(string.Format("={0}", defaultValue));
                    }
                }

                list.Add(codeString.ToString());
            }

            return list;
        }

        /// <summary>
        /// 从类型获取代理名
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        public virtual string GetProxyParameterName(ParameterInfo parameterInfo)
        {
            return this.ClassCodeGenerator.GetTypeFullName(parameterInfo);
        }

        /// <summary>
        /// 获取返回值
        /// </summary>
        /// <param name="methodInstance"></param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        public virtual string GetReturn(MethodInstance methodInstance, bool isAsync)
        {
            if (isAsync)
            {
                return methodInstance.ReturnType == null ? "Task" : $"Task<{this.GetProxyParameterName(methodInstance.Info.ReturnParameter)}>";
            }
            else
            {
                return methodInstance.ReturnType == null ? "void" : this.GetProxyParameterName(methodInstance.Info.ReturnParameter);
            }
        }

        internal void SetClassCodeGenerator(ClassCodeGenerator classCodeGenerator)
        {
            this.ClassCodeGenerator = classCodeGenerator;
        }
    }
}
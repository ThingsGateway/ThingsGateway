//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#if NETFRAMEWORK || NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis;

/// <summary>指定在方法返回 ReturnValue 时，即使相应的类型允许，参数也不会为 null。</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>获取返回值条件。</summary>
    public Boolean ReturnValue { get; }

    /// <summary>使用指定的返回值条件初始化属性。</summary>
    /// <param name="returnValue"></param>
    public NotNullWhenAttribute(Boolean returnValue) => ReturnValue = returnValue;
}

#endif
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;

namespace ThingsGateway.Foundation;

/// <summary>
/// VariableRuntimeProperty
/// </summary>
public class VariableRuntimeProperty
{
    /// <summary>
    /// VariableRuntimeProperty
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="property"></param>
    public VariableRuntimeProperty(VariableRuntimeAttribute attribute, PropertyInfo property)
    {
        Attribute = attribute;
        Property = property;
    }

    /// <summary>
    /// Attribute
    /// </summary>
    public VariableRuntimeAttribute Attribute { get; }

    /// <summary>
    /// Property
    /// </summary>
    public PropertyInfo Property { get; }

    /// <summary>
    /// VariableClass
    /// </summary>
    public VariableClass VariableClass { get; set; }
}

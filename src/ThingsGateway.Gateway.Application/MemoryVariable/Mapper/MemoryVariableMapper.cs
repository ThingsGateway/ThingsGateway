//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Riok.Mapperly.Abstractions;

namespace ThingsGateway.Gateway.Application;

[Mapper(UseDeepCloning = true, EnumMappingStrategy = EnumMappingStrategy.ByName, RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class MemoryVariableMapper
{
    public static partial List<MemoryVariable> AdaptListMemoryVariable(this List<MemoryVariable> src);
    public static partial List<MemoryVariableRuntime> AdaptListMemoryVariableRuntime(this List<MemoryVariable> src);
    public static partial List<MemoryVariable> AdaptListMemoryVariable(this IEnumerable<MemoryVariableRuntime> src);
    public static partial IEnumerable<MemoryVariable> AdaptIEnumerableMemoryVariable(this IEnumerable<MemoryVariableRuntime> src);
    public static partial List<MemoryVariableRuntime> AdaptListMemoryVariableRuntime(this IEnumerable<MemoryVariable> src);

    public static partial MemoryVariableRuntime AdaptMemoryVariableRuntime(this MemoryVariableRuntime src);
    public static partial IEnumerable<MemoryVariable> AdaptListMemoryVariable(this IEnumerable<MemoryVariable> src);
    public static partial MemoryVariable AdaptMemoryVariable(this MemoryVariable src);
    public static partial MemoryVariable AdaptMemoryVariable(this MemoryVariableRuntime src);

}


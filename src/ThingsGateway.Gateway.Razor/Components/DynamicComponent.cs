//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Razor;

/// <summary>
/// 动态组件类
/// </summary>
public class ThingsGatewayDynamicComponent
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="componentType"></param>
    /// <param name="parameters">TCom 组件所需要的参数集合</param>
    public ThingsGatewayDynamicComponent(Type componentType, IDictionary<string, object?> parameters = null)
    {
        ComponentType = componentType;
        Parameters = parameters;
    }

    /// <summary>
    /// 获得/设置 组件类型
    /// </summary>
    private Type ComponentType { get; }

    /// <summary>
    /// 获得/设置 组件参数集合
    /// </summary>
    private IDictionary<string, object?> Parameters { get; set; }

    /// <summary>
    /// 创建自定义组件方法
    /// </summary>
    /// <typeparam name="TCom"></typeparam>
    /// <param name="parameters">TCom 组件所需要的参数集合</param>
    /// <returns></returns>
    public static BootstrapDynamicComponent CreateComponent<TCom>(IDictionary<string, object?> parameters = null) where TCom : IComponent
    {
        return new(typeof(TCom), parameters);
    }

    /// <summary>
    /// 创建自定义组件方法
    /// </summary>
    /// <typeparam name="TCom"></typeparam>
    /// <returns></returns>
    public static BootstrapDynamicComponent CreateComponent<TCom>() where TCom : IComponent => CreateComponent<TCom>(new Dictionary<string, object?>());

    /// <summary>
    /// 创建组件实例并渲染
    /// </summary>
    /// <returns></returns>
    public RenderFragment Render(Action<object> action = null) => builder =>
    {
        var index = 0;
#pragma warning disable ASP0006 // Do not use non-literal sequence numbers
        builder.OpenComponent(index++, ComponentType);

        if (Parameters != null)
        {
            foreach (var p in Parameters)
            {
                builder.AddAttribute(index++, p.Key, p.Value);
            }
        }
        if (action != null)
            builder.AddComponentReferenceCapture(index++, action);
        builder.CloseComponent();
#pragma warning restore ASP0006 // Do not use non-literal sequence numbers
    };
}

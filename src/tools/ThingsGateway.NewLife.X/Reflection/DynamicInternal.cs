//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Dynamic;
using System.Globalization;
using System.Reflection;

namespace ThingsGateway.NewLife.X.Reflection;

/// <summary>包装程序集内部类的动态对象</summary>
public class DynamicInternal : DynamicObject
{
    private Object? Real { get; set; }

    /// <summary>包装</summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Object Wrap(Object obj)
    {
        //if (obj == null) return null;
        if (obj.GetType().IsPublic) return obj;

        return new DynamicInternal { Real = obj };
    }

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString() => Real?.ToString() ?? nameof(DynamicInternal);

    /// <summary>类型转换</summary>
    /// <param name="binder"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override Boolean TryConvert(ConvertBinder binder, out Object? result)
    {
        result = Real;

        return true;
    }

    /// <summary>成员取值</summary>
    /// <param name="binder"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override Boolean TryGetMember(GetMemberBinder binder, out Object? result)
    {
        if (Real == null) throw new ArgumentNullException(nameof(Real));

        var property = Real.GetType().GetProperty(binder.Name, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            result = null;
        }
        else
        {
            result = property.GetValue(Real, null);
            if (result != null) result = Wrap(result);
        }
        return true;
    }

    /// <summary>调用成员</summary>
    /// <param name="binder"></param>
    /// <param name="args"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override Boolean TryInvokeMember(InvokeMemberBinder binder, Object?[]? args, out Object? result)
    {
        if (Real == null) throw new ArgumentNullException(nameof(Real));

        result = Real.GetType().InvokeMember(binder.Name, BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Real, args, CultureInfo.InvariantCulture);

        return true;
    }
}


//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 特殊方法变量信息
/// </summary>
public class VariableMethod
{
    /// <summary>
    /// 间隔时间实现
    /// </summary>
    private readonly TimeTick _timeTick;

    public VariableMethod(Method method, VariableRunTime variable, int milliSeconds = 1000)
    {
        _timeTick = new TimeTick(milliSeconds);
        MethodInfo = method;
        Variable = variable;
        variable.VariableMethod = this;
    }

    /// <summary>
    /// 需分配的变量
    /// </summary>
    public VariableRunTime Variable { get; }

    /// <summary>
    /// 方法
    /// </summary>
    public Method MethodInfo { get; }

    /// <summary>
    /// 最后一次失败原因
    /// </summary>
    public string? LastErrorMessage { get; internal set; }

    private object?[]? OS;

    /// <summary>
    /// 执行方法
    /// </summary>
    /// <param name="value">以,逗号分割的参数</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async Task<IOperResult> InvokeMethodAsync(object driverBase, string? value = null, CancellationToken cancellationToken = default)
    {
        try
        {
            object?[]? os = null;
            if (value == null && OS == null)
            {
                //默认的参数
                var addresss = Variable.RegisterAddress?.Trim()?.TrimEnd(',').Split(',') ?? Array.Empty<string>();
                //通过分号分割，并且合并参数
                var strs = addresss;

                OS = GetOS(strs, cancellationToken);
                os = OS;
            }
            else
            {
                var addresss = Variable.RegisterAddress?.Trim()?.TrimEnd(',').Split(',') ?? Array.Empty<string>();
                var values = value?.Trim()?.TrimEnd(',').Split(',') ?? Array.Empty<string>();
                //通过分号分割，并且合并参数
                var strs = DataTransUtil.SpliceArray(addresss, values);

                os = GetOS(strs, cancellationToken);
            }

            dynamic result;
            switch (MethodInfo.TaskType)
            {
                case TaskReturnType.Task:
                    await MethodInfo.InvokeAsync(driverBase, os);
                    result = new OperResult();
                    break;

                case TaskReturnType.TaskObject:
                    result = await MethodInfo.InvokeObjectAsync(driverBase, os);
                    break;

                case TaskReturnType.None:
                default:
                    result = MethodInfo.Invoke(driverBase, os);
                    break;
            }
            if (MethodInfo.HasReturn)
            {
                return result;
            }
            return new OperResult();
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    private object?[] GetOS(string[] strs, CancellationToken cancellationToken)
    {
        var method = MethodInfo;
        var ps = method.Info.GetParameters();
        var os = new object?[ps.Length];
        var index = 0;
        for (var i = 0; i < ps.Length; i++)
        {
            if (typeof(CancellationToken).IsAssignableFrom(ps[i].ParameterType))
            {
                os[i] = cancellationToken;
            }
            else
            {
                os[i] = ThingsGatewayStringConverter.Default.Deserialize(null, strs[index], ps[i].ParameterType);
                index++;
            }
        }
        return os;
    }

    /// <summary>
    /// 检测是否达到读取间隔
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool CheckIfRequestAndUpdateTime(DateTime time) => _timeTick.IsTickHappen(time);
}
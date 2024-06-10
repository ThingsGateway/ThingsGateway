//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// VariableValuePraseExtensions
/// </summary>
public static class VariableValuePraseExtensions
{
    /// <summary>
    /// 在返回的字节数组中解析每个变量的值
    /// 根据每个变量的<see cref="IVariable.Index"/>
    /// 不支持变长字符串类型变量，不能存在于变量List中
    /// </summary>
    /// <param name="protocol">设备</param>
    /// <param name="variables">设备变量List</param>
    /// <param name="buffer">返回的字节数组</param>
    /// <param name="exWhenAny">任意一个失败时抛出异常</param>
    /// <returns>解析结果</returns>
    public static OperResult PraseStructContent<T>(this IEnumerable<T> variables, IProtocol protocol, byte[] buffer, bool exWhenAny) where T : IVariable
    {
        foreach (var variable in variables)
        {
            IThingsGatewayBitConverter byteConverter = variable.ThingsGatewayBitConverter;
            var dataType = variable.DataType;
            int index = variable.Index;
            try
            {
                var data = byteConverter.GetDataFormBytes(protocol, variable.RegisterAddress, buffer, index, dataType);
                var result = Set(variable, data);
                if (exWhenAny)
                    if (!result.IsSuccess)
                        return result;
            }
            catch (Exception ex)
            {
                return new OperResult($"Error parsing byte array, address: {variable.RegisterAddress}, array length: {buffer.Length}, index: {index}, type: {dataType}", ex);
            }
        }
        return new();
        OperResult Set(IVariable organizedVariable, object num)
        {
            return organizedVariable.SetValue(num);
        }
    }
}

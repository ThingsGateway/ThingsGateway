//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

public interface IRpcService : ISingleton
{
    /// <summary>
    /// 反向RPC入口方法
    /// </summary>
    /// <param name="sourceDes">触发该方法的源说明</param>
    /// <param name="items">指定键为变量名称，值为附带方法参数或写入值，方法参数会按逗号分割解析</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> 取消令箭</param>
    /// <returns></returns>
    Task<Dictionary<string, OperResult>> InvokeDeviceMethodAsync(string sourceDes, Dictionary<string, string> items, CancellationToken cancellationToken = default);
}
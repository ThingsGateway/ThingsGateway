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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 具有设置配置的对象接口
    /// </summary>
    public interface ISetupConfigObject : IConfigObject, IPluginObject, IResolverObject
    {
        /// <summary>
        /// 配置设置项
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        void Setup(TouchSocketConfig config);

        /// <summary>
        /// 异步配置设置项
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        Task SetupAsync(TouchSocketConfig config);
    }
}

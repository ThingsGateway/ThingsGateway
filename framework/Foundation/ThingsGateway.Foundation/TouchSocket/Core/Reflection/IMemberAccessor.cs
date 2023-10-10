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
    /// 一个成员访问接口
    /// </summary>
    public interface IMemberAccessor
    {
        /// <summary>
        /// 获取指定成员的值
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        object GetValue(object instance, string memberName);

        /// <summary>
        ///设置指定成员的值
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="memberName"></param>
        /// <param name="newValue"></param>
        void SetValue(object instance, string memberName, object newValue);
    }
}
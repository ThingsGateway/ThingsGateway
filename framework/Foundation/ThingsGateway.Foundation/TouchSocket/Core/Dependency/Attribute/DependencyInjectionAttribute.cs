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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 指定依赖类型。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class DependencyInjectAttribute : Attribute
    {
        /// <summary>
        /// 默认注入配置
        /// </summary>
        public DependencyInjectAttribute()
        {

        }

        /// <summary>
        /// 使用指定Key参数注入。
        /// </summary>
        /// <param name="key"></param>
        public DependencyInjectAttribute(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// 类型，Key指定性注入。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        public DependencyInjectAttribute(Type type, string key)
        {
            this.Key = key;
            this.Type = type;
        }

        /// <summary>
        /// 类型，指定性注入。
        /// </summary>
        /// <param name="type"></param>
        public DependencyInjectAttribute(Type type)
        {
            this.Key = string.Empty;
            this.Type = type;
        }

        /// <summary>
        /// 指定键。
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 注入类型
        /// </summary>
        public Type Type { get; }
    }

    /// <summary>
    /// 指定依赖类型。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DependencyTypeAttribute : Attribute
    {
        /// <summary>
        /// 初始化一个依赖类型。当确定某个类型仅以某种特定方式注入时，可以过滤不必要的注入操作，以提高效率。
        /// </summary>
        /// <param name="type">可以叠加位域</param>
        public DependencyTypeAttribute(DependencyType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// 支持类型。
        /// </summary>
        public DependencyType Type { get; }
    }
}
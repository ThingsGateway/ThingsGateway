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

using Newtonsoft.Json;

namespace ThingsGateway.Foundation.WebApi.Swagger
{
    [JsonConverter(typeof(OpenApiStringEnumConverter))]
    internal enum OpenApiDataTypes
    {
        /// <summary>
        /// 字符串
        /// </summary>
        String,

        /// <summary>
        /// 数值
        /// </summary>
        Number,

        /// <summary>
        /// 整数
        /// </summary>
        Integer,

        /// <summary>
        /// 布尔值
        /// </summary>
        Boolean,

        /// <summary>
        /// 二进制（如文件）
        /// </summary>
        Binary,

        /// <summary>
        /// 二进制集合
        /// </summary>
        BinaryCollection,

        /// <summary>
        /// 记录值（字典）
        /// </summary>
        Record,

        /// <summary>
        /// 元组值
        /// </summary>
        Tuple,

        /// <summary>
        /// 数组
        /// </summary>
        Array,

        /// <summary>
        /// 对象
        /// </summary>
        Object,

        /// <summary>
        /// 结构
        /// </summary>
        Struct,

        /// <summary>
        /// Any
        /// </summary>
        Any
    }
}
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ThingsGateway.Logging;

/// <summary>
/// <see cref="LogMessage"/> 拓展
/// </summary>
public static class LogMessageExtensions
{
    /// <summary>
    /// 高性能创建 JSON 对象字符串
    /// </summary>
    /// <param name="_"><see cref="LogMessage"/></param>
    /// <param name="writeAction"></param>
    /// <param name="writeIndented">是否对 JSON 格式化</param>
    /// <returns><see cref="string"/></returns>
    public static string Write(this LogMessage _, Action<Utf8JsonWriter> writeAction, bool writeIndented = false)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            // 解决中文乱码问题
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = writeIndented
        });

        writeAction?.Invoke(writer);

        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// 高性能创建 JSON 数组字符串
    /// </summary>
    /// <param name="logMsg"><see cref="LogMessage"/></param>
    /// <param name="writeAction"></param>
    /// <param name="writeIndented">是否对 JSON 格式化</param>
    /// <returns><see cref="string"/></returns>
    public static string WriteArray(this LogMessage logMsg, Action<Utf8JsonWriter> writeAction, bool writeIndented = false)
    {
        return logMsg.Write(writer =>
        {
            writer.WriteStartArray();

            writeAction?.Invoke(writer);

            writer.WriteEndArray();
        }, writeIndented);
    }
}

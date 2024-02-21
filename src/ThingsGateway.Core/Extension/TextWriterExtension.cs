//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Core.Extension;

/// <summary>
/// TextWriter扩展
/// </summary>
public static class TextWriterExtension
{
    private const string DefaultBackgroundColor = "\x1B[49m";
    private const string DefaultForegroundColor = "\x1B[39m\x1B[22m";

    /// <summary>
    /// 写入流
    /// </summary>
    /// <param name="textWriter"></param>
    /// <param name="message"></param>
    /// <param name="background"></param>
    /// <param name="foreground"></param>
    public static void WriteWithColor(
        this TextWriter textWriter,
        string message,
        ConsoleColor? background,
        ConsoleColor? foreground)
    {
        var backgroundColor = background.HasValue ? GetBackgroundColorEscapeCode(background.Value) : null;
        var foregroundColor = foreground.HasValue ? GetForegroundColorEscapeCode(foreground.Value) : null;

        if (backgroundColor is not null)
        {
            textWriter.Write(backgroundColor);
        }
        if (foregroundColor is not null)
        {
            textWriter.Write(foregroundColor);
        }

        textWriter.WriteLine(message);

        if (foregroundColor is not null)
        {
            textWriter.Write(DefaultForegroundColor);
        }
        if (backgroundColor is not null)
        {
            textWriter.Write(DefaultBackgroundColor);
        }
    }

    private static string GetBackgroundColorEscapeCode(ConsoleColor color) =>
        color switch
        {
            ConsoleColor.Black => "\x1B[40m",
            ConsoleColor.DarkRed => "\x1B[41m",
            ConsoleColor.DarkGreen => "\x1B[42m",
            ConsoleColor.DarkYellow => "\x1B[43m",
            ConsoleColor.DarkBlue => "\x1B[44m",
            ConsoleColor.DarkMagenta => "\x1B[45m",
            ConsoleColor.DarkCyan => "\x1B[46m",
            ConsoleColor.Gray => "\x1B[47m",

            _ => DefaultBackgroundColor
        };

    private static string GetForegroundColorEscapeCode(ConsoleColor color) =>
        color switch
        {
            ConsoleColor.Black => "\x1B[30m",
            ConsoleColor.DarkRed => "\x1B[31m",
            ConsoleColor.DarkGreen => "\x1B[32m",
            ConsoleColor.DarkYellow => "\x1B[33m",
            ConsoleColor.DarkBlue => "\x1B[34m",
            ConsoleColor.DarkMagenta => "\x1B[35m",
            ConsoleColor.DarkCyan => "\x1B[36m",
            ConsoleColor.Gray => "\x1B[37m",
            ConsoleColor.Red => "\x1B[1m\x1B[31m",
            ConsoleColor.Green => "\x1B[1m\x1B[32m",
            ConsoleColor.Yellow => "\x1B[1m\x1B[33m",
            ConsoleColor.Blue => "\x1B[1m\x1B[34m",
            ConsoleColor.Magenta => "\x1B[1m\x1B[35m",
            ConsoleColor.Cyan => "\x1B[1m\x1B[36m",
            ConsoleColor.White => "\x1B[1m\x1B[37m",

            _ => DefaultForegroundColor
        };
}
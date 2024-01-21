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

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Tests;

public class LogTest
{
    public LogTest()
    {
    }

    [Fact]
    public void FileLogOK()
    {
        ILog logger = TextFileLogger.Create(AppContext.BaseDirectory.CombinePath("logs", this.ToString()));
        for (int i = 0; i < 100000; i++)
        {
            logger.Info("哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈");
        }
    }

    [Fact]
    public void FileLogReadOK()
    {
        string path = AppContext.BaseDirectory.CombinePath("logs", this.ToString());
        var files = TextFileReader.GetFile(path);
        Assert.NotEmpty(files);
        Assert.True(files.FirstOrDefault().IsSuccess, files.FirstOrDefault().ErrorMessage);
        //获取指定文件最后200行记录
        var result = TextFileReader.LastLog(files.FirstOrDefault().FullName, files.FirstOrDefault().Length);
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }
}
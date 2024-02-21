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

namespace ThingsGateway.Foundation.Tests;

public class JsonUtilTest
{
    [Fact]
    public void GetJTokenFromStringOK()
    {
        var data = JTokenUtil.GetJTokenFromString("ThingsGateway");
        Assert.True(data.Type == Newtonsoft.Json.Linq.JTokenType.String);
    }

    [Fact]
    public void GetJTokenFromDataOK()
    {
        string jsonString = "{\"name\":\"John\",\"age\":30}";
        var data = JTokenUtil.GetJTokenFromString(jsonString);
        var obj = JTokenUtil.GetObjectFromJToken(data);
        Assert.True(data.Type == Newtonsoft.Json.Linq.JTokenType.Object);
        Assert.Equal(obj.ToJsonString(), jsonString);
    }
}
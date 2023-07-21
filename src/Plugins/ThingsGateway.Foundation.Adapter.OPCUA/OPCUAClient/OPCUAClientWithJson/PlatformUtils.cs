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

using Opc.Ua;

using System.Text.RegularExpressions;

namespace ThingsGateway.Foundation.Adapter.OPCUA
{
    public static class PlatformUtils
    {
        public static NodeId ParsePlatformNodeIdString(string str)
        {
            const string pattern = @"^(\d+)-(?:(\d+)|(\S+))$";
            var match = Regex.Match(str, pattern);
            var isString = match.Groups[3].Length != 0;
            var isNumeric = match.Groups[2].Length != 0;

            var idStr = (isString) ? $"s={match.Groups[3]}" : $"i={match.Groups[2]}";
            var builtStr = $"ns={match.Groups[1]};" + idStr;
            NodeId nodeId = null;
            try
            {
                nodeId = new NodeId(builtStr);
            }
            catch (ServiceResultException exc)
            {
                switch (exc.StatusCode)
                {
                    case StatusCodes.BadNodeIdInvalid:
                        throw new Exception("Wrong Type Error: String is not formatted as expected (number-yyy where yyy can be string or number or guid)");
                    default:
                        throw new Exception(exc.Message);
                }
            }

            return nodeId;
        }
    }
}
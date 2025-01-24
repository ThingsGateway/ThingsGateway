//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------


using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

using ThingsGateway.Foundation;
namespace ThingsGateway;

public class UriValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var uriString = value?.ToString();
        if (uriString.IsNullOrWhiteSpace()) return ValidationResult.Success;
        // 正则表达式匹配 IPv4 格式
        var ipv4Pattern = @"^\d{1,3}(\.\d{1,3}){3}(:\d+)?$";
        // 正则表达式匹配 IPv6 格式
        var ipv6Pattern = @"^\[\*::\*\](?::\d+)?$";
        // 匹配域名格式（tcp/http）
        var domainPattern = @"^(tcp|http)://([\w.-]+)(:\d+)?$";

        // 验证端口号
        if (int.TryParse(uriString, out int port))
        {
            if (port <= 0 || port > 65535)
            {
                return new ValidationResult(DefaultResource.Localizer["InvalidPortRange"]);
            }
        }
        else if (Regex.IsMatch(uriString, ipv4Pattern))
        {
            // IPv4 验证
            string[] segments = uriString.Split(':')[0].Split('.');
            foreach (var segment in segments)
            {
                if (int.Parse(segment) > 255)
                {
                    return new ValidationResult(DefaultResource.Localizer["InvalidIPv4Segment"]);
                }
            }
        }
        else if (!Regex.IsMatch(uriString, ipv6Pattern) && !Regex.IsMatch(uriString, domainPattern))
        {
            // 其他格式验证失败
            return new ValidationResult(DefaultResource.Localizer["InvalidUriFormat"]);
        }

        // 验证通过
        return ValidationResult.Success;
    }
}

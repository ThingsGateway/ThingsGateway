// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using ThingsGateway.Extensions;

namespace ThingsGateway.SpecificationDocument;

/// <summary>
/// 修正 规范化文档 Enum 提示
/// </summary>
[SuppressSniffer]
public class EnumSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// 中文正则表达式
    /// </summary>
    private const string CHINESE_PATTERN = @"[\u4e00-\u9fa5]";

    /// <summary>
    /// 实现过滤器方法
    /// </summary>
    /// <param name="model"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        var type = context.Type;

        // 排除其他程序集的枚举
        if (type.IsEnum && App.Assemblies.Contains(type.Assembly))
        {
            model.Enum.Clear();
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{model.Description}<br />");

            var enumValues = Enum.GetValues(type);

            bool convertToNumber;
            // 定义 [EnumToNumber] 特性情况
            if (type.IsDefined(typeof(EnumToNumberAttribute), false))
            {
                var enumToNumberAttribute = type.GetCustomAttribute<EnumToNumberAttribute>(false);
                convertToNumber = enumToNumberAttribute.Enabled;
            }
            else
            {
                convertToNumber = App.Configuration.GetValue("SpecificationDocumentSettings:EnumToNumber", false);
            }

            // 包含中文情况
            if (Enum.GetNames(type).Any(v => Regex.IsMatch(v, CHINESE_PATTERN)))
            {
                convertToNumber = true;
            }

            // 获取枚举实际值类型
            var enumValueType = type.GetField("value__").FieldType;

            foreach (var value in enumValues)
            {
                var numValue = value.ChangeType(enumValueType);

                // 获取枚举成员特性
                var fieldinfo = type.GetField(Enum.GetName(type, value));
                var descriptionAttribute = fieldinfo.GetCustomAttribute<DescriptionAttribute>(true);
                model.Enum.Add(!convertToNumber
                    ? new OpenApiString(value.ToString())
                    : OpenApiAnyFactory.CreateFromJson($"{numValue}"));

                stringBuilder.Append($"&nbsp;{descriptionAttribute?.Description} {value} = {numValue}<br />");
            }
            model.Description = stringBuilder.ToString();

            if (!convertToNumber)
            {
                model.Type = "string";
                model.Format = null;
            }
        }
    }
}
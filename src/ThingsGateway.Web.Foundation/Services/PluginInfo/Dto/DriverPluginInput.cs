using Microsoft.AspNetCore.Components.Forms;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 插件添加DTO
    /// </summary>
    public class DriverPluginAddInput
    {
        /// <summary>
        /// 主程序集
        /// </summary>
        [Description("主程序集")]
        [Required(ErrorMessage = "主程序集不能为空")]
        public IBrowserFile MainFile { get; set; }
        /// <summary>
        /// 附属程序集
        /// </summary>
        [Description("附属程序集")]
        public List<IBrowserFile> OtherFiles { get; set; } = new();

    }

    /// <summary>
    /// 插件分页
    /// </summary>
    public class DriverPluginPageInput : BasePageInput
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        [Description("插件名称")]
        public string Name { get; set; }
    }

}
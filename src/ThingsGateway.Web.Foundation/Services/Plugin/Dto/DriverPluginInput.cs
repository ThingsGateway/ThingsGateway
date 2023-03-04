using Microsoft.AspNetCore.Components.Forms;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Application
{
    public class DriverPluginAddInput
    {

        [Description("主程序集")]
        [Required(ErrorMessage = "主程序集不能为空")]
        public IBrowserFile MainFile { get; set; }
        [Description("附属程序集")]
        public List<IBrowserFile> OtherFiles { get; set; } = new();

    }
    public class DriverPluginEditInput
    {
        public DriverEnum DriverTypeEnum { get; set; }

    }
    public class DriverPluginPageInput : BasePageInput
    {
        [Description("插件名称")]
        public string Name { get; set; }
    }

}
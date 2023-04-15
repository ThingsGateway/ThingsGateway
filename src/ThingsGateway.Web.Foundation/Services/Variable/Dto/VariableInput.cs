using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 添加变量DTO
    /// </summary>
    public class VariableAddInput : VariableEditInput
    {
        /// <inheritdoc/>
        [MinValue(100, ErrorMessage = "低于最小值")]
        public override int IntervalTime { get; set; } = 1000;
        /// <inheritdoc/>
        public override long DeviceId { get; set; }
    }
    /// <summary>
    /// 修改变量DTO
    /// </summary>
    public class VariableEditInput : CollectDeviceVariable, IValidatableObject
    {

        /// <inheritdoc/>
        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }

        /// <inheritdoc/>
        [MinValue(1, ErrorMessage = "不能为空")]
        public override long DeviceId { get; set; }

        /// <inheritdoc/>
        [MinValue(100, ErrorMessage = "低于最小值")]
        public override int IntervalTime { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(VariableAddress) && string.IsNullOrEmpty(OtherMethod))
                yield return new ValidationResult("变量地址或特殊方法不能同时为空", new[] { nameof(VariableAddress) });
        }
    }

    /// <summary>
    /// 变量分页查询参数
    /// </summary>
    public class VariablePageInput : BasePageInput
    {
        /// <inheritdoc/>
        [Description("变量名称")]
        public string Name { get; set; }
        /// <inheritdoc/>
        [Description("设备名称")]
        public string DeviceName { get; set; }
        /// <inheritdoc/>
        [Description("变量地址")]
        public string VariableAddress { get; set; }



    }



}
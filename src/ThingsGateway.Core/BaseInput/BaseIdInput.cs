using Furion.DataValidation;

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Core
{
    /// <summary>
    /// 主键Id输入参数
    /// </summary>
    public class BaseIdInput
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [MinValue(1, ErrorMessage = "Id不能为空")]
        [DataValidation(ValidationTypes.Numeric)]
        public virtual long Id { get; set; }
    }
}
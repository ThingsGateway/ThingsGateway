using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace ThingsGateway.Core
{
    /// <summary>
    /// 导入基础输入
    /// </summary>
    [ExcelImporter(IsLabelingError = true)]
    public class ImportPreviewInput
    {

        /// <summary>
        /// Id
        /// </summary>
        [ImporterHeader(IsIgnore = true)]
        [ExporterHeader(IsIgnore = true)]
        public long Id { get; set; }
        /// <summary>
        /// 是否有错误
        /// </summary>
        [ImporterHeader(IsIgnore = true)]
        [ExporterHeader(IsIgnore = true)]
        public bool HasError { get; set; } = false;

        /// <summary>
        /// 错误详情
        /// </summary>
        [ImporterHeader(IsIgnore = true)]
        [ExporterHeader(IsIgnore = true)]
        public IDictionary<string, string> ErrorInfo { get; set; } = new Dictionary<string, string>();
    }
}

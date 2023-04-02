namespace System.ComponentModel
{
    /// <summary>
    /// 操作事件说明
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OperDescAttribute : Attribute
    {
        /// <summary>
        /// 操作记录标识
        /// </summary>
        /// <param name="description"></param>
        /// <param name="catcategory"></param>
        public OperDescAttribute(string description, string catcategory = CateGoryConst.Log_OPERATE)
        {
            Description = description;
            Catcategory = catcategory;
        }
        /// <summary>
        /// 分类
        /// </summary>
        public string Catcategory { get; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// 记录参数
        /// </summary>
        public bool IsRecordPar { get; set; } = true;
    }
}
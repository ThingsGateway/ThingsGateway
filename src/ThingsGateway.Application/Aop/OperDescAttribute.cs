namespace System.ComponentModel
{
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

        public string Catcategory { get; }
        public string Description { get; }
        public bool IsRecordPar { get; set; } = true;
    }
}
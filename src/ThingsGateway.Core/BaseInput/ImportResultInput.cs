namespace ThingsGateway.Core
{
    public class ImportResultInput<T> where T : class
    {
        /// <summary>
        /// 数据
        /// </summary>
        public List<T> Data { get; set; }

    }
}

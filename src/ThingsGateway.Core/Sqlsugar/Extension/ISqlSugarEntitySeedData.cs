namespace ThingsGateway.Core
{
    /// <summary>
    /// 实体种子数据接口
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ISqlSugarEntitySeedData<TEntity>
        where TEntity : PrimaryIdEntity, new()
    {
        /// <summary>
        /// 种子数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> SeedData();
    }
}
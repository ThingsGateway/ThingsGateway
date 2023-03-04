namespace ThingsGateway.Application
{
    /// <summary>
    /// 用户表种子数据
    /// </summary>
    public class OpenApiUserSeedData : ISqlSugarEntitySeedData<OpenApiUser>
    {
        public IEnumerable<OpenApiUser> SeedData()
        {
            return SeedDataUtil.GetSeedData<OpenApiUser>("openapi_user.json");
        }
    }
}
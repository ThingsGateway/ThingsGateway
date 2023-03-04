namespace ThingsGateway.Application.Services.Auth
{
    public interface IOpenApiAuthService : ITransient
    {
        Task<LoginOpenApiOutPut> LoginOpenApi(LoginOpenApiInput input);

        Task LoginOut();
    }
}
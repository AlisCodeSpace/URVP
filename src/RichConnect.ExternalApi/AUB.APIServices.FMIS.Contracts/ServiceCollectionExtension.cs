using AUB.APIServices.Base.Client;

namespace AUB.APIServices.FMIS.Contracts;

public static class ServiceCollectionExtension
{
    public static void AddFMISServices(this IServiceCollection services, ServicesConfiguration config)
    {
        services.AddAubGrpcClient<Interfaces.IFMISService>("FMIS", config);
    }
}
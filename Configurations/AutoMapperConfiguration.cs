using StressApi.Mappers;

namespace StressApi.Configurations
{
    public static class AutoMapperConfiguration
    {
        public static void AddMappers(this IServiceCollection services)
        {
            services.AddAutoMapper(new Type[] { typeof(ContractMapping) });
        }
    }
}

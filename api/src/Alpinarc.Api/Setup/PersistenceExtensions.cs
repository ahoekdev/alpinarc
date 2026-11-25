using Alpinarc.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Alpinarc.Api.Setup;

public static class PersistenceExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Registers <see cref="AlpinarcDbContext"/> on Postgres, with a validated connection string.</summary>
        public IServiceCollection AddPersistence(IConfiguration configuration)
        {
            services.AddOptions<ConnectionStringsOptions>()
                .BindConfiguration(ConnectionStringsOptions.SectionName)
                .ValidateOnStartUnlessGeneratingOpenApi();

            return services.AddDbContext<AlpinarcDbContext>(options => options
                .UseNpgsql(configuration.GetConnectionString("Alpinarc"))
                .UseSnakeCaseNamingConvention());
        }
    }
}

using HurryUpHaul.Domain.Commands;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Helpers;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;

namespace HurryUpHaul.Domain
{
    public static class DomainServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services, string connectionString)
        {
            return services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommand>())
                .AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString))
                .AddSingleton<IDateTimeProvider, DateTimeProvider>();
        }
    }
}
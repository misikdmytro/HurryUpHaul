using HurryUpHaul.Domain.Commands;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Helpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;

namespace HurryUpHaul.Domain
{
    public static class DomainServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services, string connectionString)
        {
            services
                .AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            return services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommand>())
                .AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString))
                .AddSingleton<IDateTimeProvider, DateTimeProvider>();
        }
    }
}
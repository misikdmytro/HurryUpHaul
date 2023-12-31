using HurryUpHaul.Domain.Commands;
using HurryUpHaul.Domain.Configuration;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Helpers;
using HurryUpHaul.Domain.Profiles;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HurryUpHaul.Domain
{
    public static class DomainServiceCollectionExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services,
            string connectionString,
            IConfiguration jwt)
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
                .AddOptions()
                .Configure<JwtSettings>(jwt)
                .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommand>())
                .AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString))
                .AddAutoMapper(cfg => cfg.AddProfile<AppProfile>())
                .AddSingleton<IDateTimeProvider, DateTimeProvider>();
        }
    }
}
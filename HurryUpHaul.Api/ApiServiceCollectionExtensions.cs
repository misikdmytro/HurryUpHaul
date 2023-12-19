using FluentValidation;

using HurryUpHaul.Api.Validators;

using HurryUpHaul.Contracts.Http;

namespace HurryUpHaul.Api
{
    public static class ApiServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            return services
                .AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>()
                .AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>()
                .AddScoped<IValidator<AuthenticateUserRequest>, AuthenticateUserRequestValidator>()
                .AddScoped<IValidator<AdminUpdateUserRequest>, AdminUpdateUserRequestValidator>();
        }
    }
}
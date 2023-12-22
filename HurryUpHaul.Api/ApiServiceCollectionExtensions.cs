using FluentValidation;

using HurryUpHaul.Api.Helpers;
using HurryUpHaul.Api.Validators;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Authorization;

namespace HurryUpHaul.Api
{
    public static class ApiServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IAuthorizationMiddlewareResultHandler, AppAuthorizationMiddlewareResultHandler>()
                .AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>()
                .AddScoped<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>()
                .AddScoped<IValidator<AuthenticateUserRequest>, AuthenticateUserRequestValidator>()
                .AddScoped<IValidator<AdminUpdateUserRequest>, AdminUpdateUserRequestValidator>()
                .AddScoped<IValidator<CreateRestaurantRequest>, CreateRestaurantRequestValidator>();
        }
    }
}
using Bogus;

using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Domain.Constants;

using Microsoft.AspNetCore.Identity;

namespace HurryUpHaul.IntegrationTests
{
    internal class UserInfo
    {
        public string Id { get; init; }
        public string Username { get; init; }
        public string Password { get; init; }
        public string Role { get; init; }
        public string Token { get; init; }
    }

    [Collection("IntegrationTests")]
    public class Base : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
    {
        protected readonly Faker _faker;
        private protected readonly IApiClient _apiClient;
        private readonly IServiceScope _scope;

        public Base(CustomWebApplicationFactory<Program> factory)
        {
            _apiClient = new ApiClient(factory.CreateClient());
            _scope = factory.Services.CreateScope();
            _faker = new Faker();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scope?.Dispose();
                _apiClient?.Dispose();
            }
        }

        private protected async Task<UserInfo> CreateTestUser(string role = Roles.User)
        {
            // 1. registration
            var registrationRequest = new RegisterUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = $"Aa1!_{_faker.Internet.Password()}"
            };
            var registrationResponse = await _apiClient.RegisterUser(registrationRequest);

            // 2. role assignment
            var roleManager = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = _scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(registrationRequest.Username);
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains(role))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }

                await userManager.RemoveFromRolesAsync(user, roles);
                await userManager.AddToRoleAsync(user, role);
            }

            // 3. authentication
            var authenticateResponse = await _apiClient.AuthenticateUser(new AuthenticateUserRequest
            {
                Username = registrationRequest.Username,
                Password = registrationRequest.Password
            });

            return new UserInfo
            {
                Id = registrationResponse.UserId,
                Username = registrationRequest.Username,
                Password = registrationRequest.Password,
                Role = role,
                Token = authenticateResponse.Token
            };
        }
    }
}
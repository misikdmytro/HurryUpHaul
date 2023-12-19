using System.Net.Mime;
using System.Text;

using Bogus;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;

using Newtonsoft.Json;

namespace HurryUpHaul.IntegrationTests
{
    public class UserInfo
    {
        public string Username { get; init; }
        public string Password { get; init; }
        public string Role { get; init; }
        public string Token { get; init; }
    }

    public class Base : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly Faker _faker;
        private readonly IServiceScope _scope;

        public Base(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _faker = new Faker();
            _scope = _factory.Services.CreateScope();
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
            }
        }

        protected async Task<UserInfo> CreateTestUser(string role = "customer")
        {
            // 1. registration
            var client = _factory.CreateClient();
            var registrationRequest = new RegisterUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = $"Aa1!_{_faker.Internet.Password()}"
            };
            using var registrationContent = new StringContent(JsonConvert.SerializeObject(registrationRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var registrationResponse = await client.PostAsync("api/users", registrationContent);
            registrationResponse.EnsureSuccessStatusCode();

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
            var authenticateRequest = new AuthenticateUserRequest
            {
                Username = registrationRequest.Username,
                Password = registrationRequest.Password
            };

            using var authenticateContent = new StringContent(JsonConvert.SerializeObject(authenticateRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var authenticateResponse = await client.PostAsync("api/users/token", authenticateContent);
            authenticateResponse.EnsureSuccessStatusCode();

            var authenticateResponseContent = await authenticateResponse.Content.ReadFromJsonAsync<AuthenticateUserResponse>();
            return new UserInfo
            {
                Username = registrationRequest.Username,
                Password = registrationRequest.Password,
                Role = role,
                Token = authenticateResponseContent.Token
            };
        }
    }
}
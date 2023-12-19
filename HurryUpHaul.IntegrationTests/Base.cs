using System.Net.Mime;
using System.Text;

using Bogus;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Mvc.Testing;

using Newtonsoft.Json;

namespace HurryUpHaul.IntegrationTests
{
    public class Base : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> _factory;
        protected readonly Faker _faker;

        public Base(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _faker = new Faker();
        }

        protected async Task<string> CreateTestUser()
        {
            // 1. registration
            var client = _factory.CreateClient();
            var registrationRequest = new RegisterUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = "TestPassword123!?"
            };
            using var registrationContent = new StringContent(JsonConvert.SerializeObject(registrationRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var registrationResponse = await client.PostAsync("api/users", registrationContent);
            registrationResponse.EnsureSuccessStatusCode();

            // 2. authentication
            var authenticateRequest = new AuthenticateUserRequest
            {
                Username = registrationRequest.Username,
                Password = registrationRequest.Password
            };

            using var authenticateContent = new StringContent(JsonConvert.SerializeObject(authenticateRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var authenticateResponse = await client.PostAsync("api/users/token", authenticateContent);
            authenticateResponse.EnsureSuccessStatusCode();

            var authenticateResponseContent = await authenticateResponse.Content.ReadFromJsonAsync<AuthenticateUserResponse>();
            return authenticateResponseContent.Token;
        }
    }
}
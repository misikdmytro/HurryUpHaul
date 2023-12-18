using System.Net;
using System.Net.Mime;
using System.Text;

using Bogus;

using FluentAssertions;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Mvc.Testing;

using Newtonsoft.Json;

namespace HurryUpHaul.IntegrationTests
{
    public class UsersTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Faker _faker;

        public UsersTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _faker = new Faker();
        }

        [Fact]
        public async Task RegisterUserShouldDoIt()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new RegisterUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = "TestPassword123!?"
            };
            using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);

            // Act
            using var response = await client.PostAsync("api/users", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData(null, "TestPassword123!?", "'Username' must not be empty.", "Username must only contain the following characters: abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+")]
        [InlineData("", "TestPassword123!?", "'Username' must not be empty.")]
        [InlineData("TestUser", null, "'Password' must not be empty.")]
        [InlineData("TestUser", "", "'Password' must not be empty.", "The length of 'Password' must be at least 8 characters. You entered 0 characters.")]
        [InlineData("TestUser", "short", "The length of 'Password' must be at least 8 characters. You entered 5 characters.")]
        [InlineData("TestUser", "longnodigitsnouppercase", "Passwords must have at least one non alphanumeric character.", "Passwords must have at least one digit ('0'-'9').", "Passwords must have at least one uppercase ('A'-'Z').")]
        [InlineData("TestUser", "long123nouppercase", "Passwords must have at least one non alphanumeric character.", "Passwords must have at least one uppercase ('A'-'Z').")]
        [InlineData("TestUser", "Long123nospecial", "Passwords must have at least one non alphanumeric character.")]
        [InlineData("TestUser", "Long!nodigits", "Passwords must have at least one digit ('0'-'9').")]
        public async Task RegisterUserShouldReturnBadRequestWhenUsernameOrPasswordIsNullOrEmpty(string username, string password, params string[] errors)
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new RegisterUserRequest
            {
                Username = username,
                Password = password
            };
            using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);

            // Act
            using var response = await client.PostAsync("api/users", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().NotBeNullOrEmpty();
            responseContent.Errors.Should().BeEquivalentTo(errors);
        }
    }
}
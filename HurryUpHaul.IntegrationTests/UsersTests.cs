using System.Net;
using System.Net.Mime;
using System.Text;

using FluentAssertions;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Mvc.Testing;

using Newtonsoft.Json;

namespace HurryUpHaul.IntegrationTests
{
    public class UsersTests : Base
    {
        public UsersTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task RegisterAndAuthenticateUserShouldDoIt()
        {
            // 1. registration
            var client = _factory.CreateClient();
            var registerRequest = new RegisterUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = "TestPassword123!?"
            };
            using var registerContent = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var registerResponse = await client.PostAsync("api/users", registerContent);

            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 2. authentication
            var authenticateRequest = new AuthenticateUserRequest
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password
            };

            using var authenticateContent = new StringContent(JsonConvert.SerializeObject(authenticateRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var authenticateResponse = await client.PostAsync("api/users/token", authenticateContent);

            authenticateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var authenticateResponseContent = await authenticateResponse.Content.ReadFromJsonAsync<AuthenticateUserResponse>();

            authenticateResponseContent.Should().NotBeNull();
            authenticateResponseContent.Token.Should().NotBeNullOrEmpty();

            // 3. me
            using var meHttpRequest = new HttpRequestMessage(HttpMethod.Get, "api/users/me");
            meHttpRequest.Headers.Add("Authorization", $"Bearer {authenticateResponseContent.Token}");

            using var meResponse = await client.SendAsync(meHttpRequest);

            meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var meResponseContent = await meResponse.Content.ReadFromJsonAsync<MeResponse>();

            meResponseContent.Should().NotBeNull();
            meResponseContent.Username.Should().Be(registerRequest.Username);
            meResponseContent.Role.Should().Be("customer");
        }

        [Theory]
        [InlineData(null, "TestPassword123!?", "'Username' must not be empty.")]
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
            responseContent.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task AuthenticateUserShouldReturnBadRequestWhenUserDoesNotExists()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new AuthenticateUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = "TestPassword123!?"
            };
            using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);

            // Act
            using var response = await client.PostAsync("api/users/token", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().HaveCount(1);
            responseContent.Errors.First().Should().Be("Invalid username or password.");
        }

        [Fact]
        public async Task AuthenticateUserShouldReturnBadRequestWhenPasswordIsIncorrect()
        {
            // Arrange
            var client = _factory.CreateClient();
            var registerRequest = new RegisterUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = "TestPassword123!?"
            };
            using var registerContent = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var registerResponse = await client.PostAsync("api/users", registerContent);

            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var authenticateRequest = new AuthenticateUserRequest
            {
                Username = registerRequest.Username,
                Password = "IncorrectPassword123!?"
            };
            using var authenticateContent = new StringContent(JsonConvert.SerializeObject(authenticateRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            // Act
            using var authenticateResponse = await client.PostAsync("api/users/token", authenticateContent);

            // Assert
            authenticateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var authenticateResponseContent = await authenticateResponse.Content.ReadFromJsonAsync<ErrorResponse>();

            authenticateResponseContent.Should().NotBeNull();
            authenticateResponseContent.Errors.Should().HaveCount(1);
            authenticateResponseContent.Errors.First().Should().Be("Invalid username or password.");
        }
    }
}
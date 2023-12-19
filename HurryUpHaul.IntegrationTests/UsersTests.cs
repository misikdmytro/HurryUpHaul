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
            meResponseContent.Roles.Should().BeEquivalentTo(["customer"]);
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

        public static IEnumerable<object[]> AdminUpdateUserDataSuccess => new object[][]
        {
            [
                new string[] { "merchant" },
                new string[] { "customer" },
                new string[] { "merchant" }
            ],
            [
                new string[] { "merchant" },
                Array.Empty<string>(),
                new string[] { "customer", "merchant" }
            ],
            [
                Array.Empty<string>(),
                new string[] { "customer" },
                Array.Empty<string>()
            ],
            [
                new string[] { "admin", "merchant" },
                Array.Empty<string>(),
                new string[] { "customer", "admin", "merchant" }
            ]
        };

        [Theory]
        [MemberData(nameof(AdminUpdateUserDataSuccess))]
        public async Task AdminUpdateUserShouldUpdateUserToMerchant(string[] rolesToAdd, string[] rolesToRemove, string[] expectedRoles)
        {
            var client = _factory.CreateClient();

            // 1. create customer
            var user = await CreateTestUser();

            // 2. create admin user
            var admin = await CreateTestUser("admin");

            // 3. add role 'merchant' and remove role 'customer'
            var adminUpdateRequest = new AdminUpdateUserRequest
            {
                Username = user.Username,
                Roles = rolesToAdd
                    .Select(x => new UpdateRole
                    {
                        Role = x,
                        Action = UpdateRoleAction.Add
                    })
                    .Concat(rolesToRemove.Select(x => new UpdateRole
                    {
                        Role = x,
                        Action = UpdateRoleAction.Remove
                    }))
                    .ToArray()
            };

            using var adminUpdateHttpRequest = new HttpRequestMessage(HttpMethod.Put, "api/users/admin");
            adminUpdateHttpRequest.Content = new StringContent(JsonConvert.SerializeObject(adminUpdateRequest), Encoding.UTF8, MediaTypeNames.Application.Json);
            adminUpdateHttpRequest.Headers.Add("Authorization", $"Bearer {admin.Token}");

            using var adminUpdateResponse = await client.SendAsync(adminUpdateHttpRequest);

            adminUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 4. authenticate again as user
            var authenticateRequest = new AuthenticateUserRequest
            {
                Username = user.Username,
                Password = user.Password
            };

            using var authenticateContent = new StringContent(JsonConvert.SerializeObject(authenticateRequest), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var authenticateResponse = await client.PostAsync("api/users/token", authenticateContent);

            authenticateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var authenticateResponseContent = await authenticateResponse.Content.ReadFromJsonAsync<AuthenticateUserResponse>();

            // 5. 'me' as user
            using var meHttpRequest = new HttpRequestMessage(HttpMethod.Get, "api/users/me");
            meHttpRequest.Headers.Add("Authorization", $"Bearer {authenticateResponseContent.Token}");

            using var meResponse = await client.SendAsync(meHttpRequest);

            meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var meResponseContent = await meResponse.Content.ReadFromJsonAsync<MeResponse>();

            meResponseContent.Should().NotBeNull();
            meResponseContent.Roles.Should().BeEquivalentTo(expectedRoles);
        }
    }
}
using System.Net;

using FluentAssertions;

using Flurl.Http;

using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Domain.Constants;

using Microsoft.AspNetCore.Mvc.Testing;

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
            var registerRequest = new RegisterUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = "TestPassword123!?"
            };
            var registerResponse = await _apiClient.RegisterUser(registerRequest);

            registerResponse.UserId.Should().NotBeNullOrEmpty();

            // 2. authentication
            var authenticateResponse = await _apiClient.AuthenticateUser(new AuthenticateUserRequest
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password
            });

            authenticateResponse.Should().NotBeNull();
            authenticateResponse.Token.Should().NotBeNullOrEmpty();

            // 3. me
            var meResponse = await _apiClient.Me(authenticateResponse.Token);

            meResponse.Should().NotBeNull();
            meResponse.Username.Should().Be(registerRequest.Username);
            meResponse.Roles.Should().BeEquivalentTo([Roles.User]);
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
            try
            {
                await _apiClient.RegisterUser(new RegisterUserRequest
                {
                    Username = username,
                    Password = password
                });

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().BeEquivalentTo(errors);
            }
        }

        [Fact]
        public async Task AuthenticateUserShouldReturnBadRequestWhenUserDoesNotExists()
        {
            try
            {
                await _apiClient.AuthenticateUser(new AuthenticateUserRequest
                {
                    Username = $"test_{_faker.Database.Random.Uuid():N}",
                    Password = "TestPassword123!?"
                });

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().HaveCount(1);
                responseContent.Errors.First().Should().Be("Invalid username or password.");
            }
        }

        [Fact]
        public async Task AuthenticateUserShouldReturnBadRequestWhenPasswordIsIncorrect()
        {
            // 1. registration
            var registerRequest = new RegisterUserRequest
            {
                Username = $"test_{_faker.Database.Random.Uuid():N}",
                Password = "TestPassword123!?"
            };
            await _apiClient.RegisterUser(registerRequest);

            try
            {
                // 2. authentication
                await _apiClient.AuthenticateUser(new AuthenticateUserRequest
                {
                    Username = registerRequest.Username,
                    Password = "IncorrectPassword123!?"
                });
                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().HaveCount(1);
                responseContent.Errors.First().Should().Be("Invalid username or password.");
            }
        }

        public static IEnumerable<object[]> AdminUpdateUserDataSuccess => new object[][]
        {
            [
                new string[] { Roles.Admin },
                new string[] { Roles.User },
                new string[] { Roles.Admin }
            ],
            [
                new string[] { Roles.Admin },
                Array.Empty<string>(),
                new string[] { Roles.User, Roles.Admin }
            ],
            [
                Array.Empty<string>(),
                new string[] { Roles.User },
                Array.Empty<string>()
            ]
        };

        [Theory]
        [MemberData(nameof(AdminUpdateUserDataSuccess))]
        public async Task AdminUpdateUserShouldUpdateUser(string[] rolesToAdd, string[] rolesToRemove, string[] expectedRoles)
        {
            // 1. create user
            var user = await CreateTestUser();

            // 2. create admin user
            var admin = await CreateTestUser("admin");

            // 3. update user
            await _apiClient.AdminUpdate(new AdminUpdateUserRequest
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
            }, admin.Token);

            // 4. authenticate again as user
            var authResult = await _apiClient.AuthenticateUser(new AuthenticateUserRequest
            {
                Username = user.Username,
                Password = user.Password
            });

            // 5. 'me' as user
            var meResult = await _apiClient.Me(authResult.Token);

            meResult.Should().NotBeNull();
            meResult.Roles.Should().BeEquivalentTo(expectedRoles);
        }

        [Fact]
        public async Task AdminUpdateUserShouldReturnBadRequestWhenRoleIsNotDefined()
        {
            // 1. create user
            var user = await CreateTestUser();

            // 2. create admin user
            var admin = await CreateTestUser("admin");

            try
            {
                // 3. update user
                await _apiClient.AdminUpdate(new AdminUpdateUserRequest
                {
                    Username = user.Username,
                    Roles =
                    [
                        new()
                        {
                            Role = "notdefined",
                            Action = UpdateRoleAction.Add
                        }
                    ]
                }, admin.Token);

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().HaveCount(1);
                responseContent.Errors.First().Should().Be("Roles must only contain the following roles: user, admin.");
            }
        }

        [Fact]
        public async Task AdminUpdateUserShouldReturnBadRequestWhenTheSameRoleAddedAndRemoved()
        {
            // 1. create user
            var user = await CreateTestUser();

            // 2. create admin user
            var admin = await CreateTestUser("admin");

            try
            {
                // 3. update user
                await _apiClient.AdminUpdate(new AdminUpdateUserRequest
                {
                    Username = user.Username,
                    Roles =
                    [
                        new()
                        {
                            Role = Roles.User,
                            Action = UpdateRoleAction.Add
                        },
                        new()
                        {
                            Role = Roles.User,
                            Action = UpdateRoleAction.Remove
                        }
                    ]
                }, admin.Token);

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().HaveCount(1);
                responseContent.Errors.First().Should().Be("Roles must not contain duplicate roles.");
            }
        }

        [Fact]
        public async Task AdminUpdateUserShouldReturnBadRequestWhenRolesAreEmpty()
        {
            // 1. create user
            var user = await CreateTestUser();

            // 2. create admin user
            var admin = await CreateTestUser("admin");

            try
            {
                // 3. update user
                await _apiClient.AdminUpdate(new AdminUpdateUserRequest
                {
                    Username = user.Username,
                    Roles = []
                }, admin.Token);

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().HaveCount(1);
                responseContent.Errors.First().Should().Be("'Roles' must not be empty.");
            }
        }
    }
}
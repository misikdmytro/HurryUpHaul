using System.Net;

using FluentAssertions;

using Flurl.Http;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Mvc.Testing;

namespace HurryUpHaul.IntegrationTests
{
    public class RestaurantsTests : Base
    {
        public RestaurantsTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateAndGetRestaurantShouldDoIt()
        {
            // 1. create restaurant
            var admin = await CreateTestUser("admin");
            var owner = await CreateTestUser();
            var user = await CreateTestUser();

            var restarauntName = _faker.Company.CompanyName();

            var result = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = restarauntName,
                ManagersIds = [admin.Id, owner.Id]
            }, admin.Token);

            result.Should().NotBeNull();
            result.RestaurantId.Should().NotBeNullOrEmpty();

            // 2. get restaurant as admin
            AssertRestarauntAsOwner(await _apiClient.GetRestaurant(result.RestaurantId, admin.Token), result.RestaurantId);

            // 3. get restaurant as owner
            AssertRestarauntAsOwner(await _apiClient.GetRestaurant(result.RestaurantId, owner.Token), result.RestaurantId);

            // 4. get restaurant as user
            AssertRestarauntAsGuest(await _apiClient.GetRestaurant(result.RestaurantId, user.Token), result.RestaurantId);

            // 5. get restaurant as anonymous
            AssertRestarauntAsGuest(await _apiClient.GetRestaurant(result.RestaurantId), result.RestaurantId);

            void AssertRestarauntAsOwner(GetRestaurantResponse result, string expectedRestarauntId)
            {
                result.Should().NotBeNull();
                result.Restaurant.Should().NotBeNull();
                result.Restaurant.Id.Should().Be(expectedRestarauntId);
                result.Restaurant.Name.Should().Be(restarauntName);
                result.Restaurant.Managers.Should().BeEquivalentTo([admin.Username, owner.Username]);
                result.Restaurant.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
            }

            void AssertRestarauntAsGuest(GetRestaurantResponse result, string expectedRestarauntId)
            {
                result.Should().NotBeNull();
                result.Restaurant.Should().NotBeNull();
                result.Restaurant.Id.Should().Be(expectedRestarauntId);
                result.Restaurant.Name.Should().Be(restarauntName);
                result.Restaurant.Managers.Should().BeNull();
                result.Restaurant.CreatedAt.Should().BeNull();
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CreateRestaurantShouldReturnBadRequestWhenNameIsNullOrEmpty(string name)
        {
            var user = await CreateTestUser("admin");

            try
            {
                await _apiClient.CreateRestaurant(new CreateRestaurantRequest
                {
                    Name = name,
                    ManagersIds = [user.Id]
                }, user.Token);

                Assert.Fail("Expected FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);
                result.Errors.First().Should().Be("'Name' must not be empty.");
            }
        }

        [Fact]
        public async Task CreateRestaurantShouldReturnBadRequestWhenManagersIdsIsEmpty()
        {
            var user = await CreateTestUser("admin");

            try
            {
                await _apiClient.CreateRestaurant(new CreateRestaurantRequest
                {
                    Name = "Test Restaurant",
                    ManagersIds = []
                }, user.Token);

                Assert.Fail("Expected FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);

                result.Errors.First().Should().Be("'Managers Ids' must not be empty.");
            }
        }

        [Fact]
        public async Task CreateRestaurantShouldReturnBadRequestWhenThereAreMoreThan10Managers()
        {
            var user = await CreateTestUser("admin");

            try
            {
                await _apiClient.CreateRestaurant(new CreateRestaurantRequest
                {
                    Name = "Test Restaurant",
                    ManagersIds = Enumerable.Range(1, 11).Select(_ => Guid.NewGuid().ToString()).ToArray()
                }, user.Token);

                Assert.Fail("Expected FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);

                result.Errors.First().Should().Be("Managers Ids must not contain more than 10 values.");
            }
        }

        [Fact]
        public async Task CreateRestaurantShouldReturnUnauthenticatedWhenUserIsNotAuthenticated()
        {
            var user = await CreateTestUser();

            try
            {
                await _apiClient.CreateRestaurant(new CreateRestaurantRequest
                {
                    Name = "Test Restaurant",
                    ManagersIds = [user.Id]
                });

                Assert.Fail("Expected FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);
                result.Errors.First().Should().Be("You are not authorized to access this resource.");
            }
        }

        [Fact]
        public async Task CreateRestaurantShouldBeForbiddenForUsers()
        {
            var user = await CreateTestUser();

            try
            {
                await _apiClient.CreateRestaurant(new CreateRestaurantRequest
                {
                    Name = "Test Restaurant",
                    ManagersIds = [user.Id]
                }, user.Token);

                Assert.Fail("Expected FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);
                result.Errors.First().Should().Be("You are not authorized to access this resource.");
            }
        }

        [Fact]
        public async Task GetRestaurantOrdersShouldReturnCorrectPage()
        {
            // Arrange
            var admin = await CreateTestUser("admin");
            var user = await CreateTestUser();

            var restaurantResult = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [admin.Id]
            }, admin.Token);

            var expectedOrders = Enumerable.Empty<string>();

            for (int i = 0; i < 10; i++)
            {
                var createOrderRequest = new CreateOrderRequest
                {
                    RestaurantId = restaurantResult.RestaurantId,
                    Details = _faker.Lorem.Sentence(),
                };

                var createOrderResult = await _apiClient.CreateOrder(createOrderRequest, user.Token);

                // page requested later
                if (i >= 1 && i <= 3)
                {
                    expectedOrders = expectedOrders.Prepend(createOrderResult.OrderId);
                }
            }

            // Act
            var result = await _apiClient.GetRestaurantOrders(restaurantResult.RestaurantId, 3, 3, admin.Token);

            // Assert
            result.Should().NotBeNull();
            result.Orders.Should().HaveCount(3);
            result.Orders.Select(o => o.Id).Should().BeEquivalentTo(expectedOrders);
        }

        [Theory]
        [InlineData(0, 10, "Page size must be greater than 0.")]
        [InlineData(10, 0, "Page number must be greater than 0.")]
        [InlineData(-1, 10, "Page size must be greater than 0.")]
        [InlineData(10, -1, "Page number must be greater than 0.")]
        [InlineData(-1, -1, "Page size must be greater than 0.", "Page number must be greater than 0.")]
        [InlineData(1001, 1, "Page size must be less than or equal to 1000.")]
        [InlineData(1001, -1, "Page size must be less than or equal to 1000.", "Page number must be greater than 0.")]
        public async Task GetRestaurantOrdersShouldReturnBadRequestWhenPageSizeOrPageNumberIsInvalid(int pageSize, int pageNumber, params string[] errors)
        {
            // Arrange
            var admin = await CreateTestUser("admin");

            // Act
            try
            {
                await _apiClient.GetRestaurantOrders(Guid.NewGuid().ToString(), pageSize, pageNumber, admin.Token);

                Assert.Fail("Expected FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                // Assert
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().BeEquivalentTo(errors);
            }
        }

        [Fact]
        public async Task GetRestaurantOrdersShouldReturnNotFoundWhenRestaurantDoesNotExist()
        {
            // Arrange
            var admin = await CreateTestUser("admin");
            var restaurantId = Guid.NewGuid().ToString();

            // Act
            try
            {
                await _apiClient.GetRestaurantOrders(restaurantId, 10, 1, admin.Token);

                Assert.Fail("Expected FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                // Assert
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().BeEquivalentTo([$"Restaurant with ID '{restaurantId}' not found."]);
            }
        }

        [Fact]
        public async Task GetRestaurantOrdersShouldReturnForbiddenForUsers()
        {
            // Arrange
            var user = await CreateTestUser();
            var admin = await CreateTestUser("admin");

            var restaurantResult = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [admin.Id]
            }, admin.Token);

            // Act
            try
            {
                await _apiClient.GetRestaurantOrders(restaurantResult.RestaurantId, 10, 1, user.Token);

                Assert.Fail("Expected FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                // Assert
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().BeEquivalentTo(["You are not authorized to view this restaurant's orders."]);
            }
        }

        [Fact]
        public async Task GetRestaurantOrdersShouldReturnResultForManagers()
        {
            // Arrange
            var owner = await CreateTestUser();
            var admin = await CreateTestUser("admin");

            var restaurantResult = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [owner.Id]
            }, admin.Token);

            await _apiClient.CreateOrder(new CreateOrderRequest
            {
                RestaurantId = restaurantResult.RestaurantId,
                Details = _faker.Lorem.Sentence(),
            }, owner.Token);

            // Act
            var result = await _apiClient.GetRestaurantOrders(restaurantResult.RestaurantId, 10, 1, owner.Token);

            // Assert
            result.Should().NotBeNull();
            result.Orders.Should().HaveCount(1);
        }
    }
}
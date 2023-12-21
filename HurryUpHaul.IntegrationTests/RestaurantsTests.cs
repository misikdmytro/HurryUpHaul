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
                result.Restaurant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
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

                result.Errors.First().Should().Be("Managers Ids must not contain more than 10 values");
            }
        }
    }
}
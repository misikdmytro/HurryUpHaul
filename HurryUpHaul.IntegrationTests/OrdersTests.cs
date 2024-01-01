using System.Net;

using FluentAssertions;

using Flurl.Http;

using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Contracts.Models;

namespace HurryUpHaul.IntegrationTests
{
    public class OrdersTests : Base
    {
        public OrdersTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Theory]
        [InlineData("details")]
        [InlineData("détails")]
        [InlineData("细节")]
        [InlineData("details!@#$%^&*()_+")]
        [InlineData("1234567890")]
        [InlineData("details with spaces")]
        [InlineData("details with punctuation.")]
        [InlineData("details with emoji 🤓")]
        public async Task CreateAndGetOrderShouldDoIt(string details)
        {
            // 1. create restaurant
            var admin = await CreateTestUser("admin");

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [admin.Id]
            }, admin.Token);

            // 2. create order
            var user = await CreateTestUser();

            var createRequest = new CreateOrderRequest
            {
                RestaurantId = restaurant.RestaurantId,
                Details = details
            };
            var createResult = await _apiClient.CreateOrder(createRequest, user.Token);

            createResult.Should().NotBeNull();
            createResult.OrderId.Should().NotBeNullOrEmpty();

            // 3. get order
            var getResult = await _apiClient.GetOrder(createResult.OrderId, user.Token);

            getResult.Should().NotBeNull();
            getResult.Order.Should().NotBeNull();
            getResult.Order.Id.Should().Be(createResult.OrderId);
            getResult.Order.Details.Should().Be(details);
            getResult.Order.CreatedBy.Should().NotBeNullOrEmpty();
            getResult.Order.CreatedAt.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(5));
            getResult.Order.LastUpdatedAt.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(5));
            getResult.Order.Status.Should().Be(OrderStatus.Created);
        }

        [Fact]
        public async Task GetOrderShouldFailForNonCreator()
        {
            // 1. create restaurant
            var admin = await CreateTestUser("admin");

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [admin.Id]
            }, admin.Token);

            // 2. create order
            var user1 = await CreateTestUser();

            var creadeOrderResult = await _apiClient.CreateOrder(new CreateOrderRequest
            {
                RestaurantId = restaurant.RestaurantId,
                Details = _faker.Lorem.Sentence()
            }, user1.Token);

            // 3. get order
            var user2 = await CreateTestUser();

            try
            {
                await _apiClient.GetOrder(creadeOrderResult.OrderId, user2.Token);
                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().HaveCount(1);
                responseContent.Errors.First().Should().Be("You are not authorized to view this order.");
            }
        }

        [Fact]
        public async Task GetOrderShouldReturnOrderForManagerAndAdmin()
        {
            // 1. create restaurant
            var admin = await CreateTestUser("admin");
            var owner = await CreateTestUser();

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [owner.Id]
            }, admin.Token);

            // 2. create order
            var user = await CreateTestUser();

            var creadeOrderRequest = new CreateOrderRequest
            {
                RestaurantId = restaurant.RestaurantId,
                Details = _faker.Lorem.Sentence()
            };
            var creadeOrderResult = await _apiClient.CreateOrder(creadeOrderRequest, user.Token);

            // 3. get order as manager
            AssertGetOrder(await _apiClient.GetOrder(creadeOrderResult.OrderId, owner.Token));

            // 4. get order as admin
            AssertGetOrder(await _apiClient.GetOrder(creadeOrderResult.OrderId, admin.Token));

            void AssertGetOrder(GetOrderResponse result)
            {
                result.Should().NotBeNull();
                result.Order.Should().NotBeNull();
                result.Order.Id.Should().Be(creadeOrderResult.OrderId);
                result.Order.Details.Should().Be(creadeOrderRequest.Details);
                result.Order.CreatedBy.Should().Be(user.Username);
                result.Order.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
                result.Order.LastUpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
                result.Order.Status.Should().Be(OrderStatus.Created);
            }
        }

        [Fact]
        public async Task GetOrderShouldReturnNotFoundWhenOrderDoesNotExist()
        {
            // Arrange
            var user = await CreateTestUser();
            var orderId = Guid.NewGuid().ToString();

            try
            {
                // Act
                await _apiClient.GetOrder(orderId, user.Token);
                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                // Assert
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().HaveCount(1);
                responseContent.Errors.First().Should().Be($"Order with ID '{orderId}' not found.");
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CreateOrderShouldReturnBadRequestWhenDetailsIsNullOrEmpty(string details)
        {
            // 1. create restaurant
            var owner = await CreateTestUser("admin");
            var user = await CreateTestUser();

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [owner.Id]
            }, owner.Token);

            try
            {
                // 2. create order
                await _apiClient.CreateOrder(new CreateOrderRequest
                {
                    RestaurantId = restaurant.RestaurantId,
                    Details = details
                }, user.Token);

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);
                result.Errors.First().Should().Be("'Details' must not be empty.");
            }
        }

        [Fact]
        public async Task CreateOrderShouldReturnUnauthorizedWhenNotAuthenticated()
        {
            // 1. create restaurant
            var owner = await CreateTestUser("admin");

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [owner.Id]
            }, owner.Token);

            try
            {
                // 2. create order
                await _apiClient.CreateOrder(new CreateOrderRequest
                {
                    RestaurantId = restaurant.RestaurantId,
                    Details = _faker.Lorem.Sentence()
                });

                Assert.Fail("Should have thrown FlurlHttpException");
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
        public async Task CreateShouldReturnBadRequestWhenRestaurantDoesNotExist()
        {
            var user = await CreateTestUser();
            var restaurantId = Guid.NewGuid().ToString();

            try
            {
                var createResult = await _apiClient.CreateOrder(new CreateOrderRequest
                {
                    RestaurantId = restaurantId,
                    Details = _faker.Lorem.Sentence()
                }, user.Token);

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);
                result.Errors.First().Should().Be($"Restaurant with ID '{restaurantId}' not found.");
            }
        }

        [Theory]
        [InlineData(OrderStatus.OrderAccepted)]
        [InlineData(OrderStatus.Cancelled)]
        [InlineData(OrderStatus.OrderAccepted, OrderStatus.InProgress, OrderStatus.WaitingDelivery, OrderStatus.Delivering, OrderStatus.Completed)]
        public async Task UpdateShouldBeAllowedForAdmin(params OrderStatus[] statuses)
        {
            // 1. create restaurant
            var admin = await CreateTestUser("admin");
            var owner = await CreateTestUser();

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [owner.Id]
            }, admin.Token);

            // 2. create order
            var user = await CreateTestUser();

            var createRequest = new CreateOrderRequest
            {
                RestaurantId = restaurant.RestaurantId,
                Details = _faker.Lorem.Sentence()
            };
            var createResult = await _apiClient.CreateOrder(createRequest, user.Token);

            // 3. update order
            foreach (var status in statuses)
            {
                await _apiClient.UpdateOrder(createResult.OrderId, new UpdateOrderRequest
                {
                    Status = status
                }, admin.Token);

                // 4. get order
                var getResult = await _apiClient.GetOrder(createResult.OrderId, admin.Token);

                getResult.Should().NotBeNull();
                getResult.Order.Should().NotBeNull();
                getResult.Order.Status.Should().Be(status);
            }
        }

        [Theory]
        [InlineData(OrderStatus.OrderAccepted)]
        [InlineData(OrderStatus.Cancelled)]
        [InlineData(OrderStatus.OrderAccepted, OrderStatus.InProgress, OrderStatus.WaitingDelivery, OrderStatus.Delivering, OrderStatus.Completed)]
        public async Task UpdateShouldBeAllowedForManagers(params OrderStatus[] statuses)
        {
            // 1. create restaurant
            var admin = await CreateTestUser("admin");
            var owner = await CreateTestUser();

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [owner.Id]
            }, admin.Token);

            // 2. create order
            var user = await CreateTestUser();

            var createRequest = new CreateOrderRequest
            {
                RestaurantId = restaurant.RestaurantId,
                Details = _faker.Lorem.Sentence()
            };
            var createResult = await _apiClient.CreateOrder(createRequest, user.Token);

            // 3. update order
            foreach (var status in statuses)
            {
                await _apiClient.UpdateOrder(createResult.OrderId, new UpdateOrderRequest
                {
                    Status = status
                }, owner.Token);

                // 4. get order
                var getResult = await _apiClient.GetOrder(createResult.OrderId, owner.Token);

                getResult.Should().NotBeNull();
                getResult.Order.Should().NotBeNull();
                getResult.Order.Status.Should().Be(status);
            }
        }

        [Fact]
        public async Task UpdateOrderShouldBeForbiddenForUsers()
        {
            // 1. create restaurant
            var admin = await CreateTestUser("admin");

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [admin.Id]
            }, admin.Token);

            // 2. create order
            var user = await CreateTestUser();

            var createRequest = new CreateOrderRequest
            {
                RestaurantId = restaurant.RestaurantId,
                Details = _faker.Lorem.Sentence()
            };
            var createResult = await _apiClient.CreateOrder(createRequest, user.Token);

            try
            {
                // 3. update order
                await _apiClient.UpdateOrder(createResult.OrderId, new UpdateOrderRequest
                {
                    Status = OrderStatus.OrderAccepted
                }, user.Token);

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);
                result.Errors.First().Should().Be($"User '{user.Username}' is not authorized to update order with ID '{createResult.OrderId}'.");
            }
        }

        [Theory]
        [InlineData(OrderStatus.Created)]
        [InlineData(OrderStatus.OrderAccepted, OrderStatus.Completed)]
        [InlineData(OrderStatus.OrderAccepted, OrderStatus.InProgress, OrderStatus.Completed)]
        [InlineData(OrderStatus.OrderAccepted, OrderStatus.InProgress, OrderStatus.WaitingDelivery, OrderStatus.WaitingDelivery)]
        public async Task UpdateOrderShouldReturnBadRequestForWrongTransition(params OrderStatus[] statuses)
        {
            // 1. create restaurant
            var admin = await CreateTestUser("admin");

            var restaurant = await _apiClient.CreateRestaurant(new CreateRestaurantRequest
            {
                Name = _faker.Company.CompanyName(),
                ManagersIds = [admin.Id]
            }, admin.Token);

            // 2. create order
            var user = await CreateTestUser();

            var createRequest = new CreateOrderRequest
            {
                RestaurantId = restaurant.RestaurantId,
                Details = _faker.Lorem.Sentence()
            };
            var createResult = await _apiClient.CreateOrder(createRequest, user.Token);

            foreach (var status in statuses.SkipLast(1))
            {
                // 3. update order
                await _apiClient.UpdateOrder(createResult.OrderId, new UpdateOrderRequest
                {
                    Status = status
                }, admin.Token);
            }

            // 4. wrong update
            var wrongStatus = statuses.Last();

            try
            {
                await _apiClient.UpdateOrder(createResult.OrderId, new UpdateOrderRequest
                {
                    Status = wrongStatus
                }, admin.Token);

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

                var result = await ex.GetResponseJsonAsync<ErrorResponse>();

                result.Should().NotBeNull();
                result.Errors.Should().HaveCount(1);
                result.Errors.First().Should().Be($"Order with ID '{createResult.OrderId}' cannot be updated to status '{wrongStatus:G}'.");
            }
        }

        [Fact]
        public async Task UpdateOrderShouldReturnNotFoundWhenOrderDoesNotExist()
        {
            // Arrange
            var admin = await CreateTestUser("admin");
            var orderId = Guid.NewGuid().ToString();

            try
            {
                // Act
                await _apiClient.UpdateOrder(orderId, new UpdateOrderRequest
                {
                    Status = OrderStatus.OrderAccepted
                }, admin.Token);

                Assert.Fail("Should have thrown FlurlHttpException");
            }
            catch (FlurlHttpException ex)
            {
                // Assert
                ex.Call.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

                var responseContent = await ex.GetResponseJsonAsync<ErrorResponse>();

                responseContent.Should().NotBeNull();
                responseContent.Errors.Should().HaveCount(1);
                responseContent.Errors.First().Should().Be($"Order with ID '{orderId}' not found.");
            }
        }
    }
}
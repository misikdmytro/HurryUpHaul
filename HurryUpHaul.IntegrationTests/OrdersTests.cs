using System.Net;
using System.Net.Mime;
using System.Text;

using FluentAssertions;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Mvc.Testing;

using Newtonsoft.Json;

namespace HurryUpHaul.IntegrationTests
{
    public class OrdersTests : Base
    {
        public OrdersTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateAndGetOrderShouldDoIt()
        {
            // 1. create order
            var token = await CreateTestUser();

            var client = _factory.CreateClient();
            var createOrderRequest = new CreateOrderRequest
            {
                Details = _faker.Lorem.Sentence()
            };
            using var createOrderHttpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders");
            createOrderHttpRequest.Content = new StringContent(JsonConvert.SerializeObject(createOrderRequest), Encoding.UTF8, MediaTypeNames.Application.Json);
            createOrderHttpRequest.Headers.Add("Authorization", $"Bearer {token}");

            using var createOrderResponse = await client.SendAsync(createOrderHttpRequest);

            createOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            createOrderResponse.Headers.Location.Should().NotBeNull();

            var responseContent = await createOrderResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Id.Should().NotBeNullOrEmpty();

            // 2. get order
            using var getOrderHttpRequest = new HttpRequestMessage(HttpMethod.Get, createOrderResponse.Headers.Location);
            getOrderHttpRequest.Headers.Add("Authorization", $"Bearer {token}");

            using var getOrderResponse = await client.SendAsync(getOrderHttpRequest);

            getOrderResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var getOrderResponseContent = await getOrderResponse.Content.ReadFromJsonAsync<GetOrderResponse>();

            getOrderResponseContent.Should().NotBeNull();
            getOrderResponseContent.Order.Should().NotBeNull();
            getOrderResponseContent.Order.Id.Should().Be(responseContent.Id);
            getOrderResponseContent.Order.Details.Should().Be(createOrderRequest.Details);
            getOrderResponseContent.Order.CreatedBy.Should().NotBeNullOrEmpty();
            getOrderResponseContent.Order.CreatedAt.Should().NotBe(default);
            getOrderResponseContent.Order.LastUpdatedAt.Should().NotBe(default);
            getOrderResponseContent.Order.Status.Should().Be("Created");
        }

        [Fact]
        public async Task GetOrderShouldFaileForNonCreator()
        {
            // 1. create order
            var token1 = await CreateTestUser();

            var client = _factory.CreateClient();
            var createOrderRequest = new CreateOrderRequest
            {
                Details = _faker.Lorem.Sentence()
            };
            using var createOrderHttpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders");
            createOrderHttpRequest.Content = new StringContent(JsonConvert.SerializeObject(createOrderRequest), Encoding.UTF8, MediaTypeNames.Application.Json);
            createOrderHttpRequest.Headers.Add("Authorization", $"Bearer {token1}");

            using var createOrderResponse = await client.SendAsync(createOrderHttpRequest);

            createOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            createOrderResponse.Headers.Location.Should().NotBeNull();

            var responseContent = await createOrderResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Id.Should().NotBeNullOrEmpty();

            // 2. get order
            var token2 = await CreateTestUser();

            using var getOrderHttpRequest = new HttpRequestMessage(HttpMethod.Get, createOrderResponse.Headers.Location);
            getOrderHttpRequest.Headers.Add("Authorization", $"Bearer {token2}");

            using var getOrderResponse = await client.SendAsync(getOrderHttpRequest);

            getOrderResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var getOrderResponseContent = await getOrderResponse.Content.ReadFromJsonAsync<ErrorResponse>();

            getOrderResponseContent.Should().NotBeNull();
            getOrderResponseContent.Errors.Should().HaveCount(1);

            getOrderResponseContent.Errors.First().Should().Be($"Order with ID '{responseContent.Id}' not found.");
        }


        [Fact]
        public async Task GetOrderShouldReturnNotFound()
        {
            // Arrange
            var token = await CreateTestUser();

            var client = _factory.CreateClient();
            var id = Guid.NewGuid();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{id}");
            httpRequest.Headers.Add("Authorization", $"Bearer {token}");

            // Act
            using var response = await client.SendAsync(httpRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var getOrderResponseContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            getOrderResponseContent.Should().NotBeNull();
            getOrderResponseContent.Errors.Should().HaveCount(1);

            getOrderResponseContent.Errors.First().Should().Be($"Order with ID '{id}' not found.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CreateOrderShouldReturnBadRequestWhenDetailsIsNullOrEmpty(string details)
        {
            // Arrange
            var token = await CreateTestUser();

            var client = _factory.CreateClient();
            var request = new CreateOrderRequest
            {
                Details = details
            };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders");
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);
            httpRequest.Headers.Add("Authorization", $"Bearer {token}");

            // Act
            using var response = await client.SendAsync(httpRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().HaveCount(1);
            responseContent.Errors.First().Should().Be("'Details' must not be empty.");
        }

        [Fact]
        public async Task CreateOrderShouldReturnUnauthorizedWhenNotAuthenticated()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new CreateOrderRequest
            {
                Details = _faker.Lorem.Sentence()
            };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders");
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);

            // Act
            using var response = await client.SendAsync(httpRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
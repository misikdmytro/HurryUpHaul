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
        public async Task CreateOrderShouldDoIt()
        {
            // Arrange
            var token = await CreateTestUser();

            var client = _factory.CreateClient();
            var request = new CreateOrderRequest
            {
                Details = _faker.Lorem.Sentence()
            };
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/orders");
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);
            httpRequest.Headers.Add("Authorization", $"Bearer {token}");

            // Act
            using var response = await client.SendAsync(httpRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var responseContent = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Id.Should().NotBeNullOrEmpty();
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
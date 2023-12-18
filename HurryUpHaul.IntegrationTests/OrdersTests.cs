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
    public class OrdersTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Faker _faker;

        public OrdersTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _faker = new Faker();
        }

        [Fact]
        public async Task CreateOrderShouldDoIt()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new CreateOrderRequest
            {
                Details = _faker.Lorem.Sentence()
            };
            using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);

            // Act
            using var response = await client.PostAsync("api/orders", content);

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
            var client = _factory.CreateClient();
            var request = new CreateOrderRequest
            {
                Details = details
            };
            using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);

            // Act
            using var response = await client.PostAsync("api/orders", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var responseContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().HaveCount(1);
            responseContent.Errors.First().Should().Be("'Details' must not be empty.");
        }
    }
}
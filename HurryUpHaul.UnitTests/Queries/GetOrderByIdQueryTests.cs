using FluentAssertions;

using HurryUpHaul.Domain.Models.Database;
using HurryUpHaul.Domain.Queries;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using Moq;

namespace HurryUpHaul.UnitTests.Queries
{
    public class GetOrderByIdQueryTests : Base
    {
        private readonly IRequestHandler<GetOrderByIdQuery, GetOrderByIdQueryResult> _handler;

        public GetOrderByIdQueryTests() : base()
        {
            _handler = new GetOrderByIdQueryHandler(_appDbContext, _mapper, Mock.Of<ILogger<GetOrderByIdQueryHandler>>());
        }

        [Fact]
        public async Task HandleShouldReturnEmptyResultIfOrderDoesNotExist()
        {
            // Arrange
            var query = new GetOrderByIdQuery
            {
                OrderId = Guid.NewGuid().ToString(),
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Order.Should().BeNull();
            result.Result.Should().Be(GetOrderByIdQueryResultType.OrderNotFound);
            result.RestaurantManagers.Should().BeNull();
        }

        [Fact]
        public async Task HandleShouldReturnOrder()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                CreatedBy = _faker.Person.UserName,
                CreatedAt = _faker.Date.RecentOffset(),
                Details = _faker.Lorem.Sentence(),
                LastUpdatedAt = _faker.Date.RecentOffset(),
                Status = OrderStatus.OrderAccepted,
                Restaurant = new Restaurant
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = _faker.Company.CompanyName(),
                    Managers = new List<IdentityUser>
                    {
                        new()
                        {
                            UserName = _faker.Person.UserName
                        }
                    }
                }
            };

            _appDbContext.Orders.Add(order);
            await _appDbContext.SaveChangesAsync();

            var query = new GetOrderByIdQuery
            {
                OrderId = order.Id,
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().Be(GetOrderByIdQueryResultType.Success);
            result.Order.Should().NotBeNull();
            result.Order.Id.Should().Be(order.Id);
            result.Order.Details.Should().Be(order.Details);
            result.Order.Status.Should().Be((Contracts.Models.OrderStatus)order.Status);
            result.Order.CreatedBy.Should().Be(order.CreatedBy);
            result.Order.CreatedAt.Should().Be(order.CreatedAt);
            result.Order.LastUpdatedAt.Should().Be(order.LastUpdatedAt);
            result.RestaurantManagers.Should().BeEquivalentTo(order.Restaurant.Managers.Select(m => m.UserName));
        }
    }
}
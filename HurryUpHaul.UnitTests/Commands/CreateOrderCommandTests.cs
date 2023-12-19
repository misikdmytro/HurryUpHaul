using FluentAssertions;

using HurryUpHaul.Domain.Commands;
using HurryUpHaul.Domain.Helpers;
using HurryUpHaul.Domain.Models.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

namespace HurryUpHaul.UnitTests.Commands
{
    public class CreateOrderCommandTests : Base
    {
        private readonly Mock<IDateTimeProvider> _dateTimeProvider;

        private readonly IRequestHandler<CreateOrderCommand, CreateOrderCommandResult> _handler;

        public CreateOrderCommandTests() : base()
        {
            _dateTimeProvider = new Mock<IDateTimeProvider>();

            _handler = new CreateOrderCommandHandler(_appDbContext, _dateTimeProvider.Object, Mock.Of<ILogger<CreateOrderCommandHandler>>());
        }

        [Fact]
        public async Task HandleShouldCreateOrder()
        {
            // Arrange
            var restaraunt = new Restaurant
            {
                Id = Guid.NewGuid(),
                Name = _faker.Company.CompanyName()
            };

            _appDbContext.Restaurants.Add(restaraunt);
            await _appDbContext.SaveChangesAsync();

            var command = new CreateOrderCommand
            {
                RestaurantId = restaraunt.Id,
                Customer = _faker.Person.UserName,
                OrderDetails = _faker.Lorem.Sentence()
            };

            var now = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _dateTimeProvider.SetupGet(d => d.Now).Returns(now);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();

            var order = await _appDbContext.Orders.SingleAsync(o => o.Id == result.OrderId);

            order.Details.Should().Be(command.OrderDetails);
            order.Status.Should().Be(OrderStatus.Created);
            order.CreatedBy.Should().Be(command.Customer);
            order.CreatedAt.Should().Be(now);
            order.LastUpdatedAt.Should().Be(now);
        }
    }
}
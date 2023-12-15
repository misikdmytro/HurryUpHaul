using Bogus;

using FluentAssertions;

using HurryUpHaul.Domain.Commands;
using HurryUpHaul.Domain.Constants;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Helpers;
using HurryUpHaul.Domain.Models.Database;
using HurryUpHaul.Domain.Models.Events;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace HurryUpHaul.UnitTests.Commands
{
    public class CreateOrderCommandTests : IAsyncDisposable
    {
        private readonly AppDbContext _appDbContext;
        private readonly Mock<IDateTimeProvider> _dateTimeProvider;

        private readonly IRequestHandler<CreateOrderCommand, CreateOrderCommandResult> _handler;

        private readonly Faker _faker;

        public CreateOrderCommandTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "HurryUpHaul")
                .Options;

            _appDbContext = new AppDbContext(options);

            _dateTimeProvider = new Mock<IDateTimeProvider>();

            _handler = new CreateOrderCommandHandler(_appDbContext, _dateTimeProvider.Object);

            _faker = new Faker();
        }

        [Fact]
        public async Task HandleShouldCreateOrder()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                OrderDetails = _faker.Lorem.Sentence()
            };

            var now = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _dateTimeProvider.SetupGet(d => d.Now).Returns(now);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result.OrderId, out var orderId).Should().BeTrue();

            var order = await _appDbContext.Orders
                .Include(o => o.Events)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            order.Should().NotBeNull();
            order.Details.Should().Be(command.OrderDetails);
            order.Status.Should().Be(OrderStatus.Created);
            order.CreatedAt.Should().Be(now);
            order.LastUpdatedAt.Should().Be(now);
            order.Events.Should().HaveCount(1);

            var @event = order.Events.First();

            @event.EventType.Should().Be(EventTypes.OrderCreated);
            @event.OrderId.Should().Be(orderId);
            @event.EventTime.Should().Be(now);
            @event.Payload.Should().Be(new OrderCreatedEvent
            {
                Details = command.OrderDetails
            });
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }


        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                await _appDbContext.DisposeAsync();
            }
        }
    }
}
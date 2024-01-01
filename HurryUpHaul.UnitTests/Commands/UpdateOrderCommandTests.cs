using FluentAssertions;

using HurryUpHaul.Domain.Commands;
using HurryUpHaul.Domain.Helpers;
using HurryUpHaul.Domain.Models.Database;

using MediatR;

using Microsoft.AspNetCore.Identity;

using Microsoft.Extensions.Logging;

using Moq;

namespace HurryUpHaul.UnitTests.Commands
{
    public class UpdateOrderCommandTests : Base
    {
        private readonly Mock<IDateTimeProvider> _dateTimeProvider;

        private readonly IRequestHandler<UpdateOrderCommand, UpdateOrderCommandResult> _handler;

        public UpdateOrderCommandTests() : base()
        {
            _dateTimeProvider = new Mock<IDateTimeProvider>();

            _handler = new UpdateOrderCommandHandler(_appDbContext,
                _dateTimeProvider.Object,
                Mock.Of<ILogger<UpdateOrderCommandHandler>>()
            );
        }

        [Fact]
        public async Task HandleShouldReturnOrderNotFoundIfOrderDoesNotExist()
        {
            // Arrange
            var command = new UpdateOrderCommand
            {
                OrderId = Guid.NewGuid().ToString(),
                Status = Contracts.Models.OrderStatus.Created,
                Username = _faker.Person.UserName,
                IsAdmin = false
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().Be(UpdateOrderCommandResultType.OrderNotFound);
        }

        [Fact]
        public async Task HandleShouldReturnNotAuthorizedIfUserIsNotAdminAndNotTheCreatorOfTheOrder()
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
                        new() {
                            UserName = _faker.Person.UserName
                        }
                    }
                }
            };

            _appDbContext.Orders.Add(order);
            await _appDbContext.SaveChangesAsync();

            var command = new UpdateOrderCommand
            {
                OrderId = order.Id,
                Status = Contracts.Models.OrderStatus.Cancelled,
                Username = _faker.Database.Random.Uuid().ToString(),
                IsAdmin = false
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().Be(UpdateOrderCommandResultType.Forbidden);
        }

        [Theory]
        // convert to int because OrderStatus is not visible outside of the domain  
        [InlineData((int)OrderStatus.Created, Contracts.Models.OrderStatus.InProgress)]
        [InlineData((int)OrderStatus.Created, Contracts.Models.OrderStatus.WaitingDelivery)]
        [InlineData((int)OrderStatus.Created, Contracts.Models.OrderStatus.Delivering)]
        [InlineData((int)OrderStatus.Created, Contracts.Models.OrderStatus.Completed)]
        [InlineData((int)OrderStatus.OrderAccepted, Contracts.Models.OrderStatus.Created)]
        [InlineData((int)OrderStatus.OrderAccepted, Contracts.Models.OrderStatus.WaitingDelivery)]
        [InlineData((int)OrderStatus.OrderAccepted, Contracts.Models.OrderStatus.Delivering)]
        [InlineData((int)OrderStatus.OrderAccepted, Contracts.Models.OrderStatus.Completed)]
        [InlineData((int)OrderStatus.InProgress, Contracts.Models.OrderStatus.Created)]
        [InlineData((int)OrderStatus.InProgress, Contracts.Models.OrderStatus.OrderAccepted)]
        [InlineData((int)OrderStatus.InProgress, Contracts.Models.OrderStatus.Delivering)]
        [InlineData((int)OrderStatus.InProgress, Contracts.Models.OrderStatus.Completed)]
        [InlineData((int)OrderStatus.WaitingDelivery, Contracts.Models.OrderStatus.Created)]
        [InlineData((int)OrderStatus.WaitingDelivery, Contracts.Models.OrderStatus.OrderAccepted)]
        [InlineData((int)OrderStatus.WaitingDelivery, Contracts.Models.OrderStatus.InProgress)]
        [InlineData((int)OrderStatus.WaitingDelivery, Contracts.Models.OrderStatus.Completed)]
        [InlineData((int)OrderStatus.Delivering, Contracts.Models.OrderStatus.Created)]
        [InlineData((int)OrderStatus.Delivering, Contracts.Models.OrderStatus.OrderAccepted)]
        [InlineData((int)OrderStatus.Delivering, Contracts.Models.OrderStatus.InProgress)]
        [InlineData((int)OrderStatus.Delivering, Contracts.Models.OrderStatus.WaitingDelivery)]
        [InlineData((int)OrderStatus.Completed, Contracts.Models.OrderStatus.Created)]
        [InlineData((int)OrderStatus.Completed, Contracts.Models.OrderStatus.OrderAccepted)]
        [InlineData((int)OrderStatus.Completed, Contracts.Models.OrderStatus.InProgress)]
        [InlineData((int)OrderStatus.Completed, Contracts.Models.OrderStatus.WaitingDelivery)]
        [InlineData((int)OrderStatus.Completed, Contracts.Models.OrderStatus.Delivering)]
        [InlineData((int)OrderStatus.Completed, Contracts.Models.OrderStatus.Cancelled)]
        [InlineData((int)OrderStatus.Cancelled, Contracts.Models.OrderStatus.Created)]
        [InlineData((int)OrderStatus.Cancelled, Contracts.Models.OrderStatus.OrderAccepted)]
        [InlineData((int)OrderStatus.Cancelled, Contracts.Models.OrderStatus.InProgress)]
        [InlineData((int)OrderStatus.Cancelled, Contracts.Models.OrderStatus.WaitingDelivery)]
        [InlineData((int)OrderStatus.Cancelled, Contracts.Models.OrderStatus.Delivering)]
        [InlineData((int)OrderStatus.Cancelled, Contracts.Models.OrderStatus.Completed)]
        public async Task HandleShouldReturnWrongOrderStatusIfPromotionForbidden(int initial,
            Contracts.Models.OrderStatus final)
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                CreatedBy = _faker.Person.UserName,
                CreatedAt = _faker.Date.RecentOffset(),
                Details = _faker.Lorem.Sentence(),
                LastUpdatedAt = _faker.Date.RecentOffset(),
                Status = (OrderStatus)initial,
                Restaurant = new Restaurant
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = _faker.Company.CompanyName(),
                    Managers = new List<IdentityUser>
                    {
                        new() {
                            UserName = _faker.Person.UserName
                        }
                    }
                }
            };

            _appDbContext.Orders.Add(order);
            await _appDbContext.SaveChangesAsync();

            var command = new UpdateOrderCommand
            {
                OrderId = order.Id,
                Status = final,
                Username = _faker.Person.UserName,
                IsAdmin = true
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().Be(UpdateOrderCommandResultType.WrongOrderStatus);
        }

        [Theory]
        // convert to int because OrderStatus is not visible outside of the domain
        [InlineData((int)OrderStatus.Created, Contracts.Models.OrderStatus.Cancelled)]
        [InlineData((int)OrderStatus.Created, Contracts.Models.OrderStatus.OrderAccepted)]
        [InlineData((int)OrderStatus.OrderAccepted, Contracts.Models.OrderStatus.Cancelled)]
        [InlineData((int)OrderStatus.OrderAccepted, Contracts.Models.OrderStatus.InProgress)]
        [InlineData((int)OrderStatus.InProgress, Contracts.Models.OrderStatus.Cancelled)]
        [InlineData((int)OrderStatus.InProgress, Contracts.Models.OrderStatus.WaitingDelivery)]
        [InlineData((int)OrderStatus.WaitingDelivery, Contracts.Models.OrderStatus.Cancelled)]
        [InlineData((int)OrderStatus.WaitingDelivery, Contracts.Models.OrderStatus.Delivering)]
        [InlineData((int)OrderStatus.Delivering, Contracts.Models.OrderStatus.Cancelled)]
        [InlineData((int)OrderStatus.Delivering, Contracts.Models.OrderStatus.Completed)]
        public async Task HandleShouldUpdateOrderStatus(int initial, Contracts.Models.OrderStatus final)
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                CreatedBy = _faker.Person.UserName,
                CreatedAt = _faker.Date.RecentOffset(),
                Details = _faker.Lorem.Sentence(),
                LastUpdatedAt = _faker.Date.RecentOffset(),
                Status = (OrderStatus)initial,
                Restaurant = new Restaurant
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = _faker.Company.CompanyName(),
                    Managers = new List<IdentityUser>
                    {
                        new() {
                            UserName = _faker.Person.UserName
                        }
                    }
                }
            };

            _appDbContext.Orders.Add(order);
            await _appDbContext.SaveChangesAsync();

            var command = new UpdateOrderCommand
            {
                OrderId = order.Id,
                Status = final,
                Username = _faker.Person.UserName,
                IsAdmin = true
            };

            var now = _faker.Date.RecentOffset();
            _dateTimeProvider.Setup(d => d.Now).Returns(now);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().Be(UpdateOrderCommandResultType.Success);
            order.Status.Should().Be((OrderStatus)final);
            order.LastUpdatedAt.Should().Be(now);
        }
    }
}
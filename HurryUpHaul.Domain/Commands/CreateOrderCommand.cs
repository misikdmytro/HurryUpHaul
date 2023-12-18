using HurryUpHaul.Domain.Constants;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;
using HurryUpHaul.Domain.Helpers;
using HurryUpHaul.Domain.Models.Database;
using HurryUpHaul.Domain.Models.Events;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Commands
{
    public class CreateOrderCommand : IRequest<CreateOrderCommandResult>
    {
        public string OrderDetails { get; init; }
    }

    public class CreateOrderCommandResult
    {
        public string OrderId { get; init; }
    }

    internal class CreateOrderCommandHandler : BaseHandler<CreateOrderCommand, CreateOrderCommandResult>
    {
        private readonly AppDbContext _dbContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateOrderCommandHandler(AppDbContext dbContext,
            IDateTimeProvider dateTimeProvider,
            ILogger<CreateOrderCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
        }

        protected override async Task<CreateOrderCommandResult> HandleInternal(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var orderId = Guid.NewGuid();
            var now = _dateTimeProvider.Now;

            var order = new Order
            {
                Id = orderId,
                Details = request.OrderDetails,
                Status = OrderStatus.Created,
                CreatedAt = now,
                LastUpdatedAt = now,
                Events = [
                    new OrderEvent
                    {
                        EventType = EventTypes.OrderCreated,
                        OrderId = orderId,
                        Payload = new OrderCreatedEvent
                        {
                            Details = request.OrderDetails
                        },
                        EventTime = _dateTimeProvider.Now
                    }
                ]
            };

            await _dbContext.Orders.AddAsync(order, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateOrderCommandResult
            {
                OrderId = order.Id.ToString()
            };
        }
    }
}
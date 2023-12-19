using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;
using HurryUpHaul.Domain.Helpers;
using HurryUpHaul.Domain.Models.Database;

using MediatR;

using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Commands
{
    public class CreateOrderCommand : IRequest<CreateOrderCommandResult>
    {
        public required Guid RestaurantId { get; init; }
        public required string Customer { get; init; }
        public required string OrderDetails { get; init; }
    }

    public class CreateOrderCommandResult
    {
        public Guid OrderId { get; init; }
        public string[] Errors { get; init; }
    }

    internal class CreateOrderCommandHandler : BaseRequestHandler<CreateOrderCommand, CreateOrderCommandResult>
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
                CreatedBy = request.Customer,
                LastUpdatedAt = now,
                RestaurantId = request.RestaurantId
            };

            await _dbContext.Orders.AddAsync(order, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateOrderCommandResult
            {
                OrderId = orderId
            };

            // ToDo: handle foreign key constraint exception
        }
    }
}
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;
using HurryUpHaul.Domain.Helpers;
using HurryUpHaul.Domain.Models.Database;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace HurryUpHaul.Domain.Commands
{
    public class CreateOrderCommand : IRequest<CreateOrderCommandResult>
    {
        public required string RestaurantId { get; init; }
        public required string Customer { get; init; }
        public required string OrderDetails { get; init; }
    }

    public enum CreateOrderCommandResultType
    {
        Success,
        RestaurantNotFound,
    }

    public class CreateOrderCommandResult
    {
        public CreateOrderCommandResultType Result { get; init; }
        public string OrderId { get; init; }
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
            try
            {
                var now = _dateTimeProvider.Now;

                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    Details = request.OrderDetails,
                    Status = OrderStatus.Created,
                    CreatedAt = now,
                    CreatedBy = request.Customer,
                    LastUpdatedAt = now,
                    RestaurantId = request.RestaurantId,
                    Version = Guid.NewGuid()
                };

                await _dbContext.Orders.AddAsync(order, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new CreateOrderCommandResult
                {
                    Result = CreateOrderCommandResultType.Success,
                    OrderId = order.Id
                };
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException sqlException && sqlException.SqlState == "23503")
            {
                // 23503 = foreign_key_violation
                return new CreateOrderCommandResult
                {
                    Result = CreateOrderCommandResultType.RestaurantNotFound,
                    Errors = [$"Restaurant with ID '{request.RestaurantId}' not found."]
                };
            }
        }
    }
}
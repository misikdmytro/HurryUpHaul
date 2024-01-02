using HurryUpHaul.Contracts.Models;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;
using HurryUpHaul.Domain.Helpers;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Polly;

namespace HurryUpHaul.Domain.Commands
{
    public class UpdateOrderCommand : IRequest<UpdateOrderCommandResult>
    {
        public required string OrderId { get; init; }
        public required OrderStatus Status { get; init; }
        public required string Username { get; set; }
        public required bool IsAdmin { get; set; }
    }

    public enum UpdateOrderCommandResultType
    {
        Success,
        OrderNotFound,
        WrongOrderStatus,
        Forbidden
    }

    public class UpdateOrderCommandResult
    {
        public UpdateOrderCommandResultType Result { get; init; }
        public string[] Errors { get; init; }
    }

    internal class UpdateOrderCommandHandler : BaseRequestHandler<UpdateOrderCommand, UpdateOrderCommandResult>
    {
        private static readonly Dictionary<Models.Database.OrderStatus, Models.Database.OrderStatus[]> AllowedStatusChanges =
            new()
            {
                { Models.Database.OrderStatus.Created, new[] { Models.Database.OrderStatus.Cancelled, Models.Database.OrderStatus.OrderAccepted } },
                { Models.Database.OrderStatus.OrderAccepted, new[] { Models.Database.OrderStatus.Cancelled, Models.Database.OrderStatus.InProgress } },
                { Models.Database.OrderStatus.InProgress, new[] { Models.Database.OrderStatus.Cancelled, Models.Database.OrderStatus.WaitingDelivery } },
                { Models.Database.OrderStatus.WaitingDelivery, new[] { Models.Database.OrderStatus.Cancelled, Models.Database.OrderStatus.Delivering } },
                { Models.Database.OrderStatus.Delivering, new[] { Models.Database.OrderStatus.Cancelled, Models.Database.OrderStatus.Completed } },
                { Models.Database.OrderStatus.Completed, Array.Empty<Models.Database.OrderStatus>() },
                { Models.Database.OrderStatus.Cancelled, Array.Empty<Models.Database.OrderStatus>() }
            };

        private readonly AppDbContext _dbContext;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UpdateOrderCommandHandler(AppDbContext dbContext,
            IDateTimeProvider dateTimeProvider,
            ILogger<UpdateOrderCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
            _dateTimeProvider = dateTimeProvider;
        }

        protected override Task<UpdateOrderCommandResult> HandleInternal(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            return Policy
                .Handle<DbUpdateConcurrencyException>()
                .RetryAsync(5, (exception, retryCount) =>
                {
                    Logger.LogWarning(exception, "Failed to update order with ID '{OrderId}' due to concurrency exception. Retrying...", request.OrderId);
                })
                .ExecuteAsync((token) => UpdateOrder(request, token), cancellationToken);
        }

        private async Task<UpdateOrderCommandResult> UpdateOrder(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Restaurant)
                .ThenInclude(r => r.Managers)
                .Where(o => o.Id == request.OrderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (order == null)
            {
                return new UpdateOrderCommandResult
                {
                    Result = UpdateOrderCommandResultType.OrderNotFound,
                    Errors = [$"Order with ID '{request.OrderId}' not found."]
                };
            }

            if (!request.IsAdmin && !order.Restaurant.Managers.Any(m => m.UserName == request.Username))
            {
                return new UpdateOrderCommandResult
                {
                    Result = UpdateOrderCommandResultType.Forbidden,
                    Errors = [$"User '{request.Username}' is not authorized to update order with ID '{request.OrderId}'."]
                };
            }

            var newStatus = (Models.Database.OrderStatus)request.Status;
            if (!AllowedStatusChanges.TryGetValue(order.Status, out var allowedStatuses) ||
                !allowedStatuses.Contains(newStatus))
            {
                return new UpdateOrderCommandResult
                {
                    Result = UpdateOrderCommandResultType.WrongOrderStatus,
                    Errors = [$"Order with ID '{request.OrderId}' cannot be updated to status '{request.Status}'."]
                };
            }

            order.Status = newStatus;
            order.LastUpdatedAt = _dateTimeProvider.Now;
            order.Version = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateOrderCommandResult
            {
                Result = UpdateOrderCommandResultType.Success
            };
        }
    }
}
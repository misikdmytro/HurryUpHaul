using AutoMapper;

using HurryUpHaul.Contracts.Models;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Queries
{
    public class GetOrderByIdQuery : IRequest<GetOrderByIdQueryResult>
    {
        public required string OrderId { get; init; }
    }

    public enum GetOrderByIdQueryResultType
    {
        Success,
        OrderNotFound
    }

    public class GetOrderByIdQueryResult
    {
        public GetOrderByIdQueryResultType Result { get; init; }
        public Order Order { get; init; }
        public string[] RestaurantManagers { get; init; }
        public string[] Errors { get; init; }
    }

    internal class GetOrderByIdQueryHandler : BaseRequestHandler<GetOrderByIdQuery, GetOrderByIdQueryResult>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetOrderByIdQueryHandler(AppDbContext dbContext,
            IMapper mapper,
            ILogger<GetOrderByIdQueryHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        protected override async Task<GetOrderByIdQueryResult> HandleInternal(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Restaurant)
                .ThenInclude(r => r.Managers)
                .SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            return order == null
                ? new GetOrderByIdQueryResult
                {
                    Result = GetOrderByIdQueryResultType.OrderNotFound,
                    Errors = [$"Order with ID '{request.OrderId}' not found."]
                }
                : new GetOrderByIdQueryResult
                {
                    Result = GetOrderByIdQueryResultType.Success,
                    Order = _mapper.Map<Order>(order),
                    RestaurantManagers = order.Restaurant.Managers.Select(m => m.UserName).ToArray()
                };
        }
    }
}
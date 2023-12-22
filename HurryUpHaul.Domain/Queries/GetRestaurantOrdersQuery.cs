using AutoMapper;

using HurryUpHaul.Contracts.Models;
using HurryUpHaul.Domain.Constants;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Queries
{
    public class GetRestaurantOrdersQuery : IRequest<GetRestaurantOrdersQueryResult>
    {
        public required string RestaurantId { get; init; }
        public required int PageSize { get; init; }
        public required int PageNumber { get; init; }
        public required string Requester { get; init; }
        public required string[] RequesterRoles { get; init; }
    }

    public enum GetRestaurantOrdersQueryResultType
    {
        Success,
        RestaurantNotFound,
        NoAccess
    }

    public class GetRestaurantOrdersQueryResult
    {
        public GetRestaurantOrdersQueryResultType Result { get; init; }
        public IEnumerable<Order> Orders { get; init; }
        public string[] Errors { get; set; }
    }

    internal class GetRestaurantOrdersQueryHandler : BaseRequestHandler<GetRestaurantOrdersQuery, GetRestaurantOrdersQueryResult>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetRestaurantOrdersQueryHandler(AppDbContext dbContext,
            IMapper mapper,
            ILogger<GetRestaurantOrdersQueryHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        protected override async Task<GetRestaurantOrdersQueryResult> HandleInternal(GetRestaurantOrdersQuery request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
                .Include(x => x.Managers)
                .Include(x => x.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize))
                .AsSplitQuery()
                .SingleOrDefaultAsync(x => x.Id == request.RestaurantId, cancellationToken);

            return restaurant == null
                ? new GetRestaurantOrdersQueryResult
                {
                    Result = GetRestaurantOrdersQueryResultType.RestaurantNotFound,
                    Errors = [$"Restaurant with ID '{request.RestaurantId}' not found."]
                }
                : restaurant.Managers.Any(m => m.UserName == request.Requester) != true &&
                request.RequesterRoles?.Contains(Roles.Admin) != true
                ? new GetRestaurantOrdersQueryResult
                {
                    Result = GetRestaurantOrdersQueryResultType.NoAccess,
                    Errors = ["You are not authorized to view this restaurant's orders."]
                }
                : new GetRestaurantOrdersQueryResult
                {
                    Result = GetRestaurantOrdersQueryResultType.Success,
                    Orders = _mapper.Map<IEnumerable<Order>>(restaurant.Orders)
                };
        }
    }
}
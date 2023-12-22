using AutoMapper;

using HurryUpHaul.Contracts.Models;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Queries
{
    public class GetRestaurantByIdQuery : IRequest<GetRestaurantByIdQueryResult>
    {
        public string RestaurantId { get; init; }
    }

    public enum GetRestaurantByIdQueryResultType
    {
        Success,
        RestaurantNotFound
    }

    public class GetRestaurantByIdQueryResult
    {
        public GetRestaurantByIdQueryResultType Result { get; init; }
        public Restaurant Restaurant { get; init; }
        public string[] Errors { get; set; }
    }

    internal class GetRestaurantByIdQueryHandler : BaseRequestHandler<GetRestaurantByIdQuery, GetRestaurantByIdQueryResult>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetRestaurantByIdQueryHandler(AppDbContext dbContext,
            IMapper mapper,
            ILogger<GetRestaurantByIdQueryHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        protected override async Task<GetRestaurantByIdQueryResult> HandleInternal(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
                .Include(r => r.Managers)
                .SingleOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

            return restaurant == null
                ? new GetRestaurantByIdQueryResult
                {
                    Result = GetRestaurantByIdQueryResultType.RestaurantNotFound,
                    Errors = [$"Restaurant with ID '{request.RestaurantId}' not found."]
                }
                : new GetRestaurantByIdQueryResult
                {
                    Result = GetRestaurantByIdQueryResultType.Success,
                    Restaurant = _mapper.Map<Restaurant>(restaurant)
                };
        }
    }
}
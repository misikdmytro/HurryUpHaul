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
    public class GetRestaurantByIdQuery : IRequest<GetRestaurantByIdQueryResponse>
    {
        public string RequesterUsername { get; init; }
        public string[] RequesterRoles { get; init; }
        public string RestaurantId { get; init; }
    }

    public class GetRestaurantByIdQueryResponse
    {
        public Restaurant Restaurant { get; init; }
    }

    internal class GetRestaurantByIdQueryHandler : BaseRequestHandler<GetRestaurantByIdQuery, GetRestaurantByIdQueryResponse>
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

        protected override async Task<GetRestaurantByIdQueryResponse> HandleInternal(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
        {
            var restaurant = await _dbContext.Restaurants
                .Include(r => r.Managers)
                .SingleOrDefaultAsync(r => r.Id == request.RestaurantId, cancellationToken);

            if (restaurant == null)
            {
                return new GetRestaurantByIdQueryResponse();
            }

            if (request.RequesterRoles?.Contains(Roles.Admin) != true && restaurant?.Managers.Any(m => m.UserName == request.RequesterUsername) != true)
            {
                // do not return all details if the user is not an admin or a manager of the restaurant
                restaurant.Managers = null;
                restaurant.CreatedAt = default;
            }

            return new GetRestaurantByIdQueryResponse
            {
                Restaurant = _mapper.Map<Restaurant>(restaurant)
            };
        }
    }
}
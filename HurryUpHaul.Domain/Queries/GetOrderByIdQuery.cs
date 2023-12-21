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
    public class GetOrderByIdQuery : IRequest<GetOrderByIdQueryResponse>
    {
        public required string Requester { get; init; }
        public required string[] RequesterRoles { get; init; }
        public required string OrderId { get; init; }
    }

    public class GetOrderByIdQueryResponse
    {
        public Order Order { get; init; }
    }

    internal class GetOrderByIdQueryHandler : BaseRequestHandler<GetOrderByIdQuery, GetOrderByIdQueryResponse>
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

        protected override async Task<GetOrderByIdQueryResponse> HandleInternal(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Restaurant)
                .ThenInclude(r => r.Managers)
                .SingleOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            return order == null ||
                (order.CreatedBy != request.Requester &&
                order.Restaurant.Managers.Any(m => m.UserName == request.Requester) != true &&
                request.RequesterRoles?.Contains(Roles.Admin) != true)
                ? new GetOrderByIdQueryResponse()
                : new GetOrderByIdQueryResponse
                {
                    Order = _mapper.Map<Order>(order)
                };
        }
    }
}
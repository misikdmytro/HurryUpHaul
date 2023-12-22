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
    public class GetOrderByIdQuery : IRequest<GetOrderByIdQueryResult>
    {
        public required string Requester { get; init; }
        public required string[] RequesterRoles { get; init; }
        public required string OrderId { get; init; }
    }

    public class GetOrderByIdQueryResult
    {
        public Order Order { get; init; }
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

            return order == null ||
                (order.CreatedBy != request.Requester &&
                order.Restaurant.Managers.Any(m => m.UserName == request.Requester) != true &&
                request.RequesterRoles?.Contains(Roles.Admin) != true)
                ? new GetOrderByIdQueryResult()
                : new GetOrderByIdQueryResult
                {
                    Order = _mapper.Map<Order>(order)
                };
        }
    }
}
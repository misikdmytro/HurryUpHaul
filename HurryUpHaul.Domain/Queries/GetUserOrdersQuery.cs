using AutoMapper;

using HurryUpHaul.Contracts.Models;
using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Queries
{
    public class GetUserOrdersQuery : IRequest<GetUserOrdersQueryResult>
    {
        public required string Username { get; init; }
        public required int PageSize { get; init; }
        public required int PageNumber { get; init; }
    }

    public class GetUserOrdersQueryResult
    {
        public IEnumerable<Order> Orders { get; init; }
        public string[] Errors { get; set; }
    }

    internal class GetUserOrdersQueryHandler : BaseRequestHandler<GetUserOrdersQuery, GetUserOrdersQueryResult>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetUserOrdersQueryHandler(AppDbContext dbContext,
            IMapper mapper,
            ILogger<GetUserOrdersQueryHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        protected override async Task<GetUserOrdersQueryResult> HandleInternal(GetUserOrdersQuery request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .Where(x => x.CreatedBy == request.Username)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new GetUserOrdersQueryResult
            {
                Orders = _mapper.Map<IEnumerable<Order>>(order)
            };
        }
    }
}
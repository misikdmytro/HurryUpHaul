using HurryUpHaul.Domain.Databases;
using HurryUpHaul.Domain.Handlers;
using HurryUpHaul.Domain.Helpers;
using HurryUpHaul.Domain.Models.Database;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Commands
{
    public class CreateRestaurantCommand : IRequest<CreateRestaurantCommandResult>
    {
        public string Name { get; init; }
        public string[] ManagersIds { get; init; }
    }

    public enum CreateRestaurantCommandResultType
    {
        Success,
        ManagersNotFound,
    }

    public class CreateRestaurantCommandResult
    {
        public CreateRestaurantCommandResultType Result { get; init; }
        public string RestaurantId { get; init; }
        public string[] Errors { get; init; }
    }

    internal class CreateRestaurantCommandHandler : BaseRequestHandler<CreateRestaurantCommand, CreateRestaurantCommandResult>
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateRestaurantCommandHandler(AppDbContext dbContext,
            UserManager<IdentityUser> userManager,
            IDateTimeProvider dateTimeProvider,
            ILogger<CreateRestaurantCommandHandler> logger) : base(logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _dateTimeProvider = dateTimeProvider;
        }

        protected override async Task<CreateRestaurantCommandResult> HandleInternal(CreateRestaurantCommand request, CancellationToken cancellationToken)
        {
            var managers = await _userManager.Users.Where(u => request.ManagersIds.Contains(u.Id)).ToListAsync(cancellationToken);
            if (managers.Count != request.ManagersIds.Length)
            {
                return new CreateRestaurantCommandResult
                {
                    Result = CreateRestaurantCommandResultType.ManagersNotFound,
                    Errors = ["One or more managers were not found."]
                };
            }

            var now = _dateTimeProvider.Now;
            var restaurant = new Restaurant
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                CreatedAt = now,
                Managers = managers
            };

            await _dbContext.Restaurants.AddAsync(restaurant, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateRestaurantCommandResult
            {
                Result = CreateRestaurantCommandResultType.Success,
                RestaurantId = restaurant.Id
            };
        }
    }
}
using HurryUpHaul.Domain.Handlers;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Commands
{
    public class RegisterUserCommand : IRequest<RegisterUserCommandResult>
    {
        public string Username { get; init; }
        public string Password { get; init; }
    }

    public class RegisterUserCommandResult
    {
        public bool Success { get; init; }
        public IEnumerable<IdentityError> Errors { get; init; }
    }

    internal class RegisterUserCommandHandler : BaseHandler<RegisterUserCommand, RegisterUserCommandResult>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public RegisterUserCommandHandler(UserManager<IdentityUser> userManager,
            ILogger<RegisterUserCommandHandler> logger) : base(logger)
        {
            _userManager = userManager;
        }

        protected override async Task<RegisterUserCommandResult> HandleInternal(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new IdentityUser
            {
                UserName = request.Username
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            return new RegisterUserCommandResult
            {
                Success = result.Succeeded,
                Errors = result.Errors
            };
        }
    }
}
using HurryUpHaul.Domain.Constants;
using HurryUpHaul.Domain.Handlers;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Commands
{
    public class RegisterUserCommand : IRequest<RegisterUserCommandResult>
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }

    public enum RegisterUserCommandResultType
    {
        Success,
        RegistrationFailed
    }

    public class RegisterUserCommandResult
    {
        public RegisterUserCommandResultType Result { get; init; }
        public string UserId { get; init; }
        public string[] Errors { get; init; }
    }

    internal class RegisterUserCommandHandler : BaseRequestHandler<RegisterUserCommand, RegisterUserCommandResult>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterUserCommandHandler(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterUserCommandHandler> logger) : base(logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override async Task<RegisterUserCommandResult> HandleInternal(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new IdentityUser
            {
                UserName = request.Username
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new RegisterUserCommandResult
                {
                    Result = RegisterUserCommandResultType.RegistrationFailed,
                    Errors = result.Errors.Select(x => x.Description).ToArray()
                };
            }

            if (!await _roleManager.RoleExistsAsync(Roles.User))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.User));
            }

            result = await _userManager.AddToRoleAsync(user, Roles.User);

            return new RegisterUserCommandResult
            {
                Result = RegisterUserCommandResultType.Success,
                UserId = user.Id,
                Errors = result.Errors?.Select(x => x.Description).ToArray()
            };
        }
    }
}
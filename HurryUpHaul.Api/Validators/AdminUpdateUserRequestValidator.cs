using FluentValidation;

using HurryUpHaul.Api.Extensions;
using HurryUpHaul.Contracts.Http;
using HurryUpHaul.Domain.Constants;

namespace HurryUpHaul.Api.Validators
{
    internal class AdminUpdateUserRequestValidator : AbstractValidator<AdminUpdateUserRequest>
    {
        public static string[] AvailableRoles = [Roles.User, Roles.Admin];

        public AdminUpdateUserRequestValidator()
        {
            this.RuleForUsername(x => x.Username);

            RuleFor(x => x.Roles)
                .NotNull()
                .NotEmpty()
                .Must(x => x == null || x.All(c => AvailableRoles.Contains(c.Role)))
                .WithMessage($"Roles must only contain the following roles: {string.Join(", ", AvailableRoles)}.")
                .Must(x => x == null || x.GroupBy(c => c.Role).All(c => c.Count() == 1))
                .WithMessage($"Roles must not contain duplicate roles.");
        }
    }
}
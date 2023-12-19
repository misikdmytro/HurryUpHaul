using FluentValidation;

using HurryUpHaul.Api.Extensions;
using HurryUpHaul.Contracts.Http;

namespace HurryUpHaul.Api.Validators
{
    internal class AuthenticateUserRequestValidator : AbstractValidator<AuthenticateUserRequest>
    {
        public AuthenticateUserRequestValidator()
        {
            this.RuleForUsername(x => x.Username);
            this.RuleForPassword(x => x.Password);
        }
    }
}
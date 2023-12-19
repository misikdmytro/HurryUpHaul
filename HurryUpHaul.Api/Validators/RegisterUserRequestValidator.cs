using FluentValidation;

using HurryUpHaul.Api.Extensions;
using HurryUpHaul.Contracts.Http;

namespace HurryUpHaul.Api.Validators
{
    internal class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            this.RuleForUsername(x => x.Username);
            this.RuleForPassword(x => x.Password);
        }
    }
}
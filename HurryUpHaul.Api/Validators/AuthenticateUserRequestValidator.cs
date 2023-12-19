using FluentValidation;

using HurryUpHaul.Contracts.Http;

namespace HurryUpHaul.Api.Validators
{
    internal class AuthenticateUserRequestValidator : AbstractValidator<AuthenticateUserRequest>
    {
        public const string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        public AuthenticateUserRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MaximumLength(256)
                .Must(x => x?.All(c => AllowedUserNameCharacters.Contains(c)) is not false)
                .WithMessage($"Username must only contain the following characters: {AllowedUserNameCharacters}");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
}
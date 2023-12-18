using FluentValidation;


using HurryUpHaul.Contracts.Http;


namespace HurryUpHaul.Api.Validators
{
    internal class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.Details)
                .NotEmpty()
                .MaximumLength(2000);
        }
    }
}
using FluentValidation;

using HurryUpHaul.Contracts.Http;

namespace HurryUpHaul.Api.Validators
{
    internal class CreateRestaurantRequestValidator : AbstractValidator<CreateRestaurantRequest>
    {
        public CreateRestaurantRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(x => x.ManagersIds)
                .NotEmpty()
                .Must(x => x == null || x.All(c => c != null && c.Length > 0))
                .WithMessage($"Managers Ids must not contain null or empty values")
                .Must(x => x == null || x.GroupBy(c => c).All(c => c.Count() == 1))
                .WithMessage($"Managers Ids must not contain duplicate values")
                .Must(x => x == null || x.Length <= 10)
                .WithMessage($"Managers Ids must not contain more than 10 values");
        }
    }
}
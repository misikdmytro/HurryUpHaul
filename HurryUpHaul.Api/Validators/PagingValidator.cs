using FluentValidation;

using HurryUpHaul.Contracts.Models;

namespace HurryUpHaul.Api.Validators
{
    internal class PagingValidator : AbstractValidator<Paging>
    {
        private const int MaxPageSize = 1000;

        public PagingValidator()
        {
            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(MaxPageSize);

            RuleFor(x => x.PageNumber)
                .GreaterThan(0);
        }
    }
}
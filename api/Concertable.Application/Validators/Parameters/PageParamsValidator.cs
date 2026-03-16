using Core.Interfaces;
using FluentValidation;

namespace Application.Validators.Parameters;

public class PageParamsValidator : AbstractValidator<IPageParams>
{
    public PageParamsValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

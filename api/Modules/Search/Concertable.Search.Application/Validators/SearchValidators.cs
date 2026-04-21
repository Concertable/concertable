using Concertable.Application.Validators.Parameters;
using FluentValidation;

namespace Concertable.Search.Application.Validators;

internal class SearchParamsValidator : AbstractValidator<SearchParams>
{
    public SearchParamsValidator()
    {
        Include(new PageParamsValidator());
        Include(new GeoParamsValidator());
        RuleFor(x => x.HeaderType).NotNull();
    }
}

internal class ConcertParamsValidator : AbstractValidator<ConcertParams>
{
    public ConcertParamsValidator()
    {
        Include(new GeoParamsValidator());
        RuleFor(x => x.Take).GreaterThan(0).When(x => x.Take != 0);
    }
}

using Concertable.Core.Parameters;
using FluentValidation;

namespace Concertable.Application.Validators.Parameters;

public class SearchParamsValidator : AbstractValidator<SearchParams>
{
    public SearchParamsValidator()
    {
        Include(new PageParamsValidator());
        Include(new GeoParamsValidator());
        RuleFor(x => x.HeaderType).NotNull();
    }
}

using Core.Parameters;
using FluentValidation;

namespace Application.Validators.Parameters;

public class SearchParamsValidator : AbstractValidator<SearchParams>
{
    public SearchParamsValidator()
    {
        Include(new PageParamsValidator());
        Include(new GeoParamsValidator());
    }
}

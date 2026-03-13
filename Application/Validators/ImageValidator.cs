using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Validators;

public class IFormFileValidator : AbstractValidator<IFormFile>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/webp"
    ];

    private const long MaxFileSize = 5 * 1024 * 1024;

    public IFormFileValidator()
    {
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedMimeTypes.Contains(ct))
            .WithMessage("Image must be a valid format (JPEG, PNG, GIF, BMP, WEBP).");

        RuleFor(x => x.Length)
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage("Image file exceeds the maximum size of 5MB.");
    }
}

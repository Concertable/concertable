using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

namespace Application.Responses
{
    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public IEnumerable<string> Reasons { get; set; } = Enumerable.Empty<string>();
        public string? Reason => Reasons.FirstOrDefault();

        public ValidationResponse() { }

        public ValidationResponse(bool isValid, IEnumerable<string>? reasons = null)
        {
            IsValid = isValid;
            Reasons = reasons ?? Enumerable.Empty<string>();
        }

        public ValidationResponse(IEnumerable<IdentityError> identityErrors)
        {
            IsValid = false;
            Reasons = identityErrors.Select(e => e.Description);
        }

        public static ValidationResponse Success() =>
            new ValidationResponse(true);

        public static ValidationResponse Failure(IEnumerable<string> reasons) =>
            new ValidationResponse(false, reasons);

        public static ValidationResponse Failure(params string[] reasons) =>
            new ValidationResponse(false, reasons);

        public static ValidationResponse Failure(IEnumerable<IdentityError> identityErrors) =>
            new ValidationResponse(identityErrors);
    }
}

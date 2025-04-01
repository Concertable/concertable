using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Responses
{
    public class ValidationResponse
    {
        public bool IsValid { get; set; }
        public string? Reason { get; set; }

        public ValidationResponse() { }

        public ValidationResponse(bool isValid, string? reason = null)
        {
            IsValid = isValid;
            Reason = reason;
        }

        public static ValidationResponse Success() => new(true);
        public static ValidationResponse Failure(string reason) => new(false, reason);
    }
}

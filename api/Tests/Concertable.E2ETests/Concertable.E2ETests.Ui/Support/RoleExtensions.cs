using System.Text.RegularExpressions;
using Concertable.User.Contracts;

namespace Concertable.E2ETests.Ui.Support;

internal static class RoleExtensions
{
    private static readonly Regex PascalToKebab = new("(?<!^)([A-Z])", RegexOptions.Compiled);

    internal static string ToTestId(this Role role) =>
        PascalToKebab.Replace(role.ToString(), "-$1").ToLower();
}

namespace Concertable.Application.Results;

public class ValidationResult
{
    private readonly List<string> errors = [];

    public bool IsValid => errors.Count == 0;
    public IReadOnlyList<string> Errors => errors;

    public void AddError(string message) => errors.Add(message);
}

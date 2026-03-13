namespace Application.Responses;

public class ValidationResult
{
    private readonly Dictionary<string, List<string>> _errors = [];

    public bool IsValid => _errors.Count == 0;
    public IDictionary<string, string[]> Errors =>
        _errors.ToDictionary(k => k.Key, v => v.Value.ToArray());

    public ValidationResult AddError(string message) => AddError("", message);

    public ValidationResult AddError(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var list))
            _errors[field] = list = [];
        list.Add(message);
        return this;
    }
}

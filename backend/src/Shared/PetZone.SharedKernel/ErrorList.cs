namespace PetZone.SharedKernel;

public class ErrorList
{
    private readonly List<Error> _errors;

    public ErrorList(IEnumerable<Error> errors)
    {
        _errors = errors.ToList();
    }

    public IReadOnlyList<Error> Errors => _errors.AsReadOnly();

    public static implicit operator ErrorList(Error error) => new([error]);
    public static implicit operator ErrorList(List<Error> errors) => new(errors);
}
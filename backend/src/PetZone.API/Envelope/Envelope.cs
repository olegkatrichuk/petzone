namespace PetZone.API.Envelope;

public class Envelope
{
    public object? Result { get; }
    public IReadOnlyList<ErrorInfo> ErrorInfo { get; }
    public string TimeGenerated { get; }

    private Envelope(object? result, IReadOnlyList<ErrorInfo> errorInfo)
    {
        Result = result;
        ErrorInfo = errorInfo;
        TimeGenerated = DateTime.UtcNow.ToString("dd.MM.yyyy:HH:mm:ss");
    }

    public static Envelope Ok(object? result) =>
        new(result, [new ErrorInfo(null, null, null)]);

    public static Envelope Error(IEnumerable<ErrorInfo> errors) =>
        new(null, errors.ToList());
}
